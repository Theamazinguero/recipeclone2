// Program.cs

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipeBackend.Data;
using RecipeBackend.Models;
using RecipeBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------- CORS -----------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // For dev: allow any origin. You can restrict this later.
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ----------------- JWT CONFIG -----------------
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super-secret-key-change-me";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RecipeApp";

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// ----------------- DB CONTEXT -----------------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ----------------- IDENTITY -----------------
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// ----------------- AUTH (JWT) -----------------
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // dev only
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = false,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// ----------------- SERVICES -----------------
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ----------------- AUTO DB CREATE + SEED -----------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Get the DbContext
    var db = services.GetRequiredService<AppDbContext>();

    // Automatically create the database if it doesn't exist
    // (no need for dotnet ef migrations on dev machines)
    db.Database.EnsureCreated();

    // Seed roles + admin user
    await SeedData.SeedRolesAndAdminAsync(services, jwtIssuer);
}

// ----------------- MIDDLEWARE PIPELINE -----------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
