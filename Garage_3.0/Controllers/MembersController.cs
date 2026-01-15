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
    [Authorize(Roles = "Admin")]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public MembersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        // GET: Members
        public async Task<IActionResult> Index(string searchString, string membershipFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["MembershipFilter"] = membershipFilter;

            // Get all users
            var usersQuery = _userManager.Users.AsQueryable();

            // Search by name or email
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.Contains(searchString) ||
                    u.LastName.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    u.PersonalNumber.Contains(searchString));
            }

            // Filter by membership
            if (!string.IsNullOrEmpty(membershipFilter))
            {
                if (membershipFilter == "pro")
                {
                    usersQuery = usersQuery.Where(u => u.IsProMember && u.ProMembershipExpiry > DateTime.Now);
                }
                else if (membershipFilter == "basic")
                {
                    usersQuery = usersQuery.Where(u => !u.IsProMember || u.ProMembershipExpiry <= DateTime.Now);
                }
            }

            var users = await usersQuery.ToListAsync();

            // Get vehicle counts and current costs for each user
            var memberDataList = new List<MemberViewModel>();

            foreach (var user in users)
            {
                var vehicles = await _context.Vehicles
                    .Include(v => v.ParkingSpots.Where(ps => ps.IsTaken))
                    .Where(v => v.OwnerId == user.Id)
                    .ToListAsync();

                //var activeParking = vehicles
                //    .SelectMany(v => v.ParkingSpots.Where(ps => ps.IsTaken))
                //    .ToList();

                decimal totalCurrentCost = 0;
                var hourlyRate = _configuration.GetValue<decimal>("GarageSettings:HourlyRate", 20m);
                var isProMember = user.IsProMember && user.ProMembershipExpiry > DateTime.Now;

                //foreach (var session in activeParking)
                //{
                //    totalCurrentCost += session.CalculateCost(hourlyRate, isProMember);
                //}

                var roles = await _userManager.GetRolesAsync(user);

                memberDataList.Add(new MemberViewModel
                {
                    User = user,
                    VehicleCount = vehicles.Count,
                    //ActiveParkingCount = activeParking.Count,
                    TotalCurrentCost = totalCurrentCost,
                    IsProMember = isProMember,
                    Roles = roles.ToList()
                });
            }

            return View(memberDataList);
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Get user's vehicles with parking sessions
            var vehicles = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Parkings)
                    .ThenInclude(ps => ps.ParkingSpot)
                .Where(v => v.OwnerId == id)
                .ToListAsync();

            // Calculate statistics
            var hourlyRate = _configuration.GetValue<decimal>("GarageSettings:HourlyRate", 20m);
            var isProMember = user.IsProMember && user.ProMembershipExpiry > DateTime.Now;

            var activeSessions = vehicles
                .SelectMany(v => v.Parkings.Where(ps => ps.IsActive))
                .ToList();

            decimal totalCurrentCost = 0;
            foreach (var session in activeSessions)
            {
                totalCurrentCost += session.CalculateCost(hourlyRate, isProMember);
            }

            // Calculate total historical revenue
            var allCompletedSessions = vehicles
                .SelectMany(v => v.Parkings.Where(ps => !ps.IsActive))
                .ToList();

            decimal totalHistoricalRevenue = allCompletedSessions.Sum(ps => ps.Cost);
            decimal totalRevenue = totalHistoricalRevenue + totalCurrentCost;

            var roles = await _userManager.GetRolesAsync(user);

            var viewModel = new MemberDetailsViewModel
            {
                User = user,
                Vehicles = vehicles,
                ActiveSessions = activeSessions,
                TotalCurrentCost = totalCurrentCost,
                TotalHistoricalRevenue = totalHistoricalRevenue,
                TotalRevenue = totalRevenue,
                TotalParkings = vehicles.SelectMany(v => v.Parkings).Count(),
                IsProMember = isProMember,
                Roles = roles.ToList()
            };

            return View(viewModel);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VehicleCount,ActiveParkingCount,TotalCurrentCost,IsProMember,Roles")] MemberViewModel memberViewModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(memberViewModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(memberViewModel);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var memberViewModel = await _context.MemberViewModel.FindAsync(id);
            if (memberViewModel == null)
            {
                return NotFound();
            }
            return View(memberViewModel);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VehicleCount,ActiveParkingCount,TotalCurrentCost,IsProMember,Roles")] MemberViewModel memberViewModel)
        {
            if (id != memberViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(memberViewModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberViewModelExists(memberViewModel.Id))
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
            return View(memberViewModel);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var memberViewModel = await _context.MemberViewModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (memberViewModel == null)
            {
                return NotFound();
            }

            return View(memberViewModel);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var memberViewModel = await _context.MemberViewModel.FindAsync(id);
            if (memberViewModel != null)
            {
                _context.MemberViewModel.Remove(memberViewModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberViewModelExists(int id)
        {
            return _context.MemberViewModel.Any(e => e.Id == id);
        }
    }
}
