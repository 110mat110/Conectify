using Conectify.Database.Models.Dashboard;

namespace Conectify.Services.Dashboard.Models;

public record EditDashboardApi(string Name, string Background, DashboardType Type);
