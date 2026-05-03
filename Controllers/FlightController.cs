using FlightManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class FlightController : Controller
{
    private readonly ApplicationDbContext _context;

    public FlightController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Shows all flights
    public async Task<IActionResult> Index(string source, string destination, DateTime? date)
    {
        var flights = _context.Flights.AsQueryable();

        if (!string.IsNullOrEmpty(source))
            flights = flights.Where(f => f.Source.Contains(source));

        if (!string.IsNullOrEmpty(destination))
            flights = flights.Where(f => f.Destination.Contains(destination));

        if (date.HasValue)
            flights = flights.Where(f => f.DepartureTime.Date == date.Value.Date);

        return View(await flights.ToListAsync());
    }

    // Shows details of one flight
    public async Task<IActionResult> Details(string id)
    {
        var flight = await _context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == id);
        if (flight == null) return NotFound();
        return View(flight);
    }
}