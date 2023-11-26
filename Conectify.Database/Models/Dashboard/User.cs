using Conectify.Database.Interfaces;

namespace Conectify.Database.Models.Dashboard;
public class User : IEntity
{
    public Guid Id { get; set; }

    public string UserMail { get; set; } = string.Empty;
}
