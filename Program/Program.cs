using ScheduleBot.Structs;
using static System.Environment;

namespace ScheduleBot;

public static class Program
{
    private static readonly string BotClientApiToken =
        GetEnvironmentVariable("BOT_API_TOKEN") ?? 
        throw new InvalidOperationException("Указан несуществующий токен");

    private static readonly string CloudConvertApiToken =
        GetEnvironmentVariable("CLOUD_CONVERT_API_TOKEN") ??
        throw new InvalidOperationException("Указан несуществующий токен");

    public static readonly TelegramBotClient BotClient = new(BotClientApiToken);
    public static readonly CloudConvertAPI XlsxConvert = new(CloudConvertApiToken);
    public static readonly CancellationTokenSource Cts = new();

    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static async Task Main()
    {
        InitNLog();

        var tasks = new[]
        {
            BotHandler.BotProcessingAsync(),
            ScheduleFinder.ScheduleSearchAsync(new HoursRange(10, 22))
        };

        await Task.WhenAll(tasks);
    }

    /// <summary>Устанавливает настройки конфигурации для логгера.</summary>
    private static void InitNLog()
    {
        LogManager.Setup().LoadConfiguration(builder =>
        {
            builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToConsole();
            builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToFile(
                fileName: $"Logs/log.txt",
                layout: "${date} | ${level:uppercase=true} | ${message}",
                archiveAboveSize: 10240);
        });
    }
}