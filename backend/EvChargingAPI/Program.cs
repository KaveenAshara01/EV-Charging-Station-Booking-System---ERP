using EvChargingAPI.Settings;
using EvChargingAPI.Repositories;
using EvChargingAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// ----------------------- MongoDB Configuration -----------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// ----------------------- MongoDB Configuration -----------------------
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings")
);

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped(sp =>
{
    var settings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>();
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// ----------------------- Register Repositories & Services -----------------------
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISlotRepository, SlotRepository>();
builder.Services.AddScoped<ISlotService, SlotService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ----------------------- Swagger + JWT -----------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EV Charging Station API",
        Version = "v1",
        Description = "Backend API for EV Charging Station Booking System"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ----------------------- JWT Authentication -----------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        // Debug logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"[DEBUG] Authentication failed: {ctx.Exception.Message}");
                return Task.CompletedTask;
            },
            OnMessageReceived = ctx =>
            {
                Console.WriteLine($"[DEBUG] Incoming JWT: {ctx.Token}");
                return Task.CompletedTask;
            }
        };

                options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine("[JWT DEBUG] Token: " + ctx.Request.Headers["Authorization"]);
            Console.WriteLine("[JWT DEBUG] Error: " + ctx.Exception.ToString());
            return Task.CompletedTask;
        }
    };
    });



builder.Services.AddAuthorization();

// ----------------------- Build App -----------------------
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
// builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

// ✅ Ensure MongoDB indexes are created for reservations
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
    await repo.EnsureIndexesAsync();
}

// ----------------------- Middleware -----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EV Charging API v1");
    });
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
