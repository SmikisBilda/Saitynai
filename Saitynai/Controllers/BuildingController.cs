using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using Npgsql;
using Microsoft.AspNetCore.Authorization;
using Saitynai.Authorization; 

namespace Saitynai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class BuildingController : ControllerBase
    {
        private readonly PostgresContext _context;

        public BuildingController(PostgresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all buildings.
        /// </summary>
        /// <returns>List of buildings.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Building>))]
        public async Task<ActionResult<IEnumerable<Building>>> GetBuilding()
        {
            return await _context.Building.ToListAsync();
        }

        /// <summary>
        /// Get a building by id.
        /// </summary>
        /// <param name="id">Building identifier.</param>
        /// <returns>The requested building.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Building))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Building>> GetBuilding(int id)
        {
            var building = await _context.Building.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }
            return building;
        }

        /// <summary>
        /// Get a single point under a specific floor and building.
        /// </summary>
        /// <param name="buildingId">Building identifier.</param>
        /// <param name="floorId">Floor identifier.</param>
        /// <param name="pointId">Point identifier.</param>
        /// <returns>A point belonging to the given floor within the building.</returns>
        [HttpGet("{buildingId:int}/floors/{floorId:int}/points/{pointId:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Point))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Point>> GetBuildingFloorPoint(int buildingId, int floorId, int pointId)
        {

            var buildingExists = await _context.Building.AnyAsync(b => b.Id == buildingId);
            if (!buildingExists)
            {
                return Problem(
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title: "Not Found",
                    detail: $"Building with ID {buildingId} not found.",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            var floorExists = await _context.Floor.AnyAsync(f => f.Id == floorId && f.BuildingId == buildingId);
            if (!floorExists)
            {
                return Problem(
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title: "Not Found",
                    detail: $"Floor with ID {floorId} not found in building {buildingId}.",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            var point = await _context.Point
                .Where(p => p.Id == pointId && p.FloorId == floorId)
                .FirstOrDefaultAsync();

            if (point == null)
            {
                return Problem(
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title: "Not Found",
                    detail: $"Point with ID {pointId} not found on floor {floorId} in building {buildingId}.",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return Ok(point);
        }

    

    /// <summary>
    /// Get a single AccessPoint under specific scan, point, floor, and building.
    /// </summary>
    /// <param name="buildingId">Building identifier.</param>
    /// <param name="floorId">Floor identifier.</param>
    /// <param name="pointId">Point identifier.</param>
    /// <param name="scanId">Scan identifier.</param>
    /// <param name="accessPointId">AccessPoint identifier.</param>
    /// <returns>An AccessPoint belonging to the specified hierarchy.</returns>
    [HttpGet("{buildingId:int}/floors/{floorId:int}/points/{pointId:int}/scans/{scanId:int}/accesspoints/{accessPointId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessPoint))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult<AccessPoint>> GetBuildingFloorPointScanAccessPoint(
        int buildingId,
        int floorId,
        int pointId,
        int scanId,
        int accessPointId)
    {
        // Verify building exists
        var buildingExists = await _context.Building.AnyAsync(b => b.Id == buildingId);
        if (!buildingExists)
        {
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "Not Found",
                detail: $"Building with ID {buildingId} not found.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        // Verify floor exists in building
        var floorExists = await _context.Floor.AnyAsync(f => f.Id == floorId && f.BuildingId == buildingId);
        if (!floorExists)
        {
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "Not Found",
                detail: $"Floor with ID {floorId} not found in building {buildingId}.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        // Verify point exists on floor
        var pointExists = await _context.Point.AnyAsync(p => p.Id == pointId && p.FloorId == floorId);
        if (!pointExists)
        {
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "Not Found",
                detail: $"Point with ID {pointId} not found on floor {floorId} in building {buildingId}.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        // Verify scan exists on point
        var scanExists = await _context.Scan.AnyAsync(s => s.Id == scanId && s.PointId == pointId);
        if (!scanExists)
        {
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "Not Found",
                detail: $"Scan with ID {scanId} not found for point {pointId} on floor {floorId} in building {buildingId}.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        // Retrieve the access point
        var accessPoint = await _context.AccessPoint
            .Where(ap => ap.Id == accessPointId && ap.ScanId == scanId)
            .FirstOrDefaultAsync();

        if (accessPoint == null)
        {
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "Not Found",
                detail: $"AccessPoint with ID {accessPointId} not found for scan {scanId} on point {pointId}, floor {floorId} in building {buildingId}.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(accessPoint);
    }



        /// <summary>
        /// Get a single scan under a specific point, floor, and building.
        /// </summary>
        /// <param name="buildingId">Building identifier.</param>
        /// <param name="floorId">Floor identifier.</param>
        /// <param name="pointId">Point identifier.</param>
        /// <param name="scanId">Scan identifier.</param>
        /// <returns>A scan belonging to the specified point, floor, and building.</returns>
        [HttpGet("{buildingId:int}/floors/{floorId:int}/points/{pointId:int}/scans/{scanId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Scan))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<ActionResult<Scan>> GetBuildingFloorPointScan(int buildingId, int floorId, int pointId, int scanId)
        {

            var buildingExists = await _context.Building.AnyAsync(b => b.Id == buildingId);
            if (!buildingExists)
            {
                return Problem(
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title: "Not Found",
                    detail: $"Building with ID {buildingId} not found.",
                    statusCode: StatusCodes.Status404NotFound
                );
            }


            var floorExists = await _context.Floor.AnyAsync(f => f.Id == floorId && f.BuildingId == buildingId);
            if (!floorExists)
            {
                return Problem(
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title: "Not Found",
                    detail: $"Floor with ID {floorId} not found in building {buildingId}.",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

   
            var pointExists = await _context.Point.AnyAsync(p => p.Id == pointId && p.FloorId == floorId);
            if (!pointExists)
            {
                return Problem(
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title: "Not Found",
                    detail: $"Point with ID {pointId} not found on floor {floorId} in building {buildingId}.",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            
            var scan = await _context.Scan.FirstOrDefaultAsync(s => s.Id == scanId && s.PointId == pointId);
            if (scan == null)
            {
                return Problem(
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title: "Not Found",
                    detail: $"Scan with ID {scanId} not found for point {pointId} on floor {floorId} in building {buildingId}.",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return Ok(scan);
        }




        /// <summary>
        /// Create a building.
        /// </summary>
        /// <param name="building">Building payload.</param>
        /// <returns>The created building.</returns>
        [HttpPost]
        [PermissionAuthorize("create", "Building")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Building))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
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

        /// <summary>
        /// Update a building by id.
        /// </summary>
        /// <param name="id">Building identifier from URL.</param>
        /// <param name="building">Updated building payload.</param>
        /// <returns>No content on success.</returns>
        [HttpPut("{id}")]
        [PermissionAuthorize("edit", "Building")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutBuilding(int id, Building building)
        {
            var existingBuilding = await _context.Building.FindAsync(id);
            if (existingBuilding == null)
            {
                return NotFound();
            }

            // Update only the properties provided in the request
            if (building.Name != null)
            {
                existingBuilding.Name = building.Name;
            }
            if (building.Address != null)
            {
                existingBuilding.Address = building.Address;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuildingExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a building by id.
        /// </summary>
        /// <param name="id">Building identifier.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [PermissionAuthorize("delete", "Building")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
