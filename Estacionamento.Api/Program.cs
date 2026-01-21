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
var jwtKey = builder.Configuration["Jwt:Key"] ?? "EstacionamentoSecretKey12345678901234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EstacionamentoApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EstacionamentoApi";

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

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Ensure database is created (only for InMemory, for PostgreSQL use migrations)
        if (string.IsNullOrEmpty(connectionString))
        {
            context.Database.EnsureCreated();
        }

        // Seed Admin user if none exists
        if (!context.Admins.Any())
        {
            var admin = new Admin
            {
                Usuario = "admin",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Email = "admin@estacionamento.com",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };
            context.Admins.Add(admin);
            context.SaveChanges();
            logger.LogInformation("Admin padrão criado: admin / admin123");
        }

        // Seed initial price if none exists
        if (!context.Precos.Any())
        {
            var preco = new Preco
            {
                ValorHora = 10.00m,
                ValorMinuto = 0.17m, // R$ 10.00 / 60 minutos
                DataInicio = DateTime.UtcNow,
                Ativo = true
            };
            context.Precos.Add(preco);
            context.SaveChanges();
            logger.LogInformation("Preço padrão criado: R$ 10,00/hora");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao inicializar dados do banco");
    }
}

app.Run();
