using MathNet.Numerics.Statistics;
using NLog;
using Synapse.General;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Synapse.Crypto.FastTrader
{
    public class BookAnalizer
    {

        public string rootFolder = @"D:\\Storage\\BookQuotes";

        private Logger logger = LogManager.GetCurrentClassLogger();


        private long BeforeTime = 1771212506346; // ограничитель по времени для спредов, чтобы отсечь ошибки

        private Dictionary<string, Dictionary<string, List<BestQuote>>> bbSymbols;
        private Dictionary<string, Dictionary<string, List<BestQuote>>> bfxSymbols;

        public BookAnalizer()
        {

            logger.Info("Initializing BookAnalizer...");

            var bw = new BookWriter();
            bbSymbols = bw.CreateBybitDictionary();
            bfxSymbols = bw.CreateBfxDictionary();
            rootFolder = bw.rootFolder;
        }

        public bool LoadQuotes()
        {

            try
            {

                var bbFolder = Path.Combine(rootFolder, "Bybit");

                foreach (var item in bbSymbols)
                {
                    var typeFolder = Path.Combine(bbFolder, item.Key);

                    foreach (var item2 in item.Value)
                    {
                        var file = Path.Combine(typeFolder, $"{item2.Key.Replace("/", "_")}.csv");
                        var lines = File.ReadAllLines(file);

                        List<BestQuote> quotes = [.. lines.Select(l => BestQuote.Parse(l).GetValueOrDefault())];
                        item.Value[item2.Key] = quotes;
                    }
                }

                var bfxFolder = Path.Combine(rootFolder, "Bfx");

                foreach (var item in bfxSymbols)
                {
                    var typeFolder = Path.Combine(bfxFolder, item.Key);

                    foreach (var item2 in item.Value)
                    {
                        var file = Path.Combine(typeFolder, $"{item2.Key.Replace(":", "_")}.csv");
                        var lines = File.ReadAllLines(file);

                        List<BestQuote> quotes = [.. lines.Select(l => BestQuote.Parse(l).GetValueOrDefault())];
                        item.Value[item2.Key] = quotes;
                    }
                }

                logger.Info("Loaded quotes.");

                return true;

            }
            catch (Exception ex)
            {
                logger.ToError(ex);
            }

            return false;

        }

        public Dictionary<string, Dictionary<string, Dictionary<string, List<Spread>>>> CreateSpreads()
        {
            if (!LoadQuotes()) return null;

            Dictionary<string, Dictionary<string, Dictionary<string, List<Spread>>>> spreads = [];

            foreach (var item in bbSymbols)
            {
                var category = item.Key;

                spreads.Add(category, []);

                if (category != "Spot.BTC")
                {
                    string[] bfxCategs = ["Spot.USDT", "Linear.USDT"];

                    foreach (var bfxCat in bfxCategs)
                    {
                        spreads[category].Add(bfxCat, []);
                    }

                    foreach (var item2 in item.Value)
                    {
                        var symbol = item2.Key;
                        var quotes = item2.Value.Where(q => q.Time < BeforeTime);
                        var quoteSmb = category.Split(".")[1];
                        var baseSmb = symbol.Replace(quoteSmb, "").Replace("/", "");

                        var sprSmb = baseSmb;


                        if (category == "Calendar.USDT")
                        {
                            sprSmb = symbol.Replace(quoteSmb, "");
                            baseSmb = symbol.Replace(quoteSmb, "").Split("-")[0];
                        }

                        foreach (var bfxCat in bfxCategs)
                        {
                            var bfxBaseSmb = baseSmb != "PAXG" ? baseSmb : "XAUT";

                            var bfxQuotes = bfxSymbols[bfxCat].FirstOrDefault(v => v.Key.Contains($"t{bfxBaseSmb.ToUpper()}")).Value;

                            var bfxDict = bfxQuotes.ToDictionary(t => t.Time, t => t);

                            var sprds = quotes
                                .Select(q =>
                                {
                                    bfxDict.TryGetValue(q.Time, out var q2);

                                    double q2Ask = q2.Ask;
                                    double q2Bid = q2.Bid;
                                    double q2WAsk = q2.WAAsk;
                                    double q2WBid = q2.WABid;

                                    return new Spread
                                    {
                                        Time = q.Time.UnixTimeMillisecondsToDateTime(),
                                        Sell = Math.Round(100 * (q.Ask / q2Ask - 1), 3),
                                        Buy = Math.Round(100 * (q.Bid / q2Bid - 1), 3),
                                        WABuy = Math.Round(100 * (q.Ask / q2WAsk - 1), 3),
                                        WASell = Math.Round(100 * (q.Bid / q2WBid - 1), 3),
                                    };
                                })
                                .ToList();

                            spreads[category][bfxCat].Add(sprSmb, null);
                            spreads[category][bfxCat][sprSmb] = sprds;

                        }

                    }
                }

            }

            logger.Info("Created spreads.");

            return spreads;
        }

        public void SaveSpreads()
        {
            var spreads = CreateSpreads();

            var spreadStats = new List<SpreadStat>(); 

            var spreadFolder = Path.Combine(rootFolder, "Spreads");

            if (!Directory.Exists(spreadFolder))
                Directory.CreateDirectory(spreadFolder);

            foreach (var item in spreads)
            {
                var bbCateg = item.Key;
                var bbCategFolder = Path.Combine(spreadFolder, bbCateg);
                if (!Directory.Exists(bbCategFolder))
                    Directory.CreateDirectory(bbCategFolder);

                foreach (var item2 in item.Value)
                {
                    var bfxCateg = item2.Key;
                    var bfxCategFolder = Path.Combine(bbCategFolder, bfxCateg);
                    if (!Directory.Exists(bfxCategFolder))
                        Directory.CreateDirectory(bfxCategFolder);

                    foreach (var item3 in item2.Value)
                    {
                        var sprCoin = item3.Key;
                        var file = Path.Combine(bfxCategFolder, $"{sprCoin}.csv");
                        Task<SpreadStat> tsk = Task.Factory.StartNew(() => GetSpreadStat([.. item3.Value], sprCoin, bbCateg, bfxCateg));
                        item3.Value.SaveToFile<Spread>(file);
                        var spStat = tsk.Result;
                        spreadStats.Add(spStat);
                    }
                }
            }

            var statfile = Path.Combine(spreadFolder, "spreadStats.csv");
            spreadStats.SaveToFile<SpreadStat>(statfile, SpreadStat.Header());

            logger.Info("Writed spreads and statistics to files.");

        }


       // SpreadStat(string Coin, DateTime StartTime, DateTime EndTime,
       //string LmtCat, string MktCat, string LmtSmb, string MktSmb,
       //int Count, double SellMax, double SellMin, double SellAvg, double SellSD,
       //double BuyMax, double BuyMin, double BuyAvg, double BuySD)

        public SpreadStat GetSpreadStat(Spread[] spreads, string sprCoin, string bbCat, string bfxCat)
        {
            var coin = sprCoin;
            var start = spreads.First().Time;
            var end = spreads.Last().Time;

            var sellStats = new DescriptiveStatistics(spreads.Select(s => s.Sell));
            var buyStats = new DescriptiveStatistics(spreads.Select(s => s.Buy));

            int cnt = spreads.Length;

            double sellMax = sellStats.Maximum;
            double sellMin = sellStats.Minimum; 
            double sellAvg = Math.Round(sellStats.Mean, 3);
            double sellSD = Math.Round(sellStats.StandardDeviation, 4);
            double buyMax = buyStats.Maximum;
            double buyMin = buyStats.Minimum;
            double buyAvg = Math.Round(buyStats.Mean,3);
            double buySD = Math.Round(buyStats.StandardDeviation,4);

            var spStat = new SpreadStat(coin,start,end,bbCat, bfxCat, "", "", cnt, sellMax, sellMin, sellAvg, sellSD, buyMax, buyMin, buyAvg, buySD); 

            return spStat;

        }

    }
}
