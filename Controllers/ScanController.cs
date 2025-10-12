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
    [Produces("application/json")]
    public class ScanController : ControllerBase
    {
        private readonly SaitynaiContext _context;

        public ScanController(SaitynaiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all scans.
        /// </summary>
        /// <returns>List of scans.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Scan>))]
        public async Task<ActionResult<IEnumerable<Scan>>> GetScan()
        {
            return await _context.Scan.ToListAsync();
        }

        /// <summary>
        /// Get a scan by id.
        /// </summary>
        /// <param name="id">Scan identifier.</param>
        /// <returns>The requested scan.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Scan))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Scan>> GetScan(int id)
        {
            var scan = await _context.Scan.FindAsync(id);

            if (scan == null)
            {
                return NotFound();
            }

            return scan;
        }

        /// <summary>
        /// Create a scan.
        /// </summary>
        /// <param name="scan">Scan payload.</param>
        /// <returns>The created scan.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Scan))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<Scan>> PostScan(Scan scan)
        {
            try
            {
                _context.Scan.Add(scan);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetScan", new { id = scan.Id }, scan);
            }
            // 23505: unique/primary key violation (duplicate)
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "A scan with the same identifier already exists."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
            // 23503: foreign key violation (PointId references non-existent Point)
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "The referenced point_id does not exist."
                };
                pd.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(pd);
            }
        }

        /// <summary>
        /// Get all scans for a point.
        /// </summary>
        /// <param name="pointId">Point identifier.</param>
        /// <returns>List of scans for the specified point.</returns>
        [HttpGet("point/{pointId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Scan>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Scan>>> GetPointsByPoint(int pointId)
        {
            var pointExists = await _context.Point.AnyAsync(b => b.Id == pointId);
            if (!pointExists)
            {
                return NotFound();
            }

            var scans = await _context.Scan
                .Where(f => f.PointId == pointId)
                .ToListAsync();

            return scans;
        }

        /// <summary>
        /// Delete a scan by id.
        /// </summary>
        /// <param name="id">Scan identifier.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteScan(int id)
        {
            var scan = await _context.Scan.FindAsync(id);
            if (scan == null)
            {
                return NotFound();
            }

            _context.Scan.Remove(scan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ScanExists(int id)
        {
            return _context.Scan.Any(e => e.Id == id);
        }
    }
}
