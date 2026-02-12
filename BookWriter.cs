using bybit.net.api.Models;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Sockets.Default;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using Synapse.Crypto.Bfx;
using Synapse.Crypto.Bybit;
using Synapse.Crypto.FastTrader;
using Synapse.Crypto.Trading;
using Synapse.General;
using System;
using System.Collections.Generic;
using System.Text;

namespace Synapse.Crypto.FastTrader
{
    public class BookWriter
    {
        // Списки инструментов Bybit
        // 1. Spot.USDT
        // 2. Spot.USDC
        // 3. Spot.USDE
        // 4. Spot.BTC
        // 5. Linear.USDT
        // 6. Linear.USDC
        // 7. Inverse.USD
        // 8. Calendar.USDT

        // Списки инструментов Bitfinex
        // 1. Spot.USDT
        // 1. Spot.BTC
        // 2. Perp.USDT
        // 2. Perp.BTC


        public string rootFolder = @"D:\\Storage\\BookQuotes";

        private Logger logger;

        private BybitClient bybit;
        private BfxClient bfx;
        private List<BybitSecurity> bbSecurities;
        private Dictionary<string, Dictionary<string, List<BestQuote>>> bbSymbols;
        private Dictionary<string, Dictionary<string, List<BestQuote>>> bfxSymbols;
        private Task writeTask;

        private int writeper = 2;

        public BookWriter()
        {
            var config = new LoggingConfiguration();

            // Создание консольного таргета
            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = @"${date:format=HH\:mm\:ss} ${level:uppercase=true} ${message} ${exception}"
            };

            config.AddTarget(consoleTarget);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, consoleTarget);
            //LogManager.Configuration = config;
            logger = LogManager.GetCurrentClassLogger();
            bybit = new();
            bfx = new();
        }

        private Timer timer;

        private DateTime saveTime;

        public async Task Start()
        {
            Console.WriteLine("Start");
            logger.Info("Start");
            bybit.FastBookUpdate += Bybit_FastBookUpdate;
            bbSecurities = await bybit.LoadSecuritiesAsync();
            
            bfx.FastBookUpdate += Bfx_FastBookUpdate;

            bbSymbols = CreateBybitDictionary();
            bfxSymbols = CreateBfxDictionary();

            int depth = 50;

            await BybitSubscribe(depth);

            await BfxSubscribe();

            logger.Info("Запускаем таймер");

            timer = new(new TimerCallback(OnTimerTick), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15));

            bybit.CheckBookPong();

            saveTime = DateTime.UtcNow;

        }

        public async Task Stop()
        {
            bybit.CheckBookPong(false);
            await bybit.UnsubscribeAll();
            await bfx.UnsubscribeAllAsync();
            timer?.Dispose();
            logger.Info("Stop");
            Console.WriteLine("Stop");
        }

        private async Task<bool> BybitSubscribe(int depth = 50)
        {
            logger.Info("Начинаем подписку на стакан инструментов Bybit");
            logger.Info(" --------------------------------------------- ");

            foreach (var type in bbSymbols.Keys)
            {
                var insttype = Utilites.GetBybitInstrumentType(type);

                if (insttype == InstrumentTypes.Spot)
                {
                    int symbCnt = bbSymbols[type].Keys.Count;
                    int skip = 0;

                    while (skip >= 0)
                    {
                        string[] symbols = [.. bbSymbols[type].Keys.Skip(skip).Take(10)];

                        string result = string.Join(", ", symbols);
                        logger.Debug($"skip={skip} / {result}");

                        var subscriptId = $"book.{type}.{skip}.{depth}";
                        var subsc = await bybit.SubscribeOrderBookAsync(insttype.Value, symbols, subscriptId);

                        if (!string.IsNullOrWhiteSpace(subsc))
                        {
                            logger.Debug($"Подписались на {type}, число инструментов ={symbols.Length}");
                            await Task.Delay(200);
                            skip += 10;
                            if (skip >= symbCnt)
                                skip = -1;
                        }
                        else
                        {
                            logger.Warn("Не удалось подписаться на {0}.", type);

                        }
                    }
                }
                else
                {

                    var subscriptId = $"book.{type}.{depth}";
                    string[] symbols = [.. bbSymbols[type].Keys];

                    var subsc = await bybit.SubscribeOrderBookAsync(insttype.Value, symbols, subscriptId);

                    if (!string.IsNullOrWhiteSpace(subsc))
                    {
                        logger.Debug($"Подписались на {type}, число инструментов ={symbols.Length}");
                        await Task.Delay(500);
                    }
                    else
                    {
                        logger.Warn("Не удалось подписаться на {0}.", type);
                    }

                }

            }

            logger.Info("Подписка на инструменты Bybit закончена");
            logger.Info("");

            return true;

        }

        private async Task<bool> BfxSubscribe()
        {
            logger.Info("Начинаем подписку на стакан инструментов Bitfinex");
            logger.Info("--------------------------------------------------");

            foreach (var type in bfxSymbols.Keys)
            {
                InstrumentTypes instrType = Utilites.GetBfxInstrumentTipe(type);

                foreach (var symbol in bfxSymbols[type].Keys)
                {
                    var descr = await bfx.SubscribeOrderBookAsync(instrType, symbol);

                    if (descr != null)
                    {
                        logger.Info("Подписались на {0} / {1} .", type, symbol);
                    }

                    await Task.Delay(100);
                }

                logger.Info("Подписка на {0} закончена.", type);
            }

            logger.Info("Подписка на инструменты Bitfinex закончена");
            logger.Info("");

            return true;

        }

        public Dictionary<string, Dictionary<string, List<BestQuote>>> CreateBybitDictionary()
        {
            Dictionary<string, Dictionary<string, List<BestQuote>>> bbsymbols = new()
            {
                {"Spot.USDT", []},
                {"Spot.USDC", []},
                {"Spot.USDE", []},
                {"Spot.BTC", []},
                {"Linear.USDT", []},
                {"Linear.USDC", []},
                {"Inverse.USD", []},
                {"Calendar.USDT", []},
            };

            foreach (string category in bbsymbols.Keys)
            {
                var symbols = Utilites.GetBybitSymbols(category);

                foreach (var asset in symbols)
                {
                    string symbol = "";

                    var arr = category.Split(".");

                    if (arr[0] == "Spot")
                        symbol = $"{asset}/{arr[1]}";
                    else if (arr[0] == "Linear")
                        symbol = $"{asset}{arr[1]}";
                    else if (arr[0] == "Inverse")
                        symbol = $"{asset}{arr[1]}";
                    else if (arr[0] == "Calendar")
                    {
                        if (asset == "BTC" || asset == "ETH")
                            symbol = $"{asset}{arr[1]}-27MAR26";
                        else
                            symbol = $"{asset}{arr[1]}-27FEB26";
                    }

                    bbsymbols[category].Add(symbol, []);
                }
            }

            return bbsymbols;

        }

        public Dictionary<string, Dictionary<string, List<BestQuote>>> CreateBfxDictionary()
        {
            Dictionary<string, Dictionary<string, List<BestQuote>>> bfxsymbols = new()
            {
                {"Spot.USDT", []},
                {"Spot.BTC", []},
                {"Linear.USDT", []},
                {"Linear.BTC", []},
            };

            foreach (string category in bfxsymbols.Keys)
            {
                var symbols = Utilites.GetBfxSymbols(category);

                foreach (var asset in symbols)
                {
                    string symbol = "";

                    var arr = category.Split(".");

                    if (arr[0] == "Spot")
                        symbol = bfx.GetSymbol(asset, arr[1], TradingMode.Spot);
                    else if (arr[0] == "Linear")
                        symbol = bfx.GetSymbol(asset, arr[1], TradingMode.PerpetualLinear);
                    bfxsymbols[category].Add(symbol, []);
                }

            }

            return bfxsymbols;

        }

        private void Bfx_FastBookUpdate(FastBook obj)
        {
            // throw new NotImplementedException();
        }

        private void Bybit_FastBookUpdate(FastBook fb)
        {
            if (fb.Type == InstrumentTypes.Spot)
            {
                if (fb.QuoteSymbol == "USDT")
                {
                    var s = fb;
                }
            }

            //throw new NotImplementedException();
        }

        private async void OnTimerTick(object? state)
        {
            logger.Info("Запись данных в коллекции.");

            long time = DateTime.UtcNow.ToUnixTimeMilliseconds();

            foreach (var type in bbSymbols.Keys)
            {
                foreach (var symbol in bbSymbols[type].Keys)
                {
                    if (bybit.Books.TryGetValue(symbol.ToUpper(), out BybitBook fb))
                    {
                        double volume = symbol.EndsWith("BTC") ? 0.01 : 1000;

                        var ask = fb.BestAsk?.Price;
                        var bid = fb.BestBid?.Price;

                        if (ask != null && bid != null)
                        {
                            var qt = new BestQuote(time, ask.Value, bid.Value, fb.GetWAPrice(volume, BookSides.Ask), fb.GetWAPrice(volume, BookSides.Bid));
                            bbSymbols[type][symbol].Add(qt);
                        }

                    }
                }

            }

            foreach (var type in bfxSymbols.Keys)
            {

                foreach (var symbol in bfxSymbols[type].Keys)
                {
                    if(bfx.Books.TryGetValue(symbol, out BookSubscription bk))
                    {
                        double volume = type == "Linear.BTC" ? 0.01 : 1000;
                        var fb = bk.Book;

                        var ask = fb.BestAsk?.Price;
                        var bid = fb.BestBid?.Price;

                        if (ask != null && bid != null)
                        {
                            var qt = new BestQuote(time, ask.Value, bid.Value, fb.GetWAPrice(volume, BookSides.Ask), fb.GetWAPrice(volume, BookSides.Bid));
                            bfxSymbols[type][symbol].Add(qt);
                        }
                    }
                }

            }

            if ((DateTime.UtcNow - saveTime).TotalMinutes >= writeper)
            {
                if (writeTask != null && !writeTask.IsCompleted)
                {
                    await writeTask;
                }

                var bb = Clone(bbSymbols, "bybit");
                var bfx = Clone(bfxSymbols, "bfx");

                writeTask = Task.Run(() => WriteDataToFiles(bb, bfx));

                ClearSymbols(bbSymbols);
                ClearSymbols(bfxSymbols);

                saveTime = DateTime.UtcNow;
            }


        }

        private void WriteDataToFiles(Dictionary<string, Dictionary<string, List<BestQuote>>> bbQuotes, Dictionary<string, Dictionary<string, List<BestQuote>>> bfxQuotes)
        {
            logger.Info("Сохранение данных Bybit.");

            var bybitFolder = Path.Combine(rootFolder, "Bybit");
            if (!Directory.Exists(bybitFolder))
                Directory.CreateDirectory(bybitFolder);

            foreach (var type in bbQuotes.Keys)
            {
                var typeFolder = Path.Combine(bybitFolder, type);

                if (!Directory.Exists(typeFolder))
                    Directory.CreateDirectory(typeFolder);

                foreach (var symbol in bbQuotes[type].Keys)
                {
                    var file = Path.Combine(typeFolder, $"{symbol.Replace("/","_")}.csv");
                    bbQuotes[type][symbol].SaveToFile<BestQuote>(file, "", true);
                }

            }

            logger.Info("Сохранение данных Bitfinex.");

            var bfxFolder = Path.Combine(rootFolder, "Bfx");
            if (!Directory.Exists(bfxFolder))
                Directory.CreateDirectory(bfxFolder);


            foreach (var type in bfxQuotes.Keys)
            {
                var typeFolder = Path.Combine(bfxFolder, type);
                if (!Directory.Exists(typeFolder))
                    Directory.CreateDirectory(typeFolder);

                foreach (var symbol in bfxQuotes[type].Keys)
                {
                    var file = Path.Combine(typeFolder, $"{symbol.Replace(":","_")}.csv");
                    bfxQuotes[type][symbol].SaveToFile<BestQuote>(file, "", true);
                }

            }


        }

        private void ClearSymbols(Dictionary<string, Dictionary<string, List<BestQuote>>> quotes)
        {
            foreach (var kvp in quotes)
            {
                foreach (var kvp2 in kvp.Value)
                {
                    kvp2.Value.Clear();
                }
            }
        }

        private Dictionary<string, Dictionary<string, List<BestQuote>>> Clone(Dictionary<string, Dictionary<string, List<BestQuote>>> dic, string exh)
        {
            var diclone = exh == "bybit" ? CreateBybitDictionary() : CreateBfxDictionary();

            foreach (var kvp in dic)
            {
                foreach (var kvp2 in kvp.Value)
                {
                    foreach (var quote in kvp2.Value)
                    {
                        diclone[kvp.Key][kvp2.Key].Add(quote.Copy());
                    }
                }
            }

            return diclone;

        }
    }
}
