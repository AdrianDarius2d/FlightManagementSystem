using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlightManagementSystem.Controllers
{
    /// <summary>
    /// Manages passenger profiles (REQ-14 to REQ-18).
    /// Passengers manage their own profile; Staff/Admin can manage all profiles.
    /// </summary>
    [Authorize]
    public class PassengerController : Controller
    {
        private readonly IPassengerService _passengerService;
        private readonly UserManager<IdentityUser> _userManager;

        public PassengerController(IPassengerService passengerService, UserManager<IdentityUser> userManager)
        {
            _passengerService = passengerService;
            _userManager = userManager;
        }

        // Staff / Admin: list all passengers with optional search (REQ-17)
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index(string? search)
        {
            IEnumerable<Passenger> passengers;

            if (!string.IsNullOrWhiteSpace(search))
                passengers = await _passengerService.SearchAsync(search);
            else
                passengers = await _passengerService.GetAllAsync();

            ViewBag.Search = search;
            return View(passengers);
        }

        // Details (Admin/Staff only)
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Details(int id)
        {
            var passenger = await _passengerService.GetByIdAsync(id);
            if (passenger == null) return NotFound();
            return View(passenger);
        }

        // Create passenger profile (the logged-in user creating their own profile, or Staff adding one)
        public IActionResult Create(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new Passenger());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Passenger passenger, string? returnUrl)
        {
            if (!ModelState.IsValid) return View(passenger);

            var userId = _userManager.GetUserId(User);

            // If the user doesn't have a profile yet, link this one to their account
            if (!await _passengerService.ExistsForUserAsync(userId!))
                passenger.IdentityUserId = userId;

            await _passengerService.CreateAsync(passenger);
            TempData["Success"] = "Passenger profile created successfully.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Reservation");
        }

        // Edit own profile (logged-in user) or any profile (Staff/Admin)
        public async Task<IActionResult> Edit(int id)
        {
            var passenger = await _passengerService.GetByIdAsync(id);
            if (passenger == null) return NotFound();

            // Non-staff users can only edit their own profile
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var userId = _userManager.GetUserId(User);
                if (passenger.IdentityUserId != userId) return Forbid();
            }

            return View(passenger);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Passenger passenger)
        {
            if (id != passenger.Id) return BadRequest();
            if (!ModelState.IsValid) return View(passenger);

            await _passengerService.UpdateAsync(passenger);
            TempData["Success"] = "Profile updated.";
            return RedirectToAction("Index", "Reservation");
        }

        // Delete (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var passenger = await _passengerService.GetByIdAsync(id);
            if (passenger == null) return NotFound();
            return View(passenger);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _passengerService.DeleteAsync(id);
            TempData["Success"] = "Passenger deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Allows a passenger to view and edit their own profile
        public async Task<IActionResult> MyProfile()
        {
            var userId = _userManager.GetUserId(User);
            var passenger = await _passengerService.GetByIdentityUserIdAsync(userId!);

            if (passenger == null)
                return RedirectToAction(nameof(Create));

            return RedirectToAction(nameof(Edit), new { id = passenger.Id });
        }
    }
}
