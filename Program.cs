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


//BybitClient bybit = new();
//var bbSecurities = await bybit.LoadSecuritiesAsync();

//bybit.OrderBookUpdate += fb => 
//{
//    var book = fb;
//    if (fb.Type == InstrumentTypes.Spot)
//    {
//       var book1 = fb;
//    }
//    else if (fb.Type == InstrumentTypes.LinearPerpetual)
//    {
//        var book2 = fb;
//    }
//    else if (fb.Type == InstrumentTypes.InversePerpetual)
//    {
//        var book3 = fb;
//    }
//    else if (fb.Type == InstrumentTypes.LinearFutures)
//    {
//        var book4 = fb;
//   }

//};

//var asset = "BTC";
////var asset = "SOL";
//var quote = "USDT";
//var symbol = $"{asset}{quote}-27MAR26";
////var symbol = $"{asset}{quote}-27FEB26";

//var s = await bybit.SubscribeOrderBookAsync(InstrumentTypes.Spot, ["DOGE/USDT"]);
//Task.Delay(100).Wait();
//var s1 = await bybit.SubscribeOrderBookAsync(InstrumentTypes.LinearPerpetual, ["BTCUSDT"]);
//Task.Delay(100).Wait();
//var s2 = await bybit.SubscribeOrderBookAsync(InstrumentTypes.InversePerpetual, ["BTCUSD"]);
//Task.Delay(100).Wait();
//var s3 = await bybit.SubscribeOrderBookAsync(InstrumentTypes.LinearFutures, [symbol]);

//var wr = new BookWriter();
//await wr.Start();

//Console.Read();
//await wr.Stop();

var ba = new BookAnalizer();
ba.LoadQuotes();




Console.Read();




