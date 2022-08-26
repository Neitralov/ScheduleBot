using static System.Environment;

namespace ScheduleBot;

public static class EnvironmentExtension
{
    /// <summary>Возвращает переменную среду в требуемом типе данных.</summary>
    /// <param name="variableName">Название переменной окружения.</param>
    /// <param name="environmentVariable">Результат извлечения данных переменной окружения после парсинга.</param>
    public static void GetParsedEnvironmentVariable(string variableName, out int environmentVariable)
    {
        var rawEnvironmentVariable = 
            GetEnvironmentVariable(variableName) ??
            throw new InvalidOperationException($"Переменная окружения {variableName} не указана");
        
        if (!int.TryParse(rawEnvironmentVariable, out var parsedEnvironmentVariable))
            throw new Exception($"Переменная окружения {variableName} указана неверно");

        environmentVariable = parsedEnvironmentVariable;
    }
    
    /// <summary>Возвращает переменную среду в требуемом типе данных.</summary>
    /// <param name="variableName">Название переменной окружения.</param>
    /// <param name="environmentVariable">Результат извлечения данных переменной окружения после парсинга.</param>
    public static void GetParsedEnvironmentVariable(string variableName, out uint environmentVariable)
    {
        var rawEnvironmentVariable = 
            GetEnvironmentVariable(variableName) ??
            throw new InvalidOperationException($"Переменная окружения {variableName} не указана");
        
        if (!uint.TryParse(rawEnvironmentVariable, out var parsedEnvironmentVariable))
            throw new Exception($"Переменная окружения {variableName} указана неверно");

        environmentVariable = parsedEnvironmentVariable;
    }
}