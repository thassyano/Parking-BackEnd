using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Estacionamento.Api.Infrastructure.Data;
using Estacionamento.Api.Infrastructure.Repositories;
using Estacionamento.Api.Application.Services;
using BCrypt.Net;
using Estacionamento.Api.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Estacionamento API", 
        Version = "v1",
        Description = "API para gerenciamento de estacionamento"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Use InMemory database if no connection string is provided
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("EstacionamentoDb"));
}
else
{
    // Use PostgreSQL (Supabase)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// JWT Authentication
try
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "EstacionamentoSecretKey12345678901234567890";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EstacionamentoApi";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EstacionamentoApi";

    if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    {
        throw new InvalidOperationException("JWT Key deve ter pelo menos 32 caracteres");
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao configurar JWT: {ex.Message}");
    throw;
}

builder.Services.AddAuthorization();

// CORS
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(';') 
    ?? new[] { "http://localhost:4200", "https://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Em desenvolvimento, permite qualquer origem
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // Em produção, permite apenas origens específicas
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// Register Repositories
builder.Services.AddScoped<IVagaRepository, VagaRepository>();
builder.Services.AddScoped<IOcupacaoRepository, OcupacaoRepository>();
builder.Services.AddScoped<IPrecoRepository, PrecoRepository>();

// Register Services
builder.Services.AddScoped<IVagaService, VagaService>();
builder.Services.AddScoped<IOcupacaoService, OcupacaoService>();
builder.Services.AddScoped<IPrecoService, PrecoService>();

WebApplication app;
try
{
    app = builder.Build();
    Console.WriteLine("Aplicação construída com sucesso");
}
catch (Exception ex)
{
    Console.WriteLine($"ERRO ao construir aplicação: {ex.GetType().Name}");
    Console.WriteLine($"Mensagem: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
    }
    throw;
}

// Configure the HTTP request pipeline.
try
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Estacionamento API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
    });

    // HTTPS redirection - desabilitado no Railway (ele gerencia via proxy)
    var isRailway = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"));
    if (!isRailway)
    {
        app.UseHttpsRedirection();
    }

    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", () => new { 
        status = "ok", 
        timestamp = DateTime.UtcNow,
        environment = app.Environment.EnvironmentName,
        hasConnectionString = !string.IsNullOrEmpty(connectionString)
    });

    Console.WriteLine("Pipeline configurado com sucesso");
}
catch (Exception ex)
{
    Console.WriteLine($"ERRO ao configurar pipeline: {ex.GetType().Name}");
    Console.WriteLine($"Mensagem: {ex.Message}");
    throw;
}

// Seed initial data - removido temporariamente para debug
// Será executado via endpoint ou script separado após deploy bem-sucedido

try
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    Console.WriteLine($"=== INICIANDO APLICAÇÃO ===");
    Console.WriteLine($"Porta: {port}");
    Console.WriteLine($"Ambiente: {app.Environment.EnvironmentName}");
    Console.WriteLine($"Connection String configurada: {!string.IsNullOrEmpty(connectionString)}");
    Console.WriteLine($"Railway Environment: {Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")}");
    
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"=== ERRO FATAL ===");
    Console.WriteLine($"Tipo: {ex.GetType().Name}");
    Console.WriteLine($"Mensagem: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}");
        Console.WriteLine($"Inner Message: {ex.InnerException.Message}");
    }
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    throw;
}
