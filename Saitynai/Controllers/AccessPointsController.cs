using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using Npgsql;
using Saitynai.Authorization; 
using Microsoft.AspNetCore.Authorization; 

namespace Saitynai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class AccessPointsController : ControllerBase
    {
        private readonly PostgresContext _context;

        public AccessPointsController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all access points. This endpoint is public.
        /// </summary>
        /// <returns>List of access points.</returns>
        [HttpGet]
        [AllowAnonymous] // Authorization Added
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AccessPoint>))]
        public async Task<ActionResult<IEnumerable<AccessPoint>>> GetAccessPoint()
        {
            return await _context.AccessPoint.ToListAsync();
        }

        /// <summary>
        /// Get an access point by id. This endpoint is public.
        /// </summary>
        /// <param name="id">Access point identifier.</param>
        /// <returns>The requested access point.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
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
        /// Get all access points for a scan. This endpoint is public.
        /// </summary>
        /// <param name="scanId">Scan identifier.</param>
        /// <returns>List of access points for the specified scan.</returns>
        [HttpGet("scan/{scanId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AccessPoint>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AccessPoint>>> GetAccessPointsByScan(int scanId) // Method name corrected
        {
            var scanExists = await _context.Scan.AnyAsync(b => b.Id == scanId);
            if (!scanExists)
            {
                return NotFound($"Scan with ID {scanId} not found.");
            }

            var accessPoints = await _context.AccessPoint
                .Where(f => f.ScanId == scanId)
                .ToListAsync();

            return accessPoints;
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
        [PermissionAuthorize("create", "AccessPoint")]
        public async Task<IActionResult> PostAccessPoint([FromBody] AccessPointCreateDto dto)
        {
            var pd = new ProblemDetails { Status = 422, Title = "Unprocessable Entity", Detail = "Invalid BSSID format." };

            if (string.IsNullOrWhiteSpace(dto.Bssid)) return UnprocessableEntity(pd);
            
            var normalized = dto.Bssid.Replace(":", "").Replace("-", "").Trim();
            if (normalized.Length != 12 || !normalized.All(Uri.IsHexDigit)) return UnprocessableEntity(pd);

            try
            {
                System.Net.NetworkInformation.PhysicalAddress.Parse(normalized);
            }
            catch { return UnprocessableEntity(pd); }

            var entity = new AccessPoint
            {
                ScanId = dto.ScanId, Ssid = dto.Ssid, Bssid = dto.Bssid, Capabilities = dto.Capabilities,
                Centerfreq0 = dto.Centerfreq0, Centerfreq1 = dto.Centerfreq1, Frequency = dto.Frequency, Level = dto.Level
            };

            try
            {
                _context.AccessPoint.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
            {
                return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = "An access point with the same identifier already exists.", Extensions = { { "constraint", pg.ConstraintName } } });
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23503")
            {
                return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = "The referenced scan_id does not exist.", Extensions = { { "constraint", pg.ConstraintName } } });
            }

            return CreatedAtAction(nameof(GetAccessPoint), new { id = entity.Id }, entity);
        }

        /// <summary>
        /// Update an access point.
        /// </summary>
        /// <param name="id">Access point identifier.</param>
        /// <param name="accessPoint">Access point payload.</param>
        /// <returns>No content on success.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [PermissionAuthorize("edit", "AccessPoint")]
        public async Task<IActionResult> PutAccessPoint(int id, AccessPoint accessPoint)
        {
            if (id != accessPoint.Id)
            {
                return BadRequest();
            }

            _context.Entry(accessPoint).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccessPointExists(id))
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
        /// Delete an access point by id.
        /// </summary>
        /// <param name="id">Access point identifier.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [PermissionAuthorize("delete", "AccessPoint")] // Authorization Added
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
