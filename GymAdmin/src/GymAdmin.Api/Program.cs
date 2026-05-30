using System.Text;
using GymAdmin.Api.Middleware;
using GymAdmin.Application.Services;
using GymAdmin.Infrastructure.Data;
using GymAdmin.Infrastructure.Seed;
using GymAdmin.Api.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ===== HOST =====
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// ===== Database =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (connectionString?.Contains("Data Source=") == true)
        options.UseSqlite(connectionString);
    else
        options.UseNpgsql(connectionString);
});

// ===== Services =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IRoutineService, RoutineService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGymService, GymService>();
builder.Services.AddScoped<IMembershipPlanService, MembershipPlanService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IIngresoService, IngresoService>();
builder.Services.AddScoped<IEgresoService, EgresoService>();

builder.Services.AddHostedService<StartupNotificationService>();

// ===== Authentication =====
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("X-Total-Count", "X-Page", "X-Page-Size");
    });
});

// ===== Controllers =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ===== Auto Migrations =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (db.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
    {
        try
        {
            var conn = db.Database.GetDbConnection();
            var wasClosed = conn.State == System.Data.ConnectionState.Closed;
            if (wasClosed) await conn.OpenAsync();

            using var cmdCheck = conn.CreateCommand();
            cmdCheck.CommandText = "SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Gyms');";
            var gymsTableExists = (bool)(await cmdCheck.ExecuteScalarAsync() ?? false);

            if (gymsTableExists)
            {
                using var cmdCreateHistory = conn.CreateCommand();
                cmdCreateHistory.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                        ""MigrationId"" character varying(150) NOT NULL,
                        ""ProductVersion"" character varying(32) NOT NULL,
                        CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                    );";
                await cmdCreateHistory.ExecuteNonQueryAsync();

                using var cmdCheckMigration = conn.CreateCommand();
                cmdCheckMigration.CommandText = "SELECT EXISTS (SELECT 1 FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '20260521144311_InitialPostgres');";
                var migrationExists = (bool)(await cmdCheckMigration.ExecuteScalarAsync() ?? false);

                if (!migrationExists)
                {
                    using var cmdInsertMigration = conn.CreateCommand();
                    cmdInsertMigration.CommandText = "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20260521144311_InitialPostgres', '8.0.11');";
                    await cmdInsertMigration.ExecuteNonQueryAsync();
                }
            }

            if (wasClosed) await conn.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not align migration history: {ex.Message}");
        }
    }

    db.Database.Migrate();
}

// ===== Middleware Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ===== Seed Database =====
await DbSeeder.SeedAsync(app.Services);

app.Run();
