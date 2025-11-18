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
    [Produces("application/json")]
    [Authorize]
    public class FloorController : ControllerBase
    {
        private readonly PostgresContext _context;

        public FloorController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all floors.
        /// </summary>
        /// <returns>List of floors.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Floor>))]
        public async Task<ActionResult<IEnumerable<Floor>>> GetFloor()
        {
            return await _context.Floor.ToListAsync();
        }

        /// <summary>
        /// Get a floor by id.
        /// </summary>
        /// <param name="id">Floor identifier.</param>
        /// <returns>The requested floor.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Floor))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Floor>> GetFloor(int id)
        {
            var floor = await _context.Floor.FindAsync(id);

            if (floor == null)
            {
                return NotFound();
            }

            return floor;
        }

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="floor">Floor payload.</param>
        /// <returns>The created floor.</returns>
        [HttpPost]
        [PermissionAuthorize("create", "Floor")] 
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Floor))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<Floor>> PostFloor(Floor floor)
        {
            try
            {
                _context.Floor.Add(floor);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetFloor", new { id = floor.Id }, floor);
            }
            // Unique violation => 409 Conflict
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
            // FK violation => 409 Conflict
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

        /// <summary>
        /// Update a floor by id.
        /// </summary>
        /// <param name="id">Floor identifier from URL.</param>
        /// <param name="floor">Updated floor payload.</param>
        /// <returns>No content on success.</returns>
        [HttpPut("{id}")]
        [PermissionAuthorize("edit", "Floor")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutFloor(int id, Floor floor)
        {
            if (id != floor.Id)
            {
                return BadRequest();
            }

            _context.Entry(floor).State = EntityState.Modified;

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

        /// <summary>
        /// Get all floors for a building.
        /// </summary>
        /// <param name="buildingId">Building identifier.</param>
        /// <returns>List of floors for the specified building.</returns>
        [HttpGet("building/{buildingId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Floor>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Delete a floor by id.
        /// </summary>
        /// <param name="id">Floor identifier.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [PermissionAuthorize("delete", "Floor")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
