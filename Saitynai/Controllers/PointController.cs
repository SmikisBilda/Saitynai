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
    public class PointController : ControllerBase
    {
        private readonly PostgresContext _context;

        public PointController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all points. Requires a global 'view' permission on 'Point'.
        /// </summary>
        /// <returns>List of points.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Point>))]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Point>>> GetPoint()
        {
            return await _context.Point.ToListAsync();
        }

        /// <summary>
        /// Get a point by id.
        /// </summary>
        /// <param name="id">Point identifier.</param>
        /// <returns>The requested point.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Point))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<ActionResult<Point>> GetPoint(int id)
        {
            var point = await _context.Point.FindAsync(id);

            if (point == null)
            {
                return NotFound();
            }

            return point;
        }

        /// <summary>
        /// Get all points for a floor.
        /// </summary>
        /// <param name="floorId">Floor identifier.</param>
        /// <returns>List of points for the specified floor.</returns>
        [HttpGet("floor/{floorId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Point>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Point>>> GetFloorsByFloor(int floorId)
        {
            var floorExists = await _context.Floor.AnyAsync(b => b.Id == floorId);
            if (!floorExists)
            {
                return NotFound($"Floor with ID {floorId} not found.");
            }

            var points = await _context.Point
                .Where(f => f.FloorId == floorId)
                .ToListAsync();

            return points;
        }

        /// <summary>
        /// Create a point.
        /// </summary>
        /// <param name="point">Point payload.</param>
        /// <returns>The created point.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Point))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        [PermissionAuthorize("create", "Point")]
        public async Task<ActionResult<Point>> PostPoint(Point point)
        {
            try
            {
                _context.Point.Add(point);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPoint", new { id = point.Id }, point);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
            {
                return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = "A point with the same identifier already exists.", Extensions = { { "constraint", pg.ConstraintName } } });
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23503")
            {
                return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = "The referenced floor_id does not exist.", Extensions = { { "constraint", pg.ConstraintName } } });
            }
        }
        
        /// <summary>
        /// Update a point.
        /// </summary>
        /// <param name="id">Point identifier.</param>
        /// <param name="point">Point payload.</param>
        /// <returns>No content on success.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [PermissionAuthorize("edit", "Point")]
        public async Task<IActionResult> PutPoint(int id, Point point)
        {
            if (id != point.Id)
            {
                return BadRequest();
            }

            _context.Entry(point).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PointExists(id))
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
        /// Delete a point by id.
        /// </summary>
        /// <param name="id">Point identifier.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [PermissionAuthorize("delete", "Point")]
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
