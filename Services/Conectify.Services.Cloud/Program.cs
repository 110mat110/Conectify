using Conectify.Services.Cloud;
using Conectify.Services.Cloud.Services;
using Conectify.Services.Library;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.UseConectifyWebsocket<CloudConfiguration, DeviceData>();
builder.Services.AddSingleton<CloudService>();
builder.Services.AddSingleton<DeviceService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.Services.ConnectToConectifyServer();
var cloudService = app.Services.GetRequiredService<CloudService>();
await cloudService.StartServiceAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
