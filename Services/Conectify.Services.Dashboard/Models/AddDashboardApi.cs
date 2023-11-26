using Conectify.Database.Models.Dashboard;

namespace Conectify.Services.Dashboard.Models;

public record AddDashboardApi(Guid UserId, int Position, DashboardType Type);
