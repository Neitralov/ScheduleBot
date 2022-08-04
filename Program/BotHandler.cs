﻿using ScheduleBot.Enums;
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

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message is not { Text: { } messageText } message)
            return;

        var chatId = message.Chat.Id;

        Log.Info($"Получено сообщение '{messageText}' из чата {chatId}.");

        messageText = messageText.Split('@')[0];

        switch (messageText)
        {
            case "/start":
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Это бот, который отправляет расписание БГК.\n\n" +
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
                          "/unsubscribe | Отписаться от всех подписок.",
                    cancellationToken: cancellationToken);
                break;
            case "/get1":
                await ScheduleFinder.SendSchedulePictureAsync(chatId, Corps.First);
                break;
            case "/get2":
                await ScheduleFinder.SendSchedulePictureAsync(chatId, Corps.Second);
                break;
            case "/get3":
                await ScheduleFinder.SendSchedulePictureAsync(chatId, Corps.Third);
                break;
            case "/get4":
                await ScheduleFinder.SendSchedulePictureAsync(chatId, Corps.Fourth);
                break;
            case "/subscribe1":
                await Notifier.AddSubscriberAsync(chatId, Corps.First);
                break;
            case "/subscribe2":
                await Notifier.AddSubscriberAsync(chatId, Corps.Second);
                break;
            case "/subscribe3":
                await Notifier.AddSubscriberAsync(chatId, Corps.Third);
                break;
            case "/subscribe4":
                await Notifier.AddSubscriberAsync(chatId, Corps.Fourth);
                break;
            case "/unsubscribe":
                await Notifier.RemoveSubscriberAsync(chatId);
                break;
        }
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