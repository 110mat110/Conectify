using Conectify.Database;
using Conectify.Services.Automatization;
using Conectify.Services.Automatization.Mapper;
using Conectify.Services.Automatization.Services;
using Conectify.Services.Library;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ConectifyDb>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DatabaseString")));
builder.Services.AddAutoMapper(typeof(RuleProfile).Assembly);
builder.Services.UseConectifyWebsocket<AutomatizationConfiguration>();
builder.Services.AddSingleton<AutomatizationCache>();
builder.Services.AddSingleton<IAutomatizationService, AutomatizationService>();
builder.Services.AddTransient<RuleService>();

var app = builder.Build();

var automatizationService = app.Services.GetRequiredService<IAutomatizationService>();
automatizationService.StartServiceAsync();

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