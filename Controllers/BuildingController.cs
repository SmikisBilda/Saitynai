using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using Npgsql;

namespace Saitynai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingController : ControllerBase
    {
        private readonly SaitynaiContext _context;

        public BuildingController(SaitynaiContext context)
        {
            _context = context;
        }

        // GET: api/Building
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Building>>> GetBuilding()
        {
            return await _context.Building.ToListAsync();
        }

        // GET: api/Building/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Building>> GetBuilding(int id)
        {
            var building = await _context.Building.FindAsync(id);

            if (building == null)
            {
                return NotFound();
            }

            return building;
        }
    [HttpPost]
    public async Task<ActionResult<Building>> PostBuilding(Building building)
    {
        try
        {
            _context.Building.Add(building);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBuilding", new { id = building.Id }, building);
        }
        // 23505: unique/primary key violation (duplicate id or other unique constraint)
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            var pd = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Type  = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                Title = "Conflict",
                Detail = "A building with the same identifier already exists."
            };
            pd.Extensions["constraint"] = pg.ConstraintName;
            return Conflict(pd);
        }

    }


        // DELETE: api/Building/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            var building = await _context.Building.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }

            _context.Building.Remove(building);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BuildingExists(int id)
        {
            return _context.Building.Any(e => e.Id == id);
        }
    }
}
