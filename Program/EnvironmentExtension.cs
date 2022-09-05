namespace ScheduleBot;

public static class EnvironmentExtension
{
    public static void GetParsedEnvironmentVariable(string variableName, out int environmentVariable)
    {
        var rawEnvironmentVariable = 
            GetEnvironmentVariable(variableName) ??
            throw new InvalidOperationException($"Переменная окружения {variableName} не указана");
        
        if (!int.TryParse(rawEnvironmentVariable, out var parsedEnvironmentVariable))
            throw new Exception($"Переменная окружения {variableName} указана неверно");

        environmentVariable = parsedEnvironmentVariable;
    }
    
    public static void GetParsedEnvironmentVariable(string variableName, out long environmentVariable)
    {
        var rawEnvironmentVariable = 
            GetEnvironmentVariable(variableName) ??
            throw new InvalidOperationException($"Переменная окружения {variableName} не указана");
        
        if (!long.TryParse(rawEnvironmentVariable, out var parsedEnvironmentVariable))
            throw new Exception($"Переменная окружения {variableName} указана неверно");

        environmentVariable = parsedEnvironmentVariable;
    }
}