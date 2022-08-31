namespace ScheduleBot;

public static class Notifier
{
    /// <summary>Отправляет изображение расписания подписчикам.</summary>
    /// <param name="corps">Корпус, подписчики которого оповещаются.</param>
    public static async Task NotifySubscribersAsync(Corps corps)
    {
        await using var db = new DataBaseProvider();
        var subscribers = db.Subscribers.Where(x => x.Corps == (int)corps);

        foreach (var subscriber in subscribers)
        {
            await ScheduleFinder.SendSchedulePictureAsync(subscriber.TelegramId, corps);
            await Task.Delay(50); // Ограничение телеграм: не более 30 сообщений в секунду
        }
    }

    /// <summary>Добавляет подписчика в БД.</summary>
    /// <param name="chatId">TelegramID подписывающегося.</param>
    /// <param name="corps">Корпус, расписание которого хочет получать подписчик.</param>
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

    /// <summary>Удаляет подписчика из БД.</summary>
    /// <param name="chatId">TelegramID подписчика, которого нужно отписать от обновлений.</param>
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