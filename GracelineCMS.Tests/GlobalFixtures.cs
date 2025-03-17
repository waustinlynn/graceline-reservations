using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Domain.Entities;
using GracelineCMS.Infrastructure.Repository;
using GracelineCMS.Tests.Fakes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;

namespace GracelineCMS.Tests
{
    [SetUpFixture]
    public class GlobalFixtures
    {
        private static readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private static void ResetDatabase()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.Migrate();
                context.Users.Add(new User { EmailAddress = _globalAdminEmail });
                context.SaveChanges();
            }
        }
        public static IDbContextFactory<AppDbContext> DbContextFactory
        {
            get
            {
                return _dbContextFactory;
            }
        }
        private static readonly HttpClient _httpClient;
        public static HttpClient HttpClient
        {
            get
            {
                return _httpClient;
            }
        }

        private const string _globalAdminEmail = "admin@global.com";
        public static string GlobalAdminEmail { get { return _globalAdminEmail; } }

        public static string GetAuthToken(string emailAddress)
        {
            var tokenHandler = GetRequiredService<ITokenHandler>();
            var accessRefreshToken = tokenHandler.CreateAccessAndRefreshToken(emailAddress);
            return accessRefreshToken.Result.AccessToken;
        }



        private static readonly IServiceProvider _serviceProvider;
        public static T GetRequiredService<T>()
        {
#pragma warning disable CS8714
            return _serviceProvider.GetRequiredService<T>() ?? throw new Exception($"Cannot resolve service for type: {typeof(T)}");
#pragma warning restore CS8714
        }

        private const string ConnectionString = "Host=localhost;Port=5432;Database=gracelinecms_test;Username=postgres;Password=postgres";
        public static IConfiguration Configuration { get; private set; }
        public static User GetSavedUser(IDbContextFactory<AppDbContext>? dbContextFactory = null)
        {
            if (dbContextFactory == null)
            {
                dbContextFactory = GetRequiredService<IDbContextFactory<AppDbContext>>();
            }
            using (var context = dbContextFactory.CreateDbContext())
            {
                var user = FakeUser.User;
                user.EmailAddress = user.EmailAddress.ToLower();
                context.Users.Add(user);
                context.SaveChanges();
                return user;
            }
        }

        public static AuthenticatedRequestHelper GetAuthenticatedRequestHelper(IDbContextFactory<AppDbContext>? dbContextFactory = null)
        {
            dbContextFactory = dbContextFactory ?? GetRequiredService<IDbContextFactory<AppDbContext>>();
            using (var context = dbContextFactory.CreateDbContext())
            {
                var user = FakeUser.User;
                var organization = FakeOrganization.Organization;
                user.EmailAddress = user.EmailAddress.ToLower();
                organization.Users.Add(user);
                context.Organizations.Add(organization);
                context.SaveChanges();
                var authToken = GetAuthToken(user.EmailAddress);
                var headers = new Dictionary<string, string>
                {
                    { "OrganizationId", organization.Id },
                };
                return new AuthenticatedRequestHelper { AuthToken = authToken, Organization = organization, User = user, Headers = headers };
            }
        }

        static GlobalFixtures()
        {
            var webApplicationFactory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            { "ConnectionStrings:DefaultConnection", ConnectionString },
                            { "GOOGLE_SMTP_SA_CREDENTIAL", "FakeCredential" },
                            { "AuthenticationSigningSecret", "abcdefghijklmnopqrstuvwxyz123456789abcmendikekjdjjdkkdklllsjsjsjjkdk" },
                            { "GlobalAdminEmail", _globalAdminEmail }
                        });
                    });
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IEmailClient, TestEmailClient>();
                    });
                });
            Configuration = webApplicationFactory.Services.GetRequiredService<IConfiguration>();
            _dbContextFactory = webApplicationFactory.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            _serviceProvider = webApplicationFactory.Services;
            ResetDatabase();
            _httpClient = webApplicationFactory.CreateClient();
        }

        [OneTimeTearDown]
        public async Task GlobalTeardown()
        {
            using var context = await DbContextFactory.CreateDbContextAsync();
            await context.Database.EnsureDeletedAsync(); // Cleanup after tests
            HttpClient.Dispose();
        }

        //api helper methods
        public async static Task<HttpResponseMessage> GetAsync(string path, string? authToken = null, Dictionary<string, string>? customHeaders = null)
        {
            if (authToken != null)
            {
                HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            var response = await HttpClient.GetAsync(path);

            HttpClient.DefaultRequestHeaders.Clear();
            return response;
        }

        public async static Task<HttpResponseMessage> PostAsync<T>(string path, T content, string? authToken = null, Dictionary<string, string>? customHeaders = null)
        {
            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            if (authToken != null)
            {
                HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            var response = await HttpClient.PostAsync(path, httpContent);
            HttpClient.DefaultRequestHeaders.Clear();
            return response;
        }
    }

    public static class ObjectHelpers
    {
        public static DefaultEmailAddressConfig GetDefaultEmailAddressConfig(string? fromAddress = "system@test.com", string? fromName = "System Name")
        {
            return new DefaultEmailAddressConfig()
            {
                FromAddress = fromAddress,
                FromName = fromName
            };
        }
    }

}

public class AuthenticatedRequestHelper
{
    public required string AuthToken { get; set; }
    public required Organization Organization { get; set; }
    public required User User { get; set; }
    public required Dictionary<string, string> Headers { get; set; }
}

public class TestEmailClient : IEmailClient
{
    public List<EmailMessage> SentMessages = new List<EmailMessage>();
    public async Task SendEmailAsync(EmailMessage emailMessage)
    {
        await Task.CompletedTask;
        SentMessages.Add(emailMessage);
    }
}

public static class HttpResponseExtensionMethods
{
    public static string GetRefreshToken(this HttpResponseMessage response)
    {
        if(response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            var refreshTokenCookie = setCookieHeaders.FirstOrDefault(h => h.Contains("refreshToken"));
            if(refreshTokenCookie != null)
            {
                var refreshToken = refreshTokenCookie.Split(";").FirstOrDefault() ?? throw new Exception("Cannot parse refresh token from response");
                var indexOfEquals = refreshToken?.IndexOf("=") ?? -1;
                if (indexOfEquals > -1)
                {
                    return Uri.UnescapeDataString(refreshToken?.Substring(indexOfEquals + 1) ?? throw new Exception("Cannot parse refresh token from response"));
                }
            }
        }
        throw new Exception("Cannot parse refresh token from response");
    }
}

