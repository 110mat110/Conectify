using Conectify.Services.Automatization;
using Conectify.Services.Automatization.Database;
using Conectify.Services.Automatization.Mapper;
using Conectify.Services.Automatization.Services;
using Conectify.Services.Library;
using Conectify.Shared.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AutomatizationDb>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DatabaseString")));
builder.Services.AddAutoMapper(typeof(RuleProfile).Assembly);
builder.Logging.AddRemoteLogging();
builder.Services.UseConectifyWebsocket<AutomatizationConfiguration, DeviceData>();
builder.Services.AddSingleton<IAutomatizationCache, AutomatizationCache>();
builder.Services.AddSingleton<IAutomatizationService, AutomatizationService>();
builder.Services.AddTransient<RuleService>();

var app = builder.Build();
await app.Services.ConnectToConectifyServer();
try
{
    var automatizationService = app.Services.GetRequiredService<IAutomatizationService>();
    automatizationService.StartServiceAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHealthChecks("/health");
app.UseAuthorization();

app.MapControllers();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Run();