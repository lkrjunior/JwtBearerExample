#region TopLevelStatements

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services);

var app = builder.Build();

Configure(app, app.Environment);

app.Run();

#endregion

#region Startup

static void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();

    services.AddEndpointsApiExplorer();

    services.AddSwaggerGen();

    services.AddMemoryCache();

    services.AddHttpClient("Server", httpClient =>
    {
        httpClient.BaseAddress = new Uri("https://localhost:7222");
    });
}

static void Configure(WebApplication app, IWebHostEnvironment environment)
{
    if (environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();
}

#endregion