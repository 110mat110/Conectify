using Conectify.Database;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

namespace Conectify.Server.Services;

public interface IUpdateService
{
    Task ChipUpdated(Guid deviceId);
    Task<string> GetLatestVersionFromGit();
    Task<string> GetLatestVersionUrl(Guid deviceId, CancellationToken ct = default);
    Task RegisterDeviceAsync(Guid deviceId, string softwareName, string chipVersion, CancellationToken ct = default);
}

public class UpdateService(ConectifyDb conectifyDb, Configuration configuration, IServiceScopeFactory serviceScopeFactory) : IUpdateService
{
    public async Task RegisterDeviceAsync(Guid deviceId, string softwareName, string chipVersion, CancellationToken ct = default)
    {
        var fullSwName = $"{softwareName}-{chipVersion}";
        if (!await conectifyDb.DeviceVersions.AnyAsync(x => x.DeviceId == deviceId, ct) && await conectifyDb.Devices.AnyAsync(x => x.Id == deviceId, ct))
        {
            var software = conectifyDb.Softwares.FirstOrDefault(x => x.Name == fullSwName);

            if (software != null)
            {
                await conectifyDb.DeviceVersions.AddAsync(new Database.Models.Updates.DeviceVersion { DeviceId = deviceId, Software = software, ChipVersion = chipVersion, LastUpdate = DateTime.MinValue }, ct);
                await conectifyDb.SaveChangesAsync(ct);
            }
        }
    }

    public async Task<string> GetLatestVersionUrl(Guid deviceId, CancellationToken ct = default)
    {
        var software = await conectifyDb.DeviceVersions.Include(x => x.Software).ThenInclude(x => x.Versions).FirstOrDefaultAsync(x => x.DeviceId == deviceId, ct);

        if (software is not null)
        {
            var swVersion = software.Software.Versions.OrderBy(x => x.ReleaseDate).FirstOrDefault();

            if (swVersion is not null && swVersion.ReleaseDate > software.LastUpdate)
            {
                return swVersion.Url;
            }
        }
        return string.Empty;
    }

    public async Task ChipUpdated(Guid deviceId)
    {
        var deviceVersion = await conectifyDb.DeviceVersions.FirstOrDefaultAsync(x => x.DeviceId == deviceId);

        if (deviceVersion is not null)
        {
            deviceVersion.LastUpdate = DateTime.UtcNow;
            await conectifyDb.SaveChangesAsync(); 
        }
    }

    public async Task<string> GetLatestVersionFromGit()
    {
        string owner = "110mat110";
        string repo = "ConectifyBuilds";
        string token = configuration.GitToken; // GitHub personal access token for authentication

        if (string.IsNullOrEmpty(token))
        {
            return "error";
        }

        try
        {
            // List files in the repository
            var files = await ListFiles(owner, repo, token);

            foreach (var folder in files.Where(f => f["type"]!.ToString() == "dir"))
            {
                string filePath = folder["path"]!.ToString();
                Console.WriteLine($"Processing file: {filePath}");

                // Get last modification date for each file
                var lastModifiedDate = await GetLastModifiedDate(owner, repo, filePath, token);
                var date = DateTime.Parse(lastModifiedDate).ToUniversalTime();
                var sw = await conectifyDb.Softwares.Include(x => x.Versions).FirstOrDefaultAsync(x => x.Name == filePath);
                if (sw is not null) 
                { 
                    if(!sw.Versions.Any(x => x.ReleaseDate == date))
                    {
                        sw.Versions.Add(new Database.Models.Updates.SoftwareVersion()
                        {
                            Id = Guid.NewGuid(),
                            ReleaseDate = date,
                            Software = sw,
                            Url = $"https://raw.githubusercontent.com/110mat110/{repo}/main/{filePath}/{filePath}.bin"
                        });
                    }
                }
                else
                {
                    var newSw = new Database.Models.Updates.Software()
                    {
                        Id = Guid.NewGuid(),
                        Name = filePath,
                        Versions =
                        [
                            new Database.Models.Updates.SoftwareVersion()
                            {
                                Id = Guid.NewGuid(),
                                ReleaseDate = date,
                                Url = $"https://raw.githubusercontent.com/110mat110/{repo}/main/{filePath}/{filePath}.bin"
                            }
                        ]
                    };
                    await conectifyDb.AddAsync( newSw );
                }
            }
            await conectifyDb.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return "git error";
        }

        foreach( var device in await conectifyDb.DeviceVersions.Include(x => x.Device).ToListAsync())
        {

            var command = new Event()
            {
                Type = Constants.Events.Command,
                DestinationId = device.DeviceId,
                Id = Guid.NewGuid(),
                Name = Constants.Commands.UpdateAvaliable,
                NumericValue = 0,
                SourceId = configuration.DeviceId,
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Unit = string.Empty,
                StringValue = string.Empty
            };

            using var scope = serviceScopeFactory.CreateAsyncScope();

            var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();

            await dataService.ProcessEntity(command, configuration.DeviceId, default);
        }

        return "done";
    }

    static async Task<JArray> ListFiles(string owner, string repo, string token)
    {
        string url = $"https://api.github.com/repos/{owner}/{repo}/contents/";
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return JArray.Parse(content); // Return list of files as a JArray
        }
    }

    static async Task<string> GetLastModifiedDate(string owner, string repo, string filePath, string token)
    {
        string url = $"https://api.github.com/repos/{owner}/{repo}/commits?path={filePath}";
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            JArray commits = JArray.Parse(content);

            if (commits.Count > 0)
            {
                var lastCommit = commits[0];
                var commitDate = lastCommit["commit"]!["committer"]!["date"]!.ToString();
                return commitDate; // Return the last commit date for the file
            }

            return "No commits found";
        }
    }
}
