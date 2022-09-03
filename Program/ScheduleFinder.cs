namespace ScheduleBot;

public static class ScheduleFinder
{
    private static string GetOldTablePath(Corps corps) => CurrentDirectory + $"/Data/Schedule{(int)corps}.xlsx";
    private static string GetNewTablePath(Corps corps) => CurrentDirectory + $"/Data/Schedule{(int)corps}(new).xlsx";

    private static string GetSiteSchedulePath(Corps corps) =>
        $"https://www.bgtc.su/wp-content/uploads/raspisanie/zamena{(int)corps}k.xlsx";

    private static string GetSchedulePicturePath(Corps corps) => CurrentDirectory + $"/Data/Schedule{(int)corps}.jpg";
    
    public static async Task ScheduleSearchAsync(HoursRange searchTime, int timeBetweenChecksInMilliseconds)
    {
        while (true)
        {
            if (searchTime == DateTime.Now.Hour)
                await CheckScheduleAvailabilityForAllCorpsAsync();

            await Task.Delay(timeBetweenChecksInMilliseconds);
        }
    }
    
    private static async Task CheckScheduleAvailabilityForAllCorpsAsync()
    {
        var tasks = new[]
        {
            ScheduleSearchAsync(Corps.First),
            ScheduleSearchAsync(Corps.Second),
            ScheduleSearchAsync(Corps.Third),
            ScheduleSearchAsync(Corps.Fourth)
        };

        await Task.WhenAll(tasks);
    }
    
    private static async Task ScheduleSearchAsync(Corps corps)
    {
        if (!await TryLoadScheduleAsync(corps))
            return;
        
        if (await IsNewScheduleAsync(corps))
        {
            Log.Info($"Обновлено расписание корпуса №{corps}. Оповещаю подписчиков.");
            await GetSchedulePictureAsync(corps);
            await NotifySubscribersAsync(corps);
        }
    }
    
    public static async Task CheckForCachedScheduleForAllCorpsAsync()
    {
        var tasks = new[]
        {
            CheckForCachedScheduleAsync(Corps.First),
            CheckForCachedScheduleAsync(Corps.Second),
            CheckForCachedScheduleAsync(Corps.Third),
            CheckForCachedScheduleAsync(Corps.Fourth)
        };
        
        await Task.WhenAll(tasks);
    }
    
    private static async Task CheckForCachedScheduleAsync(Corps corps)
    {
        if (File.Exists(GetOldTablePath(corps)) == false)
        {
            if (!await TryLoadScheduleAsync(corps))
                throw new Exception("Не удалось скачать расписание с сайта.");
            
            File.Move(GetNewTablePath(corps), GetOldTablePath(corps));
            await GetSchedulePictureAsync(corps);
        }
    }
    
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
    
    private static Task<bool> IsNewScheduleAsync(Corps corps)
    {
        var newTable = new FileInfo(GetNewTablePath(corps));
        var oldTable = new FileInfo(GetOldTablePath(corps));

        if (newTable.Length != oldTable.Length)
        {
            File.Move(GetNewTablePath(corps), GetOldTablePath(corps), true);
            return Task.FromResult(true);
        }

        File.Delete(GetNewTablePath(corps));
        return Task.FromResult(false);
    }
    
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
    
    public static async Task SendSchedulePictureAsync(long chatId, Corps corps)
    {
        await using var stream = File.OpenRead(GetSchedulePicturePath(corps));
        var inputOnlineFile = new InputOnlineFile(stream, $"Schedule{(int)corps}.jpg");
        await BotClient.SendDocumentAsync(chatId, inputOnlineFile);
    }
}