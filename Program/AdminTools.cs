namespace ScheduleBot;

public static class AdminTools
{
    public static async Task GetNumberOfBotSubscribersAsync(long chatId)
    {
        if (await HasAdminAccess(chatId))
        {
            Log.Info("Доступ получен");
            await using var db = new DataBaseProvider();

            var numberOfSubscribers = db.Subscribers.Count();
            var numberOfSubscribersInCorps1 = db.Subscribers.Count(x => x.Corps == 1);
            var numberOfSubscribersInCorps2 = db.Subscribers.Count(x => x.Corps == 2);
            var numberOfSubscribersInCorps3 = db.Subscribers.Count(x => x.Corps == 3);
            var numberOfSubscribersInCorps4 = db.Subscribers.Count(x => x.Corps == 4);

            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Количество подписчиков по корпусам: \n" +
                      $"[{numberOfSubscribersInCorps1,3}] - Первый корпус.\n" +
                      $"[{numberOfSubscribersInCorps2,3}] - Второй корпус.\n" +
                      $"[{numberOfSubscribersInCorps3,3}] - Третий корпус.\n" +
                      $"[{numberOfSubscribersInCorps4,3}] - Четвертый корпус.\n\n" +
                      $"[{numberOfSubscribers,3}] - Всего.");    
        }
        else
        {
            Log.Info("В доступе отказано");
            const string feedbackMessage = "У вас недостаточно прав для выполнения этой команды";
            await BotClient.SendTextMessageAsync(chatId, feedbackMessage);
        }
    }

    public static async Task GetLogsArchiveAsync(long chatId)
    {
        if (await HasAdminAccess(chatId))
        {
            Log.Info("Доступ получен");
            var sourceDirectoryName = CurrentDirectory + "/Logs";
            var destinationArchiveFileName = CurrentDirectory + "/logs.zip";
            ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName); 
            
            await using var stream = File.OpenRead(destinationArchiveFileName);
            var inputOnlineFile = new InputOnlineFile(stream, "Logs.zip");
            await BotClient.SendDocumentAsync(chatId, inputOnlineFile);
            
            File.Delete(destinationArchiveFileName);
        }
        else
        {
            Log.Info("В доступе отказано");
            const string feedbackMessage = "У вас недостаточно прав для выполнения этой команды";
            await BotClient.SendTextMessageAsync(chatId, feedbackMessage);
        }
    }

    private static Task<bool> HasAdminAccess(long chatId)
    {
        GetParsedEnvironmentVariable("ADMIN_TELEGRAM_ID", out long adminId);
        return Task.FromResult(adminId == chatId);
    }
}