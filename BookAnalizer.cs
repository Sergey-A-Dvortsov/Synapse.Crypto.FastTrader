using System;
using System.Collections.Generic;
using System.Text;

namespace Synapse.Crypto.FastTrader
{
    public class BookAnalizer
    {

        public string rootFolder = @"D:\\Storage\\BookQuotes";

        private Dictionary<string, Dictionary<string, List<BestQuote>>> bbSymbols;
        private Dictionary<string, Dictionary<string, List<BestQuote>>> bfxSymbols;

        public BookAnalizer()
        {
            var bw = new BookWriter();
            bbSymbols = bw.CreateBybitDictionary();
            bfxSymbols = bw.CreateBfxDictionary();
            rootFolder = bw.rootFolder;
        }

        public void LoadQuotes()
        {
            var bbFolder = Path.Combine(rootFolder, "Bybit");

            foreach(var item in bbSymbols)
            {
                var typeFolder = Path.Combine(bbFolder, item.Key);

                foreach (var item2 in item.Value)
                {
                    var file = Path.Combine(typeFolder, $"{item2.Key}.csv");
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
                    var file = Path.Combine(typeFolder, $"{item2.Key}.csv");
                    var lines = File.ReadAllLines(file);

                    List<BestQuote> quotes = [.. lines.Select(l => BestQuote.Parse(l).GetValueOrDefault())];
                    item.Value[item2.Key] = quotes;
                }
            }

        }

    }
}
