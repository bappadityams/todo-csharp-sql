using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleTodo.Api;

var builder = WebApplication.CreateBuilder(args);
var credential = new DefaultAzureCredential();
builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"]), credential);

builder.Services.AddScoped<ListsRepository>();
builder.Services.AddDbContext<TodoDb>(options =>
{
    var sqlConnectionString = builder.Configuration["AZURE_SQL_CONNECTION_STRING_KEY"];

    if (!string.IsNullOrEmpty(sqlConnectionString))
    {
        options.UseSqlServer(sqlConnectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
    }
    else
    {
        var cosmosConnectionString = builder.Configuration["AZURE_COSMOS_CONNECTION_STRING_KEY"];

        if (!string.IsNullOrEmpty(cosmosConnectionString))
        {
            options.UseCosmos(cosmosConnectionString, new DefaultAzureCredential(), "TodoDb");
        }
    }
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
    await db.Database.EnsureCreatedAsync();
}

app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});

// Swagger UI
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("./openapi.yaml", "v1");
    options.RoutePrefix = "";
});

app.UseStaticFiles(new StaticFileOptions
{
    // Serve openapi.yaml file
    ServeUnknownFileTypes = true,
});


app.MapGroup("/lists")
    .MapTodoApi()
    .WithOpenApi();
app.Run();