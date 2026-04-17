namespace SessiaAval.Data;

public class DbContextFactory
{
    private readonly string connectionString;

    public DbContextFactory(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public AppDbContext createDbContext()
    {
        return DbInitializer.createDbContext(connectionString);
    }
}
