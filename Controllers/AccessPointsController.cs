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
    public class AccessPointsController : ControllerBase
    {
        private readonly SaitynaiContext _context;

        public AccessPointsController(SaitynaiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all access points.
        /// </summary>
        /// <returns>List of access points.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AccessPoint>))]
        public async Task<ActionResult<IEnumerable<AccessPoint>>> GetAccessPoint()
        {
            return await _context.AccessPoint.ToListAsync();
        }

        /// <summary>
        /// Get an access point by id.
        /// </summary>
        /// <param name="id">Access point identifier.</param>
        /// <returns>The requested access point.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessPoint))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccessPoint>> GetAccessPoint(int id)
        {
            var accessPoint = await _context.AccessPoint.FindAsync(id);

            if (accessPoint == null)
            {
                return NotFound();
            }

            return accessPoint;
        }

        /// <summary>
        /// Create a new access point.
        /// </summary>
        /// <param name="dto">Access point payload.</param>
        /// <returns>The created access point.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AccessPoint))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> PostAccessPoint([FromBody] AccessPointCreateDto dto)
        {
            var pd = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Type = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2",
                Title = "Unprocessable Entity",
                Detail = "Invalid bssid."
            };

            // basic presence check
            if (string.IsNullOrWhiteSpace(dto.Bssid))
                return UnprocessableEntity(pd);

            // normalize and validate hex MAC length
            var normalized = dto.Bssid.Replace(":", "").Replace("-", "").Trim();
            if (normalized.Length != 12 || !normalized.All(Uri.IsHexDigit))
                return UnprocessableEntity(pd);

            // strict parse
            System.Net.NetworkInformation.PhysicalAddress bssid;
            try
            {
                bssid = System.Net.NetworkInformation.PhysicalAddress.Parse(normalized);
            }
            catch
            {
                return UnprocessableEntity(pd);
            }

            var entity = new AccessPoint
            {
                ScanId = dto.ScanId,
                Ssid = dto.Ssid,
                Bssid = dto.Bssid,
                Capabilities = dto.Capabilities,
                Centerfreq0 = dto.Centerfreq0,
                Centerfreq1 = dto.Centerfreq1,
                Frequency = dto.Frequency,
                Level = dto.Level
            };

            try
            {
                _context.AccessPoint.Add(entity);
                await _context.SaveChangesAsync();
            }
            // 23505 duplicate key => 409 Conflict
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var dup = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "An access scan with the same identifier already exists."
                };
                dup.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(dup);
            }
            // 23503 FK violation (e.g., ScanId references missing Scan) => 409 Conflict
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                var fk = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict",
                    Title = "Conflict",
                    Detail = "The referenced scan_id does not exist."
                };
                fk.Extensions["constraint"] = pg.ConstraintName;
                return Conflict(fk);
            }

            return CreatedAtAction(nameof(GetAccessPoint), new { id = entity.Id }, entity);
        }

        /// <summary>
        /// Get all access points for a scan.
        /// </summary>
        /// <param name="scanId">Scan identifier.</param>
        /// <returns>List of access points for the specified scan.</returns>
        [HttpGet("scan/{scanId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AccessPoint>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AccessPoint>>> GetPointsByScan(int scanId)
        {
            var scanExists = await _context.Scan.AnyAsync(b => b.Id == scanId);
            if (!scanExists)
            {
                return NotFound();
            }

            var accessPoints = await _context.AccessPoint
                .Where(f => f.ScanId == scanId)
                .ToListAsync();

            return accessPoints;
        }

        /// <summary>
        /// Delete an access point by id.
        /// </summary>
        /// <param name="id">Access point identifier.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccessPoint(int id)
        {
            var accessPoint = await _context.AccessPoint.FindAsync(id);
            if (accessPoint == null)
            {
                return NotFound();
            }

            _context.AccessPoint.Remove(accessPoint);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AccessPointExists(int id)
        {
            return _context.AccessPoint.Any(e => e.Id == id);
        }
    }
}
