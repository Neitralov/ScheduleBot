namespace ScheduleBot.DataBase;

/// <summary>Предоставляет подключение к базе данных.</summary>
/// <remarks>Генерирует базу данных при первом вызове в программе.</remarks>
public sealed class DataBaseProvider : DbContext
{
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();

    public DataBaseProvider()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dataBaseName = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var userName = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        var connectionString = $"Host=db;Port=5432;Database={dataBaseName};Username={userName};Password={password}";
        optionsBuilder.UseNpgsql(connectionString);
    }
}