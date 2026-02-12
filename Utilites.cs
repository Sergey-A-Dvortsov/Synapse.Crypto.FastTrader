using bybit.net.api.Models;
using Synapse.Crypto.Trading;
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

        public static string[] GetAssets()
        {
            return ["BTC", "ETH", "SOL", "AVAX", "XRP", "ADA", "LINK", "LTC", "DOGE", "DOT", "GALA", "APE", "TON", "ETC", "XAUt", "SUI"];
        }

        public static string[] GetBfxSymbols(string category)
        {
            var symbols = GetAssets();

            // Списки инструментов Bitfinex
            // 1. Spot.USDT
            // 2. Spot.BTC
            // 3. Linear.USDT
            // 4. Linear.BTC


            switch (category)
            {
                case "Spot.USDT":
                    return symbols;
                case "Spot.BTC":
                    return ["ETH", "SOL", "XRP", "LTC"];
                case "Linear.USDT":
                    return symbols;
                case "Linear.BTC":
                    return ["ETH", "XRP", "LTC", "XAUt"];
                default:
                    break;
            }

            return null;
        }

        public static string[] GetBybitSymbols(string category)
        {
            var symbols = GetAssets();

            // Списки инструментов Bybit
            // 1. Spot.USDT
            // 2. Spot.USDC
            // 3. Spot.USDE
            // 4. Spot.BTC
            // 5. Linear.USDT
            // 6. Linear.USDC
            // 7. Inverse.USD
            // 8. Calendar.USDT

            switch (category)
            {
                case "Spot.USDT":
                    return symbols;
                case "Spot.USDC":
                    return [.. symbols.Except(["GALA", "ETC", "XAUt"])];
                case "Spot.USDE":
                    return ["BTC", "ETH", "SOL"];
                case "Spot.BTC":
                    return ["ETH", "SOL", "XRP", "LTC"];
                case "Linear.USDT":
                    return [.. symbols.Append("PAXG")];
                case "Linear.USDC":
                    return [.. symbols.Except(["AVAX", "ADA", "GALA", "APE"]).Append("PAXG")];
                case "Inverse.USD":
                    return [.. symbols.Except(["GALA", "APE", "XAUt"])];
                case "Calendar.USDT":
                    return ["BTC", "ETH", "SOL", "XRP", "DOGE"];
                default:
                    break;
            }

            return null;
        }

        public static Category GetBybitCategory(string category)
        {
            var type = category.Split(".")[0];
            if (type == "Spot")
                return Category.SPOT;
            else if (type == "Linear" || type == "Calendar")
                return Category.LINEAR;
            else
                return Category.INVERSE;
        }

        public static InstrumentTypes? GetBybitInstrumentType(string category)
        {
            var type = category.Split(".")[0];

            if (type == "Spot")
                return InstrumentTypes.Spot;
            else if (type == "Linear")
                return InstrumentTypes.LinearPerpetual;
            else if (type == "Inverse")
                return InstrumentTypes.InversePerpetual;
            else if (type == "Calendar")
                return InstrumentTypes.LinearFutures;
            else
                return null;
        }

        public static InstrumentTypes GetBfxInstrumentTipe(string category)
        {
            var type = category.Split(".")[0];

            if (type == "Spot")
                return InstrumentTypes.Spot;
            else
                return InstrumentTypes.LinearPerpetual;
        }

    }
}


