using GracelineCMS.Auth;
using GracelineCMS.Domain.Auth;
using GracelineCMS.Domain.Communication;
using GracelineCMS.Infrastructure.Auth;
using GracelineCMS.Infrastructure.Communication;
using GracelineCMS.Infrastructure.Repository;
using GracelineCMS.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//config
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddHttpContextAccessor();

//auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secret = builder.Configuration.GetValue<string>("AuthenticationSigningSecret") ?? throw new ArgumentNullException("Missing AuthenticationSigningSecret config");
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(secret))
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GlobalAdmin", policy =>
    {
        policy.RequireRole("GlobalAdmin");
    });
    options.AddPolicy("OrganizationAdmin", policy =>
    {
        policy.Requirements.Add(new OrganizationAdminRequirement());
    });
});
builder.Services.AddSingleton<IAuthorizationHandler, OrganizationAdminRequirementHandler>();
builder.Services.AddSingleton<IClaimsProvider, ClaimsProvider>(options =>
{
    var globalAdminEmail = builder.Configuration.GetValue<string>("GlobalAdminEmail") ?? throw new ArgumentNullException("Missing GlobalAdminEmail in config");
    return new ClaimsProvider(globalAdminEmail);
});
builder.Services.AddSingleton<ITokenHandler>(options =>
{
    var secret = options.GetRequiredService<IConfiguration>()["AuthenticationSigningSecret"] ?? throw new ArgumentNullException("Missing AuthenticationSigningSecret config");
    return new AppTokenHandler(options.GetRequiredService<IClaimsProvider>(), secret, options.GetRequiredService<IDbContextFactory<AppDbContext>>());
});

//app services
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetSection("ConnectionStrings").GetValue<string>("DefaultConnection"),
        optionsBuilder =>
        {
            optionsBuilder.MigrationsAssembly("GracelineCMS");
        }
    )
);
builder.Services.AddSingleton<IEmailClient, GmailClient>(sp =>
{
    if(builder.Configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
    {
        var credentialFile = builder.Configuration.GetValue<string>("GOOGLE_SMTP_SA_CREDENTIAL") ?? throw new ArgumentNullException("GOOGLE_SMTP_SA_CREDENTIAL");
        var base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(credentialFile)));
        return new GmailClient(base64Encoded);
    }
    
    var encodedCredential = builder.Configuration.GetValue<string>("GOOGLE_SMTP_SA_CREDENTIAL") ?? throw new ArgumentNullException("GOOGLE_SMTP_SA_CREDENTIAL");
    return new GmailClient(encodedCredential);
});
builder.Services.AddSingleton<IAuthenticationCodeEmail, AuthenticationCodeEmail>();
builder.Services.AddSingleton(sp =>
{
    var defaultFromAddress = sp.GetRequiredService<IConfiguration>().GetValue<string>("DefaultFromAddress") ?? throw new ArgumentNullException("Missing DefaultFromAddress in config");
    var defaultFromName = sp.GetRequiredService<IConfiguration>().GetValue<string>("DefaultFromName") ?? defaultFromAddress;
    return new EmailCreator(new DefaultEmailAddressConfig
    {
        FromAddress = defaultFromAddress,
        FromName = defaultFromName
    });
});
builder.Services.AddSingleton<IAuthenticationCode, AuthenticationCode>();


//core
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GracelineCMS", Version = "v1" });

    c.SupportNonNullableReferenceTypes();

    // Add the custom operation filter to all endpoints
    c.OperationFilter<ProblemDetailsOperationFilter>();
});

builder.Services.AddHealthChecks()
    .AddCheck<ReadinessHealthCheck>("readiness");
builder.Services.AddCors(config =>
{
    config.AddDefaultPolicy(policy =>
    {
        var allowedHosts = builder.Configuration.GetValue<string>("AllowedOrigins")?.Split(";") ?? [];
        policy.WithOrigins(allowedHosts).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});


var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Name == "readiness"
});

app.UseMiddleware<ExceptionHandlingMiddleware>();


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();

public partial class Program { }

public class ReadinessHealthCheck() : IHealthCheck
{
    private volatile bool _isReady = false;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        _isReady = true;
        return _isReady ? Task.FromResult(HealthCheckResult.Healthy()) : Task.FromResult(HealthCheckResult.Unhealthy());
    }

    // This method can be called once the app is fully ready (e.g., after migrations)
    public void SetReady() => _isReady = true;
}