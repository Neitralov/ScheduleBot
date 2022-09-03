﻿namespace ScheduleBot;

public static class BotHandler
{
    public static async Task BotProcessingAsync()
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        BotClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, receiverOptions, Cts.Token);

        var me = await BotClient.GetMeAsync(Cts.Token);

        Log.Info($"Начато прослушивание бота @{me.Username}");
        Console.ReadLine();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message is not { Text: { } messageText } message)
            return;

        var chatId = message.Chat.Id;
        
        GetParsedEnvironmentVariable("ADMIN_TELEGRAM_ID", out long adminId);

        Log.Info(chatId == adminId
            ? $"Получено сообщение '{messageText}' из чата ADMIN."
            : $"Получено сообщение '{messageText}' из чата {chatId}.");

        var command = messageText.Split('@')[0];
        await ProcessCommandAsync(command, chatId);
    }

    private static async Task ProcessCommandAsync(string command, long chatId)
    {
        const string startMessage = "Это бот, который отправляет расписание БГК.\n\n" +
                                    "Получите изображение с актуальным расписанием!\n" +
                                    "/get1 | Первый корпус.\n" +
                                    "/get2 | Второй корпус.\n" +
                                    "/get3 | Третий корпус.\n" +
                                    "/get4 | Четвертый корпус.\n\n" +
                                    "Подпишитесь на рассылку новых расписаний!\n" +
                                    "/subscribe1 | Первый корпус.\n" +
                                    "/subscribe2 | Второй корпус.\n" +
                                    "/subscribe3 | Третий корпус.\n" +
                                    "/subscribe4 | Четвертый корпус.\n\n" +
                                    "/unsubscribe | Отписаться от всех подписок.";

        var task = command switch
        {
            "/start"       => BotClient.SendTextMessageAsync(chatId, startMessage),
            "/get1"        => SendSchedulePictureAsync(chatId, Corps.First),
            "/get2"        => SendSchedulePictureAsync(chatId, Corps.Second),
            "/get3"        => SendSchedulePictureAsync(chatId, Corps.Third),
            "/get4"        => SendSchedulePictureAsync(chatId, Corps.Fourth),
            "/subscribe1"  => SubscribeToScheduleNewsletter(chatId, Corps.First),
            "/subscribe2"  => SubscribeToScheduleNewsletter(chatId, Corps.Second),
            "/subscribe3"  => SubscribeToScheduleNewsletter(chatId, Corps.Third),
            "/subscribe4"  => SubscribeToScheduleNewsletter(chatId, Corps.Fourth),
            "/unsubscribe" => UnsubscribeToScheduleNewsletter(chatId),
            "/status"      => GetNumberOfBotSubscribersAsync(chatId),
            "/logs"        => GetLogsArchiveAsync(chatId),
            _              => Task.CompletedTask
        };

        await task;    
    }

    private static async Task GetNumberOfBotSubscribersAsync(long chatId)
    {
        if (await HasAdminAccess(chatId))
        {
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
            const string feedbackMessage = "У вас недостаточно прав для выполнения этой команды";
            await BotClient.SendTextMessageAsync(chatId, feedbackMessage);
        }
    }

    private static async Task GetLogsArchiveAsync(long chatId)
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

    private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}