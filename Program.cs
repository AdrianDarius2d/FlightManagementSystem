using FlightManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Flights.Any())
    {
        context.Flights.AddRange(
            new FlightManagementSystem.Models.Flight
            {
                FlightNumber = "FMS001",
                Source = "London",
                Destination = "Paris",
                DepartureTime = DateTime.Now.AddDays(1),
                ArrivalTime = DateTime.Now.AddDays(1).AddHours(2),
                Capacity = 150
            },
            new FlightManagementSystem.Models.Flight
            {
                FlightNumber = "FMS002",
                Source = "Paris",
                Destination = "Rome",
                DepartureTime = DateTime.Now.AddDays(2),
                ArrivalTime = DateTime.Now.AddDays(2).AddHours(3),
                Capacity = 120
            },
            new FlightManagementSystem.Models.Flight
            {
                FlightNumber = "FMS003",
                Source = "Rome",
                Destination = "London",
                DepartureTime = DateTime.Now.AddDays(3),
                ArrivalTime = DateTime.Now.AddDays(3).AddHours(2),
                Capacity = 200
            }
        );
        context.SaveChanges();
    }
}

app.Run();
