using Conectify.Database;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Server.Services;

public class UserService
{
    private readonly ConectifyDb conectifyDb;

    public UserService(ConectifyDb conectifyDb)
    {
        this.conectifyDb = conectifyDb;
    }

    public async Task<Guid> GetUser(string userMail)
    {
        var existingUser = await conectifyDb.Users.FirstOrDefaultAsync(x => x.UserMail == userMail);

        if (existingUser is null)
        {
            var id = Guid.NewGuid();
            await conectifyDb.Users.AddAsync(new Database.Models.Dashboard.User() { UserMail = userMail, Id = id });
            await conectifyDb.SaveChangesAsync();
            return id;
        }

        return existingUser.Id;
    }
}
