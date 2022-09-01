namespace ScheduleBot;

public static class Notifier
{
    public static async Task NotifySubscribersAsync(Corps corps)
    {
        await using var db = new DataBaseProvider();
        var subscribers = db.Subscribers.Where(x => x.Corps == (int)corps).ToArray();

        foreach (var subscriber in subscribers)
        {
            try
            {
                var delayTask = Task.Delay(35); // Ограничение телеграм: не более 30 сообщений в секунду
                
                await ScheduleFinder.SendSchedulePictureAsync(subscriber.TelegramId, corps);
                await delayTask;
            }
            catch 
            {
                Log.Info($"Пользователь {subscriber.TelegramId} заблокировал бота. Произодится удаление.");
                
                var allSubscriberRecords = db.Subscribers.Where(x => x.TelegramId == subscriber.TelegramId);
                db.Subscribers.RemoveRange(allSubscriberRecords);
                await db.SaveChangesAsync();
            }
        }
    }

    public static async Task AddSubscriberAsync(long chatId, Corps corps)
    {
        await using var db = new DataBaseProvider();
        var subscriber = new Subscriber { TelegramId = chatId, Corps = (int)corps };

        if (db.Subscribers.FirstOrDefault(x => x.TelegramId == chatId && x.Corps == (int)corps) == null)
        {
            var sendMessageTask = BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Вы подписались на обновление расписания корпуса №{(int)corps}.");

            await db.Subscribers.AddAsync(subscriber);
            await db.SaveChangesAsync();
            
            await sendMessageTask;
        }
        else
        {
            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вы уже подписаны на обновление расписания этого корпуса.");
        }
    }

    public static async Task RemoveSubscriberAsync(long chatId)
    {
        await using var db = new DataBaseProvider();
        var isThereSubscriber = db.Subscribers.Any(x => x.TelegramId == chatId);

        if (isThereSubscriber)
        {
            var sendMessageTask = BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Вы отписались от получения обновлений всех расписаний.");

            var allSubscriberRecords = db.Subscribers.Where(x => x.TelegramId == chatId);
            db.Subscribers.RemoveRange(allSubscriberRecords);
            await db.SaveChangesAsync();

            await sendMessageTask;
        }
        else
        {
            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вы не были подписаны на обновление какого-либо расписания.");
        }
    }
}