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
using Synapse.General;
using System.Timers;

Console.WriteLine("Hello, Serg!");

string rootFolder = @"D:\\Storage\\BookQuotes";

BybitClient bybit = new();
var bbSecurities = await bybit.LoadSecuritiesAsync();

BfxClient bfx = new();

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

Dictionary<string, Dictionary<string, List<BestQuote>>> bbSymbols = new() 
{
    {"Spot.USDT", []},
    {"Spot.USDC", []},
    {"Spot.USDE", []},
    {"Spot.BTC", []},
    {"Linear.USDT", []},
    {"Linear.USDC", []},
    {"Inverse.USD", []},
    {"Calendar.USDT", []},
} ;

foreach (string category in bbSymbols.Keys)
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
            if(asset == "BTC" || asset == "ETH")
                symbol = $"{asset}{arr[1]}-27MAR26";
            else
                symbol = $"{asset}{arr[1]}-27FEB26";
        }
            
        bbSymbols[category].Add(symbol, []);
    }

}


Dictionary<string, Dictionary<string, List<BestQuote>>> bfxSymbols = new()
{
    {"Spot.USDT", []},
    {"Spot.BTC", []},
    {"Linear.USDT", []},
    {"Linear.BTC", []},
};

foreach (string category in bfxSymbols.Keys)
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
        bbSymbols[category].Add(symbol, []);
    }

}

bybit.FastBookUpdate += fb => 
{
    var k = fb;
};

Console.WriteLine("Начинаем подписку на стакан инструментов Bybit");
Console.WriteLine(" --------------------------------------------- ");
foreach (var type in bbSymbols.Keys)
{
    var category = Utilites.GetBybitCategory(type);
    var descr = await bybit.SubscribeOrderBookAsync(category, [.. bbSymbols[type].Keys], 50);
    Console.WriteLine("Подписались на {0}.", type);
    await Task.Delay(500);
}

Console.WriteLine("Подписка на инструменты Bybit закончена");

bfx.FastBookUpdate += fb =>
{
    var k = fb;
};

foreach (var type in bfxSymbols.Keys)
{
    InstrumentTypes instrType = Utilites.GetBfxInstrumentTipe(type);

    foreach (var symbol in bfxSymbols[type].Keys)
    {
        var descr = await bfx.SubscribeToOrderBookAsync(instrType, symbol);
        await Task.Delay(200);
    }

    Console.WriteLine("Подписались на {0}.", type);
  
}

Console.WriteLine("Подписка на инструменты Bitfinex закончена");


//foreach (var asset in assets)
//{
//    var symbol = bfx.GetSymbol(asset, "USDT", TradingMode.Spot);
//    var bfxSubscript = await bfx.SubscribeToOrderBookAsync(symbol);
//    Task.Delay(200).Wait();
//}

//var bfxSubscript = await bfx.SubscribeToOrderBookAsync(bfxSymbol);

List<string> lines = ["Timestamp;SellSpread;BuySpread;SellWSpread;BuyWSpread"];

DateTime saveTime = DateTime.UtcNow;

Task writeTask;


//System.Threading.Timer timer = new(new TimerCallback(OnTimerTick), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(15));

async Task OnTimerTick(object? state)
{

    long time = DateTime.UtcNow.ToUnixTimeMilliseconds();

    foreach (var type in bbSymbols.Keys)
    {

        foreach (var symbol in bbSymbols[type].Keys)
        {
            var fb = bybit.FastBooks[symbol];
            var qt = new BestQuote(time, fb.BestAsk.Price, fb.BestBid.Price, fb.GetWAPrice(1000, BookSides.Ask), fb.GetWAPrice(1000, BookSides.Bid));
            bbSymbols[type][symbol].Add(qt);
        }

    }

    foreach (var type in bfxSymbols.Keys)
    {

        foreach (var symbol in bfxSymbols[type].Keys)
        {
            var fb = bfx.FastBooks[symbol].Book;
            var qt = new BestQuote(time, fb.BestAsk.Price, fb.BestBid.Price, fb.GetWAPrice(1000, BookSides.Ask), fb.GetWAPrice(1000, BookSides.Bid));
            bfxSymbols[type][symbol].Add(qt);
        }

    }

    if ((DateTime.UtcNow - saveTime).TotalMinutes >= 5)
    {
        if(writeTask != null && !writeTask.IsCompleted)
        {
            await writeTask;
        }

        writeTask = Task.Run(() => WriteDataToFiles(bbSymbols, bfxSymbols) );

        ClearSymbols(bbSymbols);
        ClearSymbols(bfxSymbols);

        saveTime = DateTime.UtcNow;
    }


}




void WriteDataToFiles(Dictionary<string, Dictionary<string, List<BestQuote>>> bbQuotes, Dictionary<string, Dictionary<string, List<BestQuote>>> bfxQuotes)
{
    var bybitFolder = Path.Combine(rootFolder, "Bybit");
    if (!Directory.Exists(bybitFolder))
        Directory.CreateDirectory(bybitFolder);

    foreach (var type in bbSymbols.Keys)
    {
        var typeFolder = Path.Combine(bybitFolder, type);

        if (!Directory.Exists(typeFolder))
            Directory.CreateDirectory(typeFolder);

        foreach (var symbol in bbSymbols[type].Keys)
        {
            var file = Path.Combine(typeFolder, $"{symbol}.csv");
            bbSymbols[type][symbol].SaveToFile<BestQuote>(file, "", true);
        }

    }

    var bfxFolder = Path.Combine(rootFolder, "Bfx");
    if (!Directory.Exists(bfxFolder))
        Directory.CreateDirectory(bfxFolder);


    foreach (var type in bfxSymbols.Keys)
    {
        var typeFolder = Path.Combine(bfxFolder, type);
        if (!Directory.Exists(typeFolder))
            Directory.CreateDirectory(typeFolder);

        foreach (var symbol in bfxSymbols[type].Keys)
        {
            var file = Path.Combine(typeFolder, $"{symbol}.csv");
            bfxSymbols[type][symbol].SaveToFile<BestQuote>(file, "", true);
        }

    }


}

void ClearSymbols(Dictionary<string, Dictionary<string, List<BestQuote>>> quotes)
{
    foreach (var kvp in quotes)
    {
        foreach (var kvp2 in kvp.Value)
        {
            kvp2.Value.Clear();
        }
    }
}



Console.Read();




