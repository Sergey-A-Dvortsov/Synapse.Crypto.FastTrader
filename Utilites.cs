using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Crypto.FastTrader
{

    public class SymbolPair
    {
        private string _coin;

        public string Coin
        {
            get => _coin;
            set
            {
                _coin = value;
            }
        }

        private string _quote;
        public string Quote
        {
            get => _quote;
            set
            {
                _quote = value;
            }
        }

        public string FutSymbol { get; set; }
        public string SpotSymbol { get; set; }
    }

    public class Utilites
    {
        public static List<SymbolPair> GetSymbolPairs()
        {
            var pairs = new List<SymbolPair>();
            pairs.Add(new SymbolPair { Coin = "BTC", Quote = "USDT", FutSymbol = "tBTCF0:USTF0", SpotSymbol = "tBTCUST" });
            pairs.Add(new SymbolPair { Coin = "XAUT", Quote = "BTC", FutSymbol = "tXAUTF0:BTCF0", SpotSymbol = "tXAUT:BTC" });
            pairs.Add(new SymbolPair { Coin = "XAUT", Quote = "USDT", FutSymbol = "tXAUTF0:USTF0", SpotSymbol = "tXAUT:UST" });
            pairs.Add(new SymbolPair { Coin = "ETH", Quote = "BTC", FutSymbol = "tETHF0:BTCF0", SpotSymbol = "tETHBTC" });
            pairs.Add(new SymbolPair { Coin = "ETH", Quote = "USDT", FutSymbol = "tETHF0:USTF0", SpotSymbol = "tETHUST" });
            return pairs;
        }

        public static SymbolPair? GetSymbolPair(string coin, string quote)
        {
            var pair = GetSymbolPairs().FirstOrDefault(s => s.Coin == coin && s.Quote == quote);
            return pair;

        }

        public static string[] GetBfxSymbols()
        {
            return ["BTC","ETH", "SOL", "AVAX", "XRP", "ADA", "LINK", "LTC", "DOGE", "DOT", "GALA", "APE", "TON", "ETC", "XAUt", "SUI"];
        }



    }
}

//"tXBT:USD"
