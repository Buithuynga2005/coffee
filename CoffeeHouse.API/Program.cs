using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// CORS
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// =====================================================
// JWT
// =====================================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? "COFFEE_HOUSE_SECRET_KEY_VERY_LONG_AND_SECURE_123456";

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? "CoffeeHouseAPI";

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? "CoffeeHouseApp";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                )
        };
    });

// =====================================================
// AUTHORIZATION
// =====================================================

builder.Services.AddAuthorization();

// =====================================================
// MYSQL CONNECTION FACTORY
// =====================================================

builder.Services.AddScoped<
    CoffeeHouse.API.Data.MySqlConnectionFactory
>();

// =====================================================
// CONTROLLERS
// =====================================================

builder.Services.AddControllers();

// =====================================================
// BUILD APPLICATION
// =====================================================

var app = builder.Build();

// =====================================================
// CORS
// =====================================================

app.UseCors("AllowAll");

// =====================================================
// HTTPS
// =====================================================

// Tạm thời bỏ UseHttpsRedirection()
// để tránh cảnh báo:
// "Failed to determine the https port for redirect."

// app.UseHttpsRedirection();

// =====================================================
// AUTHENTICATION
// =====================================================

app.UseAuthentication();

// =====================================================
// AUTHORIZATION
// =====================================================

app.UseAuthorization();

// =====================================================
// CONTROLLERS
// =====================================================

app.MapControllers();

// =====================================================
// RUN
// =====================================================

app.Run();