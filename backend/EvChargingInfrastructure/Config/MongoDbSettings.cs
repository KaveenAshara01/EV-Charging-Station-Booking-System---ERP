namespace Infrastructure.Config;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = default!;
    public string Database { get; set; } = default!;
}
