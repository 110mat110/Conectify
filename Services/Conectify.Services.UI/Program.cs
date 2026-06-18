using Conectify.Database;
using Conectify.Services.UI.Services;
using Conectify.Shared.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddTelemetry();
builder.Logging.AddRemoteLogging();

builder.Services.AddDbContext<ConectifyDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseString")));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IDeviceMetadataCachingService, DeviceMetadataCachingService>();
builder.Services.AddTransient<IHistoryHttpClient, HistoryHttpClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseHealthChecks("/health");
app.MapControllers();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Run();
