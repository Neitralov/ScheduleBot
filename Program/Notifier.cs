﻿namespace ScheduleBot;

public static class Notifier
{
    public static async Task NotifySubscribersAsync(Corps corps)
    {
        await using var db = new DataBaseProvider();
        var subscribers = db.Subscribers.Where(x => x.Corps == (int)corps).ToArray();

        var tasks = new List<Task>();
        
        foreach (var subscriber in subscribers)
        {
            tasks.Add(SendSchedulePictureAsync(subscriber.TelegramId, corps));
            await Task.Delay(35); // Ограничение телеграма: не более 30 сообщений в секунду.        
        }
        
        await Task.WhenAll(tasks);
        
        Log.Info($"Подписчики корпуса №{(int)corps} были оповещены.");
    }

    public static async Task SubscribeToScheduleNewsletter(long chatId, Corps corps)
    {
        await using var db = new DataBaseProvider();
        var isThereSameSubscriber =
            db.Subscribers.FirstOrDefault(x => (x.TelegramId == chatId) && (x.Corps == (int)corps)) != null;

        if (isThereSameSubscriber)
        {
            const string feedbackMessage = "Вы уже подписаны на обновление расписания этого корпуса.";
            await BotClient.SendTextMessageAsync(chatId, feedbackMessage);
        }
        else
        {
            const string feedbackMessage = $"Вы подписались на обновление расписания корпуса.";

            var tasks = new[]
            {
                BotClient.SendTextMessageAsync(chatId, feedbackMessage),
                AddSubscriberAsync(chatId, corps)
            };

            await Task.WhenAll(tasks);
        }
    }

    private static async Task AddSubscriberAsync(long chatId, Corps corps)
    {
        await using var db = new DataBaseProvider();
        var subscriber = new Subscriber { TelegramId = chatId, Corps = (int)corps };
        await db.Subscribers.AddAsync(subscriber);
        await db.SaveChangesAsync();
    }

    public static async Task UnsubscribeToScheduleNewsletter(long chatId)
    {
        await using var db = new DataBaseProvider();
        var isThereThisSubscriber = db.Subscribers.Any(x => x.TelegramId == chatId);

        if (isThereThisSubscriber)
        {
            const string feedbackMessage = "Вы отписались от получения обновлений всех расписаний.";

            var tasks = new[]
            {
                BotClient.SendTextMessageAsync(chatId, feedbackMessage),
                RemoveSubscriberAsync(chatId)
            };

            await Task.WhenAll(tasks);
        }
        else
        {
            const string feedbackMessage = "Вы не были подписаны на обновление какого-либо расписания.";
            await BotClient.SendTextMessageAsync(chatId, feedbackMessage);
        }
    }

    public static async Task RemoveSubscriberAsync(long chatId)
    {
        await using var db = new DataBaseProvider();
        var allSubscriberRecords = db.Subscribers.Where(x => x.TelegramId == chatId);
        db.Subscribers.RemoveRange(allSubscriberRecords);
        await db.SaveChangesAsync();
    }
}