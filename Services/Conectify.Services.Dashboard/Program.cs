using Conectify.Database;
using Conectify.Services.Dashboard;
using Conectify.Services.Dashboard.Mapper;
using Conectify.Services.Dashboard.Services;
using Conectify.Services.Library;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.UseConectifyWebsocket<Configuration, DeviceData>();
builder.Services.AddTransient<DashboardService>();
builder.Services.AddTransient<UserService>();
builder.Services.AddDbContext<ConectifyDb>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DatabaseString")));
builder.Services.AddAutoMapper(typeof(DashboardProfile).Assembly);
var app = builder.Build();

//await app.Services.ConnectToConectifyServer();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Run();
