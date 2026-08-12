using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SMAS.API.Data;
using SMAS.API.Models;
using SMAS.API.Services;
using SMAS.API.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;

namespace SMAS.API.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task Login_ShouldReturn_Token_And_CreateRefreshToken()
        {
            var options = new DbContextOptionsBuilder<SmasDbContext>()
                .UseInMemoryDatabase(databaseName: "AuthTestDb")
                .Options;

            var inMemorySettings = new Dictionary<string, string> {
                { "Jwt:Key", "TestJwtKey012345678901234567890123456789012345678901234567890" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiryMinutes", "60" },
                { "Jwt:RefreshExpiryDays", "30" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            using (var context = new SmasDbContext(options))
            {
                var customer = new Customer { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), FullName = "Test" };
                context.Customers.Add(customer);
                await context.SaveChangesAsync();

                var authService = new AuthService(context, configuration);
                var dto = new LoginDto { Email = "test@example.com", Password = "password", ClientIp = "127.0.0.1" };
                var result = await authService.LoginAsync(dto);

                Assert.False(string.IsNullOrEmpty(result.Token));
                Assert.False(string.IsNullOrEmpty(result.RefreshToken));

                var stored = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == result.RefreshToken);
                Assert.NotNull(stored);
            }
        }
    }
}
