using Conectify.Services.Library;
using Conectify.Services.Shelly.Components;
using Conectify.Services.Shelly.Services;
using Conectify.Services.Shelly;
using Conectify.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();
builder.Services.AddDbContext<ConectifyDb>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DatabaseString")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IShellyService, ShellyService>();
builder.Services.AddSingleton<WebsocketCache>();
builder.Services.AddTransient<ShellyFactory>();
builder.Services.UseConectifyWebsocket<Configuration, DeviceData>();



var app = builder.Build();
await app.Services.ConnectToConectifyServer();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Services.GetRequiredService<IServicesWebsocketClient>().OnIncomingEvent += OnEvent;
async void OnEvent(Conectify.Database.Models.Values.Event action)
{
    var shellyService = app.Services.GetRequiredService<IShellyService>();
    await shellyService.SendValueToShelly(action);
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();
app.UseWebSockets();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Run();
