namespace ScheduleBot;

public static class AdminTools
{
    public static async Task GetNumberOfBotSubscribersAsync(long chatId)
    {
        if (await HasAdminAccess(chatId))
        {
            await using var db = new DataBaseProvider();

            var subscribers = db.Subscribers.ToArray();
            
            var totalSubscribers = subscribers.Length;

            var chatSubscribersInCorps = new int[4];
            var groupSubscribersInCorps = new int[4];
            var subscribersInCorps = new int[4];
            
            for (var index = 0; index < 4; index++)
            {
                chatSubscribersInCorps[index] = 
                    subscribers.Count(x => (x.TelegramId >= 0) && (x.Corps == index + 1));
                groupSubscribersInCorps[index] = 
                    subscribers.Count(x => (x.TelegramId < 0) && (x.Corps == index + 1));
                
                subscribersInCorps[index] = chatSubscribersInCorps[index] + groupSubscribersInCorps[index];
            }

            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Количество подписчиков по корпусам:\n\n" +
                      $"[{subscribersInCorps[0]}] - Первый корпус.\n" +
                      $"Из них: [{chatSubscribersInCorps[0]}/{groupSubscribersInCorps[0]}] - Чатов/Групп.\n" +
                      $"[{subscribersInCorps[1]}] - Второй корпус.\n" +
                      $"Из них: [{chatSubscribersInCorps[1]}/{groupSubscribersInCorps[1]}] - Чатов/Групп.\n" +
                      $"[{subscribersInCorps[2]}] - Третий корпус.\n" +
                      $"Из них: [{chatSubscribersInCorps[2]}/{groupSubscribersInCorps[2]}] - Чатов/Групп.\n" +
                      $"[{subscribersInCorps[3]}] - Четвертый корпус.\n" +
                      $"Из них: [{chatSubscribersInCorps[3]}/{groupSubscribersInCorps[3]}] - Чатов/Групп.\n\n" +
                      $"[{totalSubscribers}] - Всего подписчиков.");
        }
        else
        {
            const string feedbackMessage = "У вас недостаточно прав для выполнения этой команды";
            await BotClient.SendTextMessageAsync(chatId, feedbackMessage);
        }
    }

    public static async Task GetLogsArchiveAsync(long chatId)
    {
        if (await HasAdminAccess(chatId))
        {
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