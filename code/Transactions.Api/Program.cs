using Transactions.Api.ExtensionMethods;
using Transactions.Api.Settings;
using Transactions.Domain;
using Transactions.Infra.Settings;
using FluentValidation;
using FluentValidation.AspNetCore;
using Libs.Api.ErrorHandling.Attributes;
using Libs.Api.Logging;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Reflection;
using System.Text;
using Grafana.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: "Transaction.Api");

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: "Transactions.Api"));

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .UseGrafana()
            .SetResourceBuilder(resourceBuilder)
            .AddHttpClientInstrumentation();
    });

builder.Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .UseGrafana()
            .SetResourceBuilder(resourceBuilder)
            .AddHttpClientInstrumentation();
    });

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.UseGrafana()
        .SetResourceBuilder(resourceBuilder);
});

// Add services to the container.

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ErrorFilterAttribute>();
        options.Filters.Add<CustomExceptionFilter>();
    })
    .AddNewtonsoftJson()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(DomainAssembly).Assembly);
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Logging.ClearProviders();
builder.Logging.AddProvider(new CustomLoggerProvider());

//Dependency injection
builder.Services.AddDependencies();
builder.Services.Configure<EmailingSettings>(builder.Configuration.GetSection("EmailingSettings"));
builder.Services.Configure<CustomClaimSettings>(builder.Configuration.GetSection("CustomClaims"));

var mongoConn = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
if(mongoConn is null) throw new ArgumentNullException(nameof(MongoDbSettings));

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
    mongoConn.IsDevelopment = true;
}

builder.Services.AddMongoDbContext(mongoConn);
builder.Services.AddGraphQL();

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JWT:key").Value);
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    c.DescribeAllParametersInCamelCase();

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJI...\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddLocalization();
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("pt-BR")
};
var localizationOptions = new RequestLocalizationOptions()
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = localizationOptions.DefaultRequestCulture;
    options.SupportedCultures = localizationOptions.SupportedCultures;
    options.SupportedUICultures = localizationOptions.SupportedUICultures;
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRequestLocalization(localizationOptions);

app.MapControllers();
app.MapGraphQL();

app.Run();

//Access point for integrated Tests
public partial class Program
{
}