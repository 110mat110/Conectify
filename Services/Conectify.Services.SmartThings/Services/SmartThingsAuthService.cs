using Conectify.Database;
using Conectify.Database.Models.SmartThings;
using Conectify.Services.SmartThings.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Conectify.Services.SmartThings.Services
{
    public class SmartThingsAuthService(ConectifyDb db)
    {
        public async Task<string> GetAccessTokenAsync(string clientId, string clientSecret)
        {
            var allTokens = await db.SmartThingsTokens.ToListAsync();
            var token = await db.SmartThingsTokens.FirstOrDefaultAsync() ?? throw new InvalidOperationException("No token in database. Insert initial refresh token first.");
            if (DateTime.UtcNow >= token.ExpiresAt.AddHours(-5))
                await RefreshTokenAsync(token, clientId, clientSecret);

            return token.AccessToken;
        }

        private async Task RefreshTokenAsync(SmartThingsToken token, string clientId, string clientSecret)
        {
            using var client = new HttpClient();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var body = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", token.RefreshToken)
        });

            var response = await client.PostAsync("https://api.smartthings.com/oauth/token", body);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            token.AccessToken = doc.RootElement.GetProperty("access_token").GetString();
            token.RefreshToken = doc.RootElement.GetProperty("refresh_token").GetString();
            var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
            token.ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

            await db.SaveChangesAsync();
        }
    }
}