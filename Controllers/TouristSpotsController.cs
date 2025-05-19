using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using SarawakTourism.Data;
using SarawakTourismApi.Models;

namespace SarawakTourismApi.Controllers
{
    public class TouristSpotsController : BaseController
    {
        private readonly AppDbContext _context;

        public TouristSpotsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var spots = await _context.TouristSpots.OrderByDescending(s => s.Rating).ToListAsync();
            return Success(spots);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var spot = await _context.TouristSpots.FindAsync(id);
            if (spot == null)
                return Error("Tourist spot not found", HttpStatusCode.NotFound);

            return Success(spot);
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var spots = await _context.TouristSpots
                .Where(s => s.Category.ToLower() == category.ToLower())
                .OrderByDescending(s => s.Rating)
                .ToListAsync();
            return Success(spots);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var spots = await _context.TouristSpots
                .Where(s => s.Name.Contains(query) || s.Description.Contains(query) || s.Location.Contains(query))
                .OrderByDescending(s => s.Rating)
                .ToListAsync();
            return Success(spots);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TouristSpot spot)
        {
            if (!ModelState.IsValid)
                return Error("Invalid tourist spot data");

            _context.TouristSpots.Add(spot);
            await _context.SaveChangesAsync();
            return Success(spot, "Tourist spot created successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TouristSpot spot)
        {
            if (id != spot.Id)
                return Error("Invalid tourist spot ID");

            if (!ModelState.IsValid)
                return Error("Invalid tourist spot data");

            spot.UpdatedAt = DateTime.UtcNow;
            _context.Entry(spot).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Success(spot, "Tourist spot updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var spot = await _context.TouristSpots.FindAsync(id);
            if (spot == null)
                return Error("Tourist spot not found", HttpStatusCode.NotFound);

            _context.TouristSpots.Remove(spot);
            await _context.SaveChangesAsync();
            return Success(new { }, "Tourist spot deleted successfully");
        }
    }
} 