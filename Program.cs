
// See https://aka.ms/new-console-template for more information
using Bitfinex.Net.Clients;
using Bitfinex.Net.Enums;
using bybit.net.api.Models;
using bybit.net.api.WebSocketStream;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using NLog;
using NLog.Config;
using NLog.Targets;
using Synapse.Crypto.Bfx;
using Synapse.Crypto.Bybit;
using Synapse.Crypto.FastTrader;
using Synapse.Crypto.Trading;
using Synapse.General;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;


internal class Program
{

    #region Нативные методы для перехвата закрытия консоли

    [DllImport("kernel32.dll")]
    static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);

    delegate bool ConsoleCtrlDelegate(CtrlTypes ctrlType);
    enum CtrlTypes
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    #endregion

    //private static readonly string logPath = "crash_monitor.log";
    private static readonly object lockObj = new();

    private static Logger logger;   

    private static async Task Main(string[] args)
    {
        bool consolelog = true; 

        if (consolelog)
        {
            var config = new LoggingConfiguration();

            // Создание консольного таргета
            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = @"${date:format=HH\:mm\:ss} ${level:uppercase=true} ${message} ${exception}"
            };

            FileTarget fileTarget = new("logfile")
            {
                FileName = "${basedir}/logs/${shortdate}.log", // 
                Layout = "${date:format=HH\\:mm\\:ss} | ${level:uppercase=true} | ${logger} | ${message} ${exception:format=tostring}", // 
                Encoding = System.Text.Encoding.UTF8, // кодировка файла
                ArchiveEvery = FileArchivePeriod.Day, // ежедневная архивация
                MaxArchiveFiles = 7 // хранить архивы за 7 дней                                                                                                                       // Encoding = System.Text.Encoding.UTF8 // 可选：设置文件编码 [citation:5]
            };

            config.AddTarget(consoleTarget);
            config.AddTarget(fileTarget);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, consoleTarget);


            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget); // Все Debug+ в файл
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget); // Все Info+ в консоль


            LogManager.Configuration = config;
            logger = LogManager.GetCurrentClassLogger();
        }

        Console.WriteLine("Hello, Serg!");

        logger.Info("Начало работы.");

        // 1. ПЕРЕХВАТ ШТАТНОГО ЗАКРЫТИЯ (CTRL+C, крестик, завершение работы)
        SetConsoleCtrlHandler(OnConsoleCloseEvent, true);

        // 2. ПЕРЕХВАТ НЕОБРАБОТАННЫХ ИСКЛЮЧЕНИЙ (CRASH)
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // 3. ПЕРЕХВАТ ЗАВЕРШЕНИЯ ПРОЦЕССА (Environment.Exit, Process.Kill и др.)
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        // 4. ПЕРЕХВАТ FAULT-СОБЫТИЙ (Access Violation, Stack Overflow)
        AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
        {
            // Логируем ранние исключения до их обработки
            if (e.Exception is AccessViolationException ||
                e.Exception is StackOverflowException ||
                e.Exception is OutOfMemoryException)
            {
               logger.Error($"First chance exception (потенциальный краш): {e.Exception.GetType()}: {e.Exception.Message}");
            }
        };


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
        ba.SaveSpreads();

        //BfxClient bfx = new();

        //bfx.OrderBookUpdate += b =>
        //{
        //    var book = b;
        //};

        //var ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tBTCUST").Result;
        //Task.Delay(100).Wait();
        //ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tETHUST").Result;
        //Task.Delay(100).Wait();
        //ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tETCUST").Result;
        //Task.Delay(100).Wait();
        //ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tADAUST").Result;
        //Task.Delay(100).Wait();
        //ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tLTCUST").Result;
        //Task.Delay(100).Wait();
        //ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tAAVE:UST").Result;
        //Task.Delay(100).Wait();
        //ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tAVAX:UST").Result;
        //Task.Delay(100).Wait();
        //ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tSOLUST").Result;
        //Task.Delay(100).Wait();
        //ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tXRPUST").Result;
        //Task.Delay(100).Wait();
        //ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tXAUT:UST").Result;
        //Task.Delay(100).Wait();
        //ss = bfx.SubscribeOrderBookAsync(InstrumentTypes.Spot, "tXMRUST").Result;

        //bool debug = true;

        //if (debug)
        //{
        //    ss.ActivityPaused += OnActivityPaused;
        //    ss.ActivityUnpaused += OnActivityUnpaused;
        //    ss.SubscriptionStatusChanged += OnSubscriptionStatusChanged;
        //    ss.ConnectionClosed += OnConnectionClosed;
        //    ss.ConnectionLost += OnConnectionLost;
        //    ss.ConnectionRestored += OnConnectionRestored;
        //    ss.Exception += OnException;
        //    ss.ResubscribingFailed += OnResubscribingFailed;
        //}


        Console.Read();

        logger.Info("Нормальное завершение работы.");

    }

    /// <summary>
    /// Обработчик закрытия консоли (CTRL+C, крестик, выключение ПК)
    /// </summary>
    private static bool OnConsoleCloseEvent(CtrlTypes ctrlType)
    {
        string reason = ctrlType switch
        {
            CtrlTypes.CTRL_C_EVENT => "CTRL+C",
            CtrlTypes.CTRL_BREAK_EVENT => "CTRL+Break",
            CtrlTypes.CTRL_CLOSE_EVENT => "Закрытие окна консоли",
            CtrlTypes.CTRL_LOGOFF_EVENT => "Завершение сеанса",
            CtrlTypes.CTRL_SHUTDOWN_EVENT => "Выключение системы",
            _ => "Неизвестное событие"
        };

         logger.Info($"Штатное закрытие консоли: {reason}");

        // Здесь можно выполнить чистку ресурсов
        CleanupResources();

        // Возвращаем true, чтобы сигнал не передавался дальше
        return true;
    }

    /// <summary>
    /// Обработчик фатальных необработанных исключений
    /// </summary>
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        bool isTerminating = e.IsTerminating;

        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════");
        sb.AppendLine($"‼️ НЕОБРАБОТАННОЕ ИСКЛЮЧЕНИЕ ({(isTerminating ? "процесс завершается" : "не фатально")})");
        sb.AppendLine($"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        sb.AppendLine($"Тип: {exception?.GetType().FullName ?? "Неизвестно"}");
        sb.AppendLine($"Сообщение: {exception?.Message ?? "Нет сообщения"}");
        sb.AppendLine($"Stack Trace: {exception?.StackTrace ?? "Нет стека"}");

        if (exception?.InnerException != null)
        {
            sb.AppendLine($"Inner Exception: {exception.InnerException.Message}");
        }

        sb.AppendLine("═══════════════════════════════════════");

        logger.Warn(sb.ToString());

        // Сохраняем dump-файл (только для фатальных ошибок)
        if (isTerminating)
        {
            try
            {
                string dumpPath = $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.dmp";
                SaveCrashDump(dumpPath);
                logger.Info($"Dump сохранен: {dumpPath}");
            }
            catch (Exception ex)
            {
                logger.Error($"Ошибка сохранения dump: {ex.Message}");
            }
        }

        // Даем время на запись лога (не блокируем основной поток)
        Thread.Sleep(500);
    }

    /// <summary>
    /// Обработчик завершения процесса (Environment.Exit, Process.Kill)
    /// </summary>
    private static void OnProcessExit(object sender, EventArgs e)
    {
        logger.Warn($"Процесс завершается. Код выхода: {Environment.ExitCode}");
        CleanupResources();
    }

    /// <summary>
    /// Очистка ресурсов перед завершением
    /// </summary>
    private static void CleanupResources()
    {
        logger.Info("Выполняется очистка ресурсов...");
        // Здесь закрывайте файлы, соединения с БД, сетевые подключения
        Thread.Sleep(100); // Имитация работы
        logger.Info("Ресурсы освобождены");
    }

    /// <summary>
    /// Сохранение минидампа (требуется Windows)
    /// </summary>
    private static void SaveCrashDump(string filePath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        using var process = Process.GetCurrentProcess();
        // Используем DbgHelp.dll для создания дампа
        // Полная реализация требует P/Invoke - здесь упрощенно
        logger.Info($"Попытка сохранения дампа: {filePath}");
    }

    private static void OnActivityPaused()
    {
        logger.Info("ActivityPaused. Server is paused");
    }

    private static void OnActivityUnpaused()
    {
        logger.Info("ActivityUnpaused. Server is unpaused");
    }

    private static void OnSubscriptionStatusChanged(SubscriptionStatus status)
    {
        logger.Info($"SubscriptionStatusChanged. Status={status}");
    }

    private static void OnConnectionClosed()
    {
        logger.Info("ConnectionClosed. Connection is closed and will not be recconected");
    }

    private static void OnConnectionLost()
    {
        logger.Info("ConnectionLost. The socket will automatically reconnect when possible");
    }

    private static void OnConnectionRestored(TimeSpan ts)
    {
        logger.Info($"ConnectionRestored. Connection is restored. Ts={ts}");
    }

    private static void OnException(Exception ex)
    {
        logger.ToError(ex);
    }

    private static void OnResubscribingFailed(Error err)
    {
        logger.Info($"ResubscribingFailed. Connection is restored but resubscribing is failed. Error={err}");
    }


}







