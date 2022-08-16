using ScheduleBot.Enums;
using static ScheduleBot.Program;

namespace ScheduleBot;

public static class BotHandler
{
    /// <summary>Запускает обработку Updates телграм-сервера для бота.</summary>
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

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Text: { } messageText } message)
            return;

        var chatId = message.Chat.Id;

        Log.Info($"Получено сообщение '{messageText}' из чата {chatId}.");

        messageText = messageText.Split('@')[0];

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

        var task = messageText switch
        {
            "/start" => botClient.SendTextMessageAsync(chatId, startMessage, cancellationToken: cancellationToken),
            "/get1" => ScheduleFinder.SendSchedulePictureAsync(chatId, Corps.First),
            "/get2" => ScheduleFinder.SendSchedulePictureAsync(chatId, Corps.Second),
            "/get3" => ScheduleFinder.SendSchedulePictureAsync(chatId, Corps.Third),
            "/get4" => ScheduleFinder.SendSchedulePictureAsync(chatId, Corps.Fourth),
            "/subscribe1" => Notifier.AddSubscriberAsync(chatId, Corps.First),
            "/subscribe2" => Notifier.AddSubscriberAsync(chatId, Corps.Second),
            "/subscribe3" => Notifier.AddSubscriberAsync(chatId, Corps.Third),
            "/subscribe4" => Notifier.AddSubscriberAsync(chatId, Corps.Fourth),
            "/unsubscribe" => Notifier.RemoveSubscriberAsync(chatId),
            _ => Task.CompletedTask
        };

        await task;
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