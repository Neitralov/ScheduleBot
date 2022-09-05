namespace ScheduleBot;

public static class Program
{
    private static readonly string BotClientApiToken =
        GetEnvironmentVariable("BOT_API_TOKEN") ?? 
        throw new InvalidOperationException("Переменная окружения не указана");

    private static readonly string CloudConvertApiToken =
        GetEnvironmentVariable("CLOUD_CONVERT_API_TOKEN") ??
        throw new InvalidOperationException("Переменная окружения не указана");
    
    public static readonly TelegramBotClient BotClient = new(BotClientApiToken);
    public static readonly CloudConvertAPI XlsxConvert = new(CloudConvertApiToken);
    public static readonly CancellationTokenSource Cts = new();

    public static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static async Task Main()
    {
        InitNLog();
        await CheckForCachedScheduleForAllCorpsAsync();
        
        GetParsedEnvironmentVariable("SCHEDULE_CHECK_TIME_START", out int scheduleCheckTimeStart);
        GetParsedEnvironmentVariable("SCHEDULE_CHECK_TIME_END", out int scheduleCheckTimeEnd);
        var scheduleCheckTimeRange = new HoursRange(scheduleCheckTimeStart, scheduleCheckTimeEnd);
        
        GetParsedEnvironmentVariable("TIME_BETWEEN_CHECKS_IN_MILLISECONDS", out int timeBetweenChecks);
        
        var tasks = new[]
        {
            BotProcessingAsync(),
            ScheduleSearchAsync(scheduleCheckTimeRange, timeBetweenChecks)
        };

        await Task.WhenAll(tasks);
    }

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