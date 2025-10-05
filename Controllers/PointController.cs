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
    public class PointController : ControllerBase
    {
        private readonly SaitynaiContext _context;

        public PointController(SaitynaiContext context)
        {
            _context = context;
        }

        // GET: api/Point
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Point>>> GetPoint()
        {
            return await _context.Point.ToListAsync();
        }

        // GET: api/Point/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Point>> GetPoint(int id)
        {
            var point = await _context.Point.FindAsync(id);

            if (point == null)
            {
                return NotFound();
            }

            return point;
        }


        // POST: api/Point
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Point>> PostPoint(Point point)
        {
            try
            {
                _context.Point.Add(point);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPoint", new { id = point.Id }, point);
            }
            // 23505: unique/primary key violation (duplicate)
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "A point with the same identifier already exists."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
            // 23503: foreign key violation (e.g., floor_id points to a missing Floor)
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "The referenced floor_id does not exist."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
        }


        // DELETE: api/Point/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePoint(int id)
        {
            var point = await _context.Point.FindAsync(id);
            if (point == null)
            {
                return NotFound();
            }

            _context.Point.Remove(point);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PointExists(int id)
        {
            return _context.Point.Any(e => e.Id == id);
        }
    }
}
