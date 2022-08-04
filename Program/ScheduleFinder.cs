using ScheduleBot.Enums;
using ScheduleBot.Structs;
using static ScheduleBot.Program;
using static System.Environment;
using File = System.IO.File;

namespace ScheduleBot;

public static class ScheduleFinder
{
    private static string GetOldTablePath(Corps corps) => CurrentDirectory + $"/Data/Schedule{(int)corps}.xlsx";
    private static string GetNewTablePath(Corps corps) => CurrentDirectory + $"/Data/Schedule{(int)corps}(new).xlsx";

    private static string GetSiteSchedulePath(Corps corps) =>
        $"https://www.bgtc.su/wp-content/uploads/raspisanie/zamena{(int)corps}k.xlsx";

    private static string GetSchedulePicturePath(Corps corps) => CurrentDirectory + $"/Data/Schedule{(int)corps}.jpg";

    /// <summary>Запускает механизм слежения за обновлением расписания.</summary>
    /// <param name="searchTime">Промежуток времени, в течение которого работает метод.</param>
    public static async Task ScheduleSearchAsync(HoursRange searchTime)
    {
        CheckForCachedScheduleForAllCorps();

        while (true)
        {
            if (searchTime == DateTime.Now.Hour)
                CheckScheduleAvailabilityForAllCorps();

            await Task.Delay(120000);
        }
    }

    /// <summary>Проверяет наличие сохраненной копии расписания для всех корпусов.</summary>
    private static void CheckForCachedScheduleForAllCorps()
    {
        var tasks = new[]
        {
            CheckForCachedScheduleAsync(Corps.First),
            CheckForCachedScheduleAsync(Corps.Second),
            CheckForCachedScheduleAsync(Corps.Third),
            CheckForCachedScheduleAsync(Corps.Fourth)
        };

        foreach (var task in tasks)
            task.Wait();
    }

    /// <summary>Проверяет наличие сохраненной копии расписания.</summary>
    /// <param name="corps">Корпус у которого проверяется кэшировнное расписание.</param>
    /// <exception cref="Exception">Невозможно скачать файл расписания с сайта.</exception>
    private static async Task CheckForCachedScheduleAsync(Corps corps)
    {
        if (File.Exists(GetOldTablePath(corps)) == false)
        {
            if (!await TryLoadScheduleAsync(corps))
                throw new Exception("Не удалось скачать расписание с сайта");
            
            File.Move(GetNewTablePath(corps), GetOldTablePath(corps));
            await GetSchedulePictureAsync(corps);
        }
    }

    /// <summary>Проверяет наличие обновления расписания для всех корпусов.</summary>
    /// <remarks>Нельзя выполнять проверку обновления расписания, не имея копий последнего расписания для корпусов.</remarks>
    private static void CheckScheduleAvailabilityForAllCorps()
    {
        var tasks = new[]
        {
            ScheduleSearchAsync(Corps.First),
            ScheduleSearchAsync(Corps.Second),
            ScheduleSearchAsync(Corps.Third),
            ScheduleSearchAsync(Corps.Fourth)
        };

        foreach (var task in tasks)
            task.Wait();
    }

    /// <summary>Проверяет наличие обновления расписания.</summary>
    /// <param name="corps">Корпус у которого проверяется обновление расписания.</param>
    private static async Task ScheduleSearchAsync(Corps corps)
    {
        if (!await TryLoadScheduleAsync(corps))
            return;
        
        if (IsNewSchedule(corps))
        {
            await GetSchedulePictureAsync(corps);
            await Notifier.NotifySubscribersAsync(corps);
        }
    }
    
    /// <summary>Пробует скачать расписание корпуса.</summary>
    /// <param name="corps">Корпус, раписание которого скачивается.</param>
    /// <returns>true - раписание было скачано. false - скачать расписание не удалось.</returns>
    private static async Task<bool> TryLoadScheduleAsync(Corps corps)
    {
        Directory.CreateDirectory(CurrentDirectory + "/Data");
        using var httpClient = new HttpClient();

        try
        {
            await File.WriteAllBytesAsync(GetNewTablePath(corps),
                await httpClient.GetByteArrayAsync(GetSiteSchedulePath(corps)));
        }
        catch
        {
            Log.Error("Не удается скачать расписание с сайта.");
            return false;
        }

        return true;
    }

    /// <summary>Проверяет является ли последнее скачанное расписание новым.</summary>
    /// <param name="corps">Корпус, расписание которого проверяется.</param>
    private static bool IsNewSchedule(Corps corps)
    {
        var newTable = new FileInfo(GetNewTablePath(corps));
        var oldTable = new FileInfo(GetOldTablePath(corps));

        if (newTable.Length != oldTable.Length)
        {
            File.Move(GetNewTablePath(corps), GetOldTablePath(corps), true);
            return true;
        }

        File.Delete(GetNewTablePath(corps));
        return false;
    }

    /// <summary>Конвертирует таблицу текущего расписания на сайте в изображение.</summary>
    /// <param name="corps">Корпус, расписание которого конвертируется.</param>
    private static async Task GetSchedulePictureAsync(Corps corps)
    {
        var job = await XlsxConvert.CreateJobAsync(new JobCreateRequest()
        {
            Tasks = new
            {
                import_it = new ImportUrlCreateRequest()
                {
                    Url = GetSiteSchedulePath(corps)
                },
                convert = new ConvertCreateRequest()
                {
                    Input = "import_it",
                    Input_Format = "xlsx",
                    Output_Format = "jpg"
                },
                export_it = new ExportUrlCreateRequest()
                {
                    Input = "convert"
                }
            }
        });

        job = await XlsxConvert.WaitJobAsync(job.Data.Id);
        var exportTask = job.Data.Tasks.FirstOrDefault(t => t.Name == "export_it");
        var fileExport = exportTask?.Result.Files.FirstOrDefault();

        using var httpClient = new HttpClient();

        await File.WriteAllBytesAsync(GetSchedulePicturePath(corps),
            await httpClient.GetByteArrayAsync(fileExport!.Url));
    }

    /// <summary>Отправляет изображение расписания в чат.</summary>
    /// <param name="chatId">TelegramID на который отправляется изображение.</param>
    /// <param name="corps">Корпус, расписание которого отправляется.</param>
    public static async Task SendSchedulePictureAsync(long chatId, Corps corps)
    {
        try
        {
            await using var stream = File.OpenRead(GetSchedulePicturePath(corps));
            var inputOnlineFile = new InputOnlineFile(stream, $"Schedule{(int)corps}.jpg");
            await BotClient.SendDocumentAsync(chatId, inputOnlineFile);
        }
        catch
        {
            await BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Обработанное изображение расписания временно отсутствует.");

            Log.Error("Пользователю не удалось получить изображение расписания.");
        }
    }
}