// See https://aka.ms/new-console-template for more information
using Bitfinex.Net.Clients;
using Bitfinex.Net.Enums;
using bybit.net.api.Models;
using bybit.net.api.WebSocketStream;
using CryptoExchange.Net.SharedApis;
using Synapse.Crypto.Bfx;
using Synapse.Crypto.Bybit;
using Synapse.Crypto.FastTrader;
using Synapse.Crypto.Trading;

Console.WriteLine("Hello, Serg!");

BybitClient bybit = new();
var bbSecurities = await bybit.LoadSecuritiesAsync();
List<string> bbSymbols = ["BTCUSDT", "ETHUSDT"];

var assets = Utilites.GetBfxSymbols();

List<string> bbUSDTSpots = [..assets.Select(a => $"{a}/USDT")];
List<string> bbUSDCSpots = [.. assets.Select(a => $"{a}/USDC")];

List<string> bbUSDTPerps = [.. assets.Select(a => $"{a}USDT")];
List<string> bbUSDCPerps = [.. assets.Select(a => $"{a}USDC")];




bybit.FastBookUpdate += fb => 
{
    var k = fb;
}; 

var bbSubscript = await bybit.SubscribeOrderBookAsync(Category.SPOT, [..bbSymbols], 50);

//Console.WriteLine("Подписались на стакан");



BfxClient bfx = new();
var pairs = Utilites.GetSymbolPairs();
string bfxSymbol = Utilites.GetSymbolPair("BTC", "USDT").FutSymbol;
//"tBTCUST";

bfx.FastBookUpdate += fb =>
{
    var k = fb;
};

//foreach (var asset in assets)
//{
//    var symbol = bfx.GetSymbol(asset, "USDT", TradingMode.Spot);
//    var bfxSubscript = await bfx.SubscribeToOrderBookAsync(symbol);
//    Task.Delay(200).Wait();
//}

//var bfxSubscript = await bfx.SubscribeToOrderBookAsync(bfxSymbol);

List<string> lines = ["Timestamp;SellSpread;BuySpread;SellWSpread;BuyWSpread"];

DateTime saveTime = DateTime.UtcNow;

//System.Threading.Timer timer = new(new TimerCallback(OnTimerTick), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(15));

void OnTimerTick(object? state)
{
    //var sellSpread = 100 * (bybit.FastBooks[bbSymbol].BestAsk.Price/bfx.FastBooks[bfxSymbol].Book.BestAsk.Price - 1);
    //var buySpread = 100 * (bybit.FastBooks[bbSymbol].BestBid.Price /bfx.FastBooks[bfxSymbol].Book.BestBid.Price - 1);
    //var sellwSpread = 100 * (bybit.FastBooks[bbSymbol].BestAsk.Price / bfx.FastBooks[bfxSymbol].Book.GetWAPrice(1000, BookSides.Ask) - 1);
    //var buywSpread = 100 * (bybit.FastBooks[bbSymbol].BestBid.Price / bfx.FastBooks[bfxSymbol].Book.GetWAPrice(1000, BookSides.Bid) - 1);
    //var line = $"{DateTime.UtcNow:O};{sellSpread:F2};{buySpread:F2};{sellwSpread:F2};{buywSpread:F2}";

    //lines.Add(line);

    //if ((DateTime.UtcNow - saveTime).TotalMinutes >= 5)
    //{
    //    System.IO.File.AppendAllLines("spreads.csv", lines);
    //    lines.Clear();
    //    saveTime = DateTime.UtcNow;
    //}

}

Console.Read();




