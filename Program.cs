using FlightManagementSystem.Constants;
using FlightManagementSystem.Data;
using FlightManagementSystem.Middleware;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ── Identity — with roles (REQ-74) ───────────────────────────────────────────
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // REQ-1: allow registration without email confirmation in this environment
    options.SignIn.RequireConfirmedAccount = false;
    // SRS §5.3: basic password security requirements
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    // REQ-66: lockout after failed login attempts (configurable via SystemSettings)
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// ── Application Services (Service Layer, SRS §5.4.5 and §5.4.21) ─────────────
// Register INotificationService first because other services depend on it
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IErrorLogService, ErrorLogService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPassengerService, PassengerService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();

// ── Logging (SRS §5.4.25) ─────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── HTTP Pipeline ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// REQ-68, SRS §5.4.9/10: global error logging middleware (must be early in the pipeline)
app.UseMiddleware<ErrorLoggingMiddleware>();

// REQ-66, REQ-80: login attempt audit middleware
app.UseMiddleware<LoginAuditMiddleware>();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages().WithStaticAssets();

// ── Seed Data ─────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // Seed roles (REQ-74)
    foreach (var role in new[] {
        ApplicationConstants.ROLE_ADMIN,
        ApplicationConstants.ROLE_STAFF,
        ApplicationConstants.ROLE_PASSENGER })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed default admin (REQ-62, REQ-79)
    if (await userManager.FindByEmailAsync(ApplicationConstants.DEFAULT_ADMIN_EMAIL) == null)
    {
        var admin = new IdentityUser
        {
            UserName = ApplicationConstants.DEFAULT_ADMIN_EMAIL,
            Email = ApplicationConstants.DEFAULT_ADMIN_EMAIL,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, ApplicationConstants.DEFAULT_ADMIN_PASSWORD);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, ApplicationConstants.ROLE_ADMIN);
    }

    // Seed default staff
    if (await userManager.FindByEmailAsync(ApplicationConstants.DEFAULT_STAFF_EMAIL) == null)
    {
        var staff = new IdentityUser
        {
            UserName = ApplicationConstants.DEFAULT_STAFF_EMAIL,
            Email = ApplicationConstants.DEFAULT_STAFF_EMAIL,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(staff, ApplicationConstants.DEFAULT_STAFF_PASSWORD);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(staff, ApplicationConstants.ROLE_STAFF);
    }

    // Seed sample flights
    if (!context.Flights.Any())
    {
        context.Flights.AddRange(
            new Flight
            {
                FlightNumber = "BTA001",
                Source = "London",
                Destination = "Paris",
                DepartureTime = DateTime.Now.AddDays(2),
                ArrivalTime = DateTime.Now.AddDays(2).AddHours(2),
                Capacity = 150,
                AircraftType = "Boeing 737",
                Price = 120.00m,
                Status = ApplicationConstants.FLIGHT_STATUS_SCHEDULED
            },
            new Flight
            {
                FlightNumber = "BTA002",
                Source = "Paris",
                Destination = "Rome",
                DepartureTime = DateTime.Now.AddDays(3),
                ArrivalTime = DateTime.Now.AddDays(3).AddHours(3),
                Capacity = 120,
                AircraftType = "Airbus A320",
                Price = 95.00m,
                Status = ApplicationConstants.FLIGHT_STATUS_SCHEDULED
            },
            new Flight
            {
                FlightNumber = "BTA003",
                Source = "Rome",
                Destination = "Madrid",
                DepartureTime = DateTime.Now.AddDays(4),
                ArrivalTime = DateTime.Now.AddDays(4).AddHours(2),
                Capacity = 200,
                AircraftType = "Boeing 777",
                Price = 145.00m,
                Status = ApplicationConstants.FLIGHT_STATUS_SCHEDULED
            },
            new Flight
            {
                FlightNumber = "BTA004",
                Source = "Madrid",
                Destination = "London",
                DepartureTime = DateTime.Now.AddDays(5),
                ArrivalTime = DateTime.Now.AddDays(5).AddHours(2),
                Capacity = 180,
                AircraftType = "Airbus A380",
                Price = 110.00m,
                Status = ApplicationConstants.FLIGHT_STATUS_SCHEDULED
            },
            new Flight
            {
                FlightNumber = "BTA005",
                Source = "London",
                Destination = "New York",
                DepartureTime = DateTime.Now.AddDays(7),
                ArrivalTime = DateTime.Now.AddDays(7).AddHours(8),
                Capacity = 300,
                AircraftType = "Boeing 787",
                Price = 450.00m,
                Status = ApplicationConstants.FLIGHT_STATUS_SCHEDULED
            }
        );
        await context.SaveChangesAsync();
    }

    // Seed system settings (REQ-65, REQ-72)
    if (!context.SystemSettings.Any())
    {
        context.SystemSettings.AddRange(
            // ── General settings ─────────────────────────────────────────────
            new SystemSetting
            {
                Key = ApplicationConstants.SETTING_MAINTENANCE_MODE,
                Value = "false",
                DisplayName = "Maintenance Mode",
                Category = "General",
                Description = "When enabled, users see a maintenance notice instead of normal pages.",
                InputType = "boolean"
            },
            new SystemSetting
            {
                Key = ApplicationConstants.SETTING_BACKUP_FREQUENCY_HOURS,
                Value = "24",
                DisplayName = "Auto-Backup Frequency (hours)",
                Category = "Backup",
                Description = "How often automatic backups are triggered.",
                InputType = "number"
            },
            // ── Security settings (REQ-72) ────────────────────────────────────
            new SystemSetting
            {
                Key = ApplicationConstants.SETTING_MAX_LOGIN_ATTEMPTS,
                Value = "5",
                DisplayName = "Max Failed Login Attempts",
                Category = "Security",
                Description = "Account is locked after this many consecutive failed attempts. (REQ-66)",
                InputType = "number"
            },
            new SystemSetting
            {
                Key = ApplicationConstants.SETTING_LOCKOUT_DURATION_MINUTES,
                Value = "15",
                DisplayName = "Lockout Duration (minutes)",
                Category = "Security",
                Description = "How long an account remains locked after too many failed logins.",
                InputType = "number"
            },
            new SystemSetting
            {
                Key = ApplicationConstants.SETTING_SESSION_TIMEOUT_MINUTES,
                Value = "60",
                DisplayName = "Session Timeout (minutes)",
                Category = "Security",
                Description = "Idle session expiry time.",
                InputType = "number"
            },
            new SystemSetting
            {
                Key = ApplicationConstants.SETTING_MIN_PASSWORD_LENGTH,
                Value = "6",
                DisplayName = "Minimum Password Length",
                Category = "Security",
                Description = "Minimum number of characters required for user passwords.",
                InputType = "number"
            },
            new SystemSetting
            {
                Key = ApplicationConstants.SETTING_REQUIRE_STRONG_PASSWORD,
                Value = "true",
                DisplayName = "Require Strong Password",
                Category = "Security",
                Description = "Require uppercase, digit, and special character in passwords.",
                InputType = "boolean"
            },
            // ── Notification settings (REQ-52) ────────────────────────────────
            new SystemSetting
            {
                Key = ApplicationConstants.SETTING_NOTIFICATION_EMAIL_ENABLED,
                Value = "false",
                DisplayName = "Email Notifications Enabled",
                Category = "Notifications",
                Description = "Send notifications by email in addition to in-app alerts.",
                InputType = "boolean"
            },
            new SystemSetting
            {
                Key = ApplicationConstants.SETTING_SMTP_HOST,
                Value = "smtp.bermudatriangle.com",
                DisplayName = "SMTP Host",
                Category = "Notifications",
                Description = "Outbound email server hostname.",
                InputType = "text"
            },
            new SystemSetting
            {
                Key = ApplicationConstants.SETTING_SMTP_PORT,
                Value = "587",
                DisplayName = "SMTP Port",
                Category = "Notifications",
                Description = "Outbound email server port.",
                InputType = "number"
            }
        );
        await context.SaveChangesAsync();
    }
}

app.Run();
