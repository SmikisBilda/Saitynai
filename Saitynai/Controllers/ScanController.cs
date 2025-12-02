using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using Npgsql;
using Saitynai.Authorization; // Added for PermissionAuthorize
using Microsoft.AspNetCore.Authorization; // Added for AllowAnonymous

namespace Saitynai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class ScanController : ControllerBase
    {
        private readonly PostgresContext _context;

        public ScanController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all scans. This endpoint is public.
        /// </summary>
        /// <returns>List of scans.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Scan>))]
        public async Task<ActionResult<IEnumerable<Scan>>> GetScan()
        {
            return await _context.Scan.ToListAsync();
        }

        /// <summary>
        /// Get a scan by id. This endpoint is public.
        /// </summary>
        /// <param name="id">Scan identifier.</param>
        /// <returns>The requested scan.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
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
        /// Get all scans for a point. This endpoint is public.
        /// </summary>
        /// <param name="pointId">Point identifier.</param>
        /// <returns>List of scans for the specified point.</returns>
        [HttpGet("point/{pointId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Scan>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Scan>>> GetScansByPoint(int pointId) // Method name corrected for clarity
        {
            var pointExists = await _context.Point.AnyAsync(b => b.Id == pointId);
            if (!pointExists)
            {
                return NotFound($"Point with ID {pointId} not found.");
            }

            var scans = await _context.Scan
                .Where(f => f.PointId == pointId)
                .ToListAsync();

            return scans;
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
        [PermissionAuthorize("create", "Scan")]
        public async Task<ActionResult<Scan>> PostScan(Scan scan)
        {
            try
            {
                _context.Scan.Add(scan);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetScan", new { id = scan.Id }, scan);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
            {
                return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = "A scan with the same identifier already exists.", Extensions = { { "constraint", pg.ConstraintName } } });
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23503")
            {
                return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = "The referenced point_id does not exist.", Extensions = { { "constraint", pg.ConstraintName } } });
            }
        }
        
        /// <summary>
        /// Update a scan.
        /// </summary>
        /// <param name="id">Scan identifier.</param>
        /// <param name="scan">Scan payload.</param>
        /// <returns>No content on success.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [PermissionAuthorize("edit", "Scan")]
        public async Task<IActionResult> PutScan(int id, Scan scan)
        {
            if (id != scan.Id)
            {
                return BadRequest();
            }

            _context.Entry(scan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScanExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a scan by id.
        /// </summary>
        /// <param name="id">Scan identifier.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [PermissionAuthorize("delete", "Scan")]
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
