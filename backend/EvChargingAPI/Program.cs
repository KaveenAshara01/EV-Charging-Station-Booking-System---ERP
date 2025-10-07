
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapControllers();

// Seed MongoDB with sample owners
try
{
    var seeder = new EvChargingAPI.MongoDbSeeder(
        "mongodb+srv://nipunabhashitha76_db_user:UaOU9UeRIsfC1y0h@cluster0.3k61g7v.mongodb.net/",
        "EvBookingSystem"
    );
    seeder.SeedAll();
    Console.WriteLine("MongoDB seeding completed.");
}
catch (Exception ex)
{
    Console.WriteLine($"MongoDB seeding error: {ex.Message}");
}

app.Run();

