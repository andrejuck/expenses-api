using Expenses.Api.ExtensionMethods;
using Expenses.Api.Settings;
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
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var isTesting = Environment.GetEnvironmentVariable("DOTNET_INTEGRATION_TESTS") == "true";
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

builder.Logging.ClearProviders();
builder.Logging.AddProvider(new CustomLoggerProvider());

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

//Dependency injection
builder.Services.AddExpensesDependencies();
builder.Services.Configure<EmailingSettings>(builder.Configuration.GetSection("EmailingSettings"));
builder.Services.Configure<CustomClaimSettings>(builder.Configuration.GetSection("CustomClaims"));

var mongoConn = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
if (!isTesting) builder.Services.AddMongoDbContext(mongoConn);

builder.Services.AddAutoMapper(typeof(Program));
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
        Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJI...\""
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
public partial class Program { }
