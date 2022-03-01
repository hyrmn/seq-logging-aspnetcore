using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddEnvironmentVariables();

    builder.Host.UseSerilog((hostContext, loggerConfiguration) => 
        loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration)
    );

    builder.Services.AddScoped<SomeService>();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.MapGet("/", (SomeService svc) =>
    {
        var customerId = Guid.NewGuid();
        app.Logger.LogInformation("Received request {CorrelationId} for {CustomerId}", Guid.NewGuid(), customerId);
        svc.DoWork(customerId);
        return "Hello World!";
    });

    app.MapGet("/inter", (SomeService svc) =>
    {
        var customerId = Guid.NewGuid();
        app.Logger.LogInformation($"Received request {Guid.NewGuid()} for {customerId}");
        svc.DoWorkButInterpolate(customerId);
        return "Hello World!";
    });

    app.MapGet("/oops", new Func<string>(() => throw new InvalidOperationException("Oops!")));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

public class SomeService
{
    public SomeService(ILogger<SomeService> logger)
    {
        Logger = logger;
    }

    public ILogger<SomeService> Logger { get; }

    public void DoWork(Guid customerId)
    {
        Logger.LogInformation("Doing work for {CustomerId}", customerId);
    }

    public void DoWorkButInterpolate(Guid customerId)
    {
        Logger.LogInformation($"Doing work for {customerId}");
    }
}