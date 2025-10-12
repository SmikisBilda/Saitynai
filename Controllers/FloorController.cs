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
    public class FloorController : ControllerBase
    {
        private readonly SaitynaiContext _context;

        public FloorController(SaitynaiContext context)
        {
            _context = context;
        }

        // GET: api/Floor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Floor>>> GetFloor()
        {
            return await _context.Floor.ToListAsync();
        }

        // GET: api/Floor/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Floor>> GetFloor(int id)
        {
            var floor = await _context.Floor.FindAsync(id);

            if (floor == null)
            {
                return NotFound();
            }

            return floor;
        }

        // POST: api/Floor
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Floor>> PostFloor(Floor floor)
        {
            try
            {
                _context.Floor.Add(floor);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetFloor", new { id = floor.Id }, floor);
            }

            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "A floor with the same identifier already exists."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
         
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "The referenced building_id does not exist."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFloor(int id, Floor building)
        {

            if (id != building.Id)
            {
                return BadRequest();
            }

            _context.Entry(building).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException) when (!FloorExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("building/{buildingId:int}")]
        public async Task<ActionResult<IEnumerable<Floor>>> GetFloorsByBuilding(int buildingId)
        {
        
            var buildingExists = await _context.Building.AnyAsync(b => b.Id == buildingId);
            if (!buildingExists)
            {
                return NotFound();
            }

            var floors = await _context.Floor
                .Where(f => f.BuildingId == buildingId)
                .ToListAsync();

            return floors;
        }

        // DELETE: api/Floor/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFloor(int id)
        {
            var floor = await _context.Floor.FindAsync(id);
            if (floor == null)
            {
                return NotFound();
            }

            _context.Floor.Remove(floor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FloorExists(int id)
        {
            return _context.Floor.Any(e => e.Id == id);
        }
    }
}
