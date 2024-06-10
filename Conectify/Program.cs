using Conectify.Database;
using Conectify.Server;
using Conectify.Server.Mapper;
using Conectify.Server.Services;
using Conectify.Shared.Maps;
using Conectify.Shared.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Logging.AddRemoteLogging();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddConectifyServices();
builder.Services.AddDbContext<ConectifyDb>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DatabaseString")));
builder.Services.AddAutoMapper(typeof(DeviceProfile).Assembly, typeof(SubscriberProfile).Assembly);
builder.Services.AddTelemetry();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var service = app.Services.GetRequiredService<IDeviceStatusService>();
await service.CheckIfAlive();
app.UseHttpsRedirection();
app.UseWebSockets();
app.UseAuthorization();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapControllers();
app.Urls.Add("http://*:5000");
app.Run();
