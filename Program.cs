using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddSeq();
    });

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
        svc.DoWork(customerId);
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
}