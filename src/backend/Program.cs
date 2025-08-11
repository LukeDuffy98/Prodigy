using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using Prodigy.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
Console.WriteLine($"Looking for .env file at: {envPath}");
Console.WriteLine($"File exists: {File.Exists(envPath)}");

// In production, environment variables are set via Azure App Service configuration
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
Console.WriteLine($"Starting Prodigy API in {environment} environment");

if (environment == "Development" && File.Exists(envPath))
{
    Env.Load(envPath);
}
else
{
    Console.WriteLine($"Using environment variables from hosting environment ({environment})");
}

// Debug: Log environment variables to verify they're loaded (but don't log secrets in production)
Console.WriteLine($"AZURE_TENANT_ID: {Environment.GetEnvironmentVariable("AZURE_TENANT_ID")}");
Console.WriteLine($"AZURE_CLIENT_ID: {Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")}");
if (environment == "Development")
{
    Console.WriteLine($"AZURE_CLIENT_SECRET: {(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")) ? "NOT SET" : "SET")}");
}

Console.WriteLine("Application URLs will be configured by the hosting environment");
Console.WriteLine("Starting application...");

// Add services to the container.
builder.Services.AddControllers();

// Add HTTP client for Azure Functions
builder.Services.AddHttpClient("AzureFunctions", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AZURE_FUNCTIONS_URL"] ?? "http://localhost:7071/api/");
    client.DefaultRequestHeaders.Add("x-functions-key", builder.Configuration["AZURE_FUNCTIONS_KEY"] ?? "");
});

// Add default HTTP client for general use (needed for authentication proxy)
builder.Services.AddHttpClient();

// Add CORS for frontend integration - Updated to include port 5174
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProdigyFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "https://localhost:5173", "http://localhost:5173", "http://localhost:5174", "https://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add JWT Authentication (placeholder configuration)
var jwtKey = builder.Configuration["JWT_SECRET_KEY"] ?? "ProdigyDefaultKeyForDevelopment";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false; // Set to true in production
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["JWT_ISSUER"] ?? "Prodigy",
        ValidAudience = builder.Configuration["JWT_AUDIENCE"] ?? "ProdigyUsers",
        ClockSkew = TimeSpan.Zero
    };
})
.AddJwtBearer("AzureAD", options =>
{
    // Use the common Microsoft endpoint that handles multi-tenant applications
    options.Authority = "https://login.microsoftonline.com/common/v2.0";
    options.RequireHttpsMetadata = false; // Allow HTTP for development
    options.SaveToken = true;
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true, // Enable audience validation for Microsoft Graph
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        
        // Accept tokens from BOTH v1.0 and v2.0 Azure AD endpoints for our tenant
        ValidIssuers = new[] {
            $"https://login.microsoftonline.com/{Environment.GetEnvironmentVariable("AZURE_TENANT_ID")}/v2.0", // v2.0 endpoint
            $"https://sts.windows.net/{Environment.GetEnvironmentVariable("AZURE_TENANT_ID")}/" // v1.0 endpoint
        },
        
        // Accept Microsoft Graph as the valid audience
        ValidAudiences = new[] { 
            "https://graph.microsoft.com",
            "00000003-0000-0000-c000-000000000000" // Microsoft Graph resource ID
        },
        
        // Clock skew
        ClockSkew = TimeSpan.FromMinutes(5) // More lenient clock skew for development
    };
    
    // Add event handlers for debugging token validation
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            Console.WriteLine($"[JWT] Token validated successfully for user: {context.Principal?.Identity?.Name}");
            var claims = context.Principal?.Claims?.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine($"[JWT] Token claims: {string.Join(", ", claims ?? new string[0])}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[JWT] Authentication failed: {context.Exception?.Message}");
            Console.WriteLine($"[JWT] Exception details: {context.Exception}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"[JWT] Authentication challenge: {context.Error}, {context.ErrorDescription}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine($"[JWT] Token received from authorization header");
            return Task.CompletedTask;
        }
    };
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAzureAD", policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes("AzureAD"));
    
    options.AddPolicy("RequireJWT", policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Prodigy API", 
        Version = "v1",
        Description = "Prodigy Intelligent Digital Workspace API - Your AI-powered productivity platform with personalized agents for email, tasks, learning, quotes, calendar management, and GitHub integration."
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Register Graph Email Services
builder.Services.AddScoped<IGraphEmailService, GraphEmailService>();
builder.Services.AddScoped<IGraphUserService, GraphUserService>();

var app = builder.Build();

// Add startup logging
Console.WriteLine($"Starting Prodigy API in {app.Environment.EnvironmentName} environment");
Console.WriteLine($"Application URLs will be configured by the hosting environment");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prodigy API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
}
else
{
    // Enable Swagger in production for Azure deployment debugging
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prodigy API v1");
        c.RoutePrefix = "swagger"; // Serve Swagger UI at /swagger in production
    });
}

app.UseHttpsRedirection();

app.UseCors("ProdigyFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint - high priority, defined early
app.MapGet("/health", () => 
{
    Console.WriteLine("Health check endpoint accessed");
    return new { Status = "Healthy", Timestamp = DateTime.UtcNow, Version = "1.0.0", Environment = app.Environment.EnvironmentName };
});

// Welcome endpoint for root
app.MapGet("/", () => new { 
    Message = "Welcome to Prodigy API", 
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Swagger = app.Environment.IsDevelopment() ? "/" : "/swagger",
    Endpoints = new { 
        Health = "/health",
        PersonalizationProfile = "/api/user/personalization-profile",
        EmailAgent = "/api/agents/email",
        TaskAgent = "/api/agents/tasks",
        LearningAgent = "/api/agents/learning",
        QuoteAgent = "/api/agents/quotes",
        CalendarAgent = "/api/agents/calendar",
        GitHubAgent = "/api/agents/github"
    }
});

Console.WriteLine("Starting application...");
app.Run();
