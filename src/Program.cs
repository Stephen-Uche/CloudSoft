using CloudSoft.Models;
using CloudSoft.Services;
using CloudSoft.Repositories;
using CloudSoft.Configurations;
using MongoDB.Driver;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

// Check if MongoDB should be used (default to false if not specified)
bool useMongoDb = builder.Configuration.GetValue<bool>("FeatureFlags:UseMongoDb");

if (useMongoDb)
{
    // Configure MongoDB options
    builder.Services.Configure<MongoDbOptions>(
        builder.Configuration.GetSection(MongoDbOptions.SectionName));

    // Configure MongoDB client
    builder.Services.AddSingleton<IMongoClient>(serviceProvider => {
        var mongoDbOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>()
            ?? throw new InvalidOperationException(
                $"Missing '{MongoDbOptions.SectionName}' configuration section.");

        ValidateMongoDbOptions(mongoDbOptions);
        return new MongoClient(mongoDbOptions.ConnectionString);
    });

    // Configure MongoDB collection
    builder.Services.AddSingleton(serviceProvider => {
        var mongoDbOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>()
            ?? throw new InvalidOperationException(
                $"Missing '{MongoDbOptions.SectionName}' configuration section.");

        ValidateMongoDbOptions(mongoDbOptions);
        var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
        var database = mongoClient.GetDatabase(mongoDbOptions.DatabaseName);
        return database.GetCollection<Subscriber>(mongoDbOptions.SubscribersCollectionName);
    });

    // Register MongoDB repository
    builder.Services.AddSingleton<ISubscriberRepository, MongoDbSubscriberRepository>();

    Console.WriteLine("Using MongoDB repository");
}
else
{
    // Register in-memory repository as fallback
    builder.Services.AddSingleton<ISubscriberRepository, InMemorySubscriberRepository>();

    Console.WriteLine("Using in-memory repository");
}

// Register service (depends on repository)
builder.Services.AddScoped<INewsletterService, NewsletterService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static void ValidateMongoDbOptions(MongoDbOptions options)
{
    if (string.IsNullOrWhiteSpace(options.ConnectionString))
    {
        throw new InvalidOperationException(
            "MongoDB is enabled, but 'MongoDb:ConnectionString' is empty. " +
            "Set it with User Secrets for Development or the 'MongoDb__ConnectionString' environment variable for Production.");
    }

    if (ContainsPlaceholder(options.ConnectionString))
    {
        throw new InvalidOperationException(
            "MongoDB is enabled, but 'MongoDb:ConnectionString' still contains template placeholders such as '{hostname}' or '{port}'. " +
            "Replace it with a real MongoDB/Cosmos DB connection string. " +
            "For Development use 'dotnet user-secrets set \"MongoDb:ConnectionString\" \"...\"'. " +
            "For Production use the 'MongoDb__ConnectionString' environment variable.");
    }
}

static bool ContainsPlaceholder(string connectionString)
{
    return connectionString.Contains("{", StringComparison.Ordinal)
        || connectionString.Contains("}", StringComparison.Ordinal)
        || connectionString.Contains("<", StringComparison.Ordinal)
        || connectionString.Contains(">", StringComparison.Ordinal);
}
