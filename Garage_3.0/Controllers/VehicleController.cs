using Garage_3._0.ConstantValues;
using Garage_3._0.Data;
using Garage_3._0.Models;
using Garage_3._0.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Garage_3._0.Controllers
{
    [Authorize]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public VehicleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Vehicle
        [Authorize]
        public async Task<IActionResult> Index(string? search)
        {
            ViewData["CurrentFilter"] = search;

            var query = _context.Vehicles
                .Include(v => v.Owner)
                .Include(v => v.VehicleType)
                .AsQueryable();

            if (!User.IsInRole(RolesNames.Admin))
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(v => v.OwnerId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                var sPlate = s.Replace(" ", "").ToUpperInvariant();

                query = query.Where(v =>
                    v.LicenseNumber.Contains(sPlate) ||
                    v.Model.Contains(s) ||
                    v.Color.Contains(s) ||
                    v.VehicleType!.Name.Contains(s)
                );
            }

            var vehicles = await query.ToListAsync();
            return View(vehicles);
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .Include(v => v.VehicleType)
                .Include(v => v.ParkingSpots)        
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            VehicleDetailsViewModel viewModel = new() {
                Id = vehicle.Id,
                LicenseNumber = vehicle.LicenseNumber,
                VehicleType = vehicle.VehicleType,
                Color = vehicle.Color,
                Model = vehicle.Model,
                NumberOfWheels = vehicle.NumberOfWheels,
                ArrivalTime = vehicle.ArrivalTime,
                OwnerName = $"{vehicle.Owner?.FirstName} {vehicle.Owner?.LastName}",
                OwnerEmail = vehicle.Owner?.Email,
                ParkingSpots = vehicle.ParkingSpots,
            };

            return View(viewModel);
        }

        // GET: Vehicle/Create
        [Authorize(Roles = $"{RolesNames.Admin},{RolesNames.Member}")]
        public IActionResult Create()
        {
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes.OrderBy(v => v.Name), "Id", "Name");

            if (User.IsInRole("Admin"))
                ViewData["OwnerId"] = new SelectList(_context.Users.OrderBy(u => u.Email), "Id", "Email");

            return View();
        }


        // POST: Vehicle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{RolesNames.Admin},{RolesNames.Member}")]
        public async Task<IActionResult> Create(
                [Bind("LicenseNumber,Model,Color,NumberOfWheels,VehicleTypeId")] Vehicle vehicle,
                string? ownerId
)
        {
            if (ModelState.IsValid)
            {
                vehicle.LicenseNumber = (vehicle.LicenseNumber ?? "").Replace(" ", "").ToUpperInvariant();

                if (User.IsInRole("Admin"))
                {
                    vehicle.OwnerId = string.IsNullOrWhiteSpace(ownerId)
                        ? _userManager.GetUserId(User)!
                        : ownerId;
                }
                else
                {
                    vehicle.OwnerId = _userManager.GetUserId(User)!;
                }

                vehicle.ArrivalTime = DateTime.Now;

                _context.Add(vehicle);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("LicenseNumber", "License number must be unique.");
                }
            }

            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes.OrderBy(v => v.Name), "Id", "Name", vehicle.VehicleTypeId);
            if (User.IsInRole("Admin"))
                ViewData["OwnerId"] = new SelectList(_context.Users.OrderBy(u => u.Email), "Id", "Email", ownerId);

            return View(vehicle);
        }

        // GET: Vehicle/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "Id", vehicle.OwnerId);
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Id", vehicle.VehicleTypeId);
            return View(vehicle);
        }

        // POST: Vehicle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LicenseNumber,ParkedDuration,Model,Color,NumberOfWheels,ArrivalTime,OwnerId,VehicleTypeId")] Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "Id", vehicle.OwnerId);
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Id", vehicle.VehicleTypeId);
            return View(vehicle);
        }

        // GET: Vehicle/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}
