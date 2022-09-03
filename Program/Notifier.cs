namespace ScheduleBot;

public static class Notifier
{
    public static async Task NotifySubscribersAsync(Corps corps)
    {
        await using var db = new DataBaseProvider();
        var subscribers = db.Subscribers.Where(x => x.Corps == (int)corps).ToArray();
        
        var timeBeforeTask = DateTime.Now;
        
        foreach (var subscriber in subscribers)
        {
            try
            {
                var tasks = new[]
                {
                    // Ограничение телеграм: не более 30 сообщений в секунду
                    Task.Delay(35),
                    SendSchedulePictureAsync(subscriber.TelegramId, corps)
                };

                await Task.WhenAll(tasks);
            }
            catch
            {
                Log.Info($"Пользователь {subscriber.TelegramId} заблокировал бота. Произодится удаление.");
                await RemoveSubscriberAsync(subscriber.TelegramId);
            }
        }
        
        var timeAfterTask = DateTime.Now;
        var alertTime = timeAfterTask - timeBeforeTask;
        Log.Info($"Подписчики корпуса №{corps} были оповещены за: {alertTime.TotalMilliseconds} миллисекунд.");
    }

    public static async Task SubscribeToScheduleNewsletter(long chatId, Corps corps)
    {
        await using var db = new DataBaseProvider();
        var isThereSameSubscriber =
            db.Subscribers.FirstOrDefault(x => x.TelegramId == chatId && x.Corps == (int)corps) == null;

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

    private static async Task RemoveSubscriberAsync(long chatId)
    {
        await using var db = new DataBaseProvider();
        var allSubscriberRecords = db.Subscribers.Where(x => x.TelegramId == chatId);
        db.Subscribers.RemoveRange(allSubscriberRecords);
        await db.SaveChangesAsync();
    }
}