using l_hospital_mang.Data;
using l_hospital_mang.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace l_hospital_mang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("add-room")]
        public async Task<IActionResult> AddRoom([FromForm] Rooms room)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kv => kv.Value.Errors.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(new
                {
                    statusCode = 400,
                    errors
                });
            }

            try
            {
                // حفظ الغرفة
                _context.Room.Add(room);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(AddRoom), new { id = room.Id }, new
                {
                    statusCode = 201,
                    message = "Room added successfully.",
                    data = new
                    {
                        room.Id,
                        room.RoomNumber,
                        room.FloorNumber,
                        room.bedsNumber,
                        room.Price,
                        room.IsOccupied 
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while adding the room.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _context.Room
                .Where(r => r.Id == id)
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    FloorNumber = r.FloorNumber,
                    bedsNumber = r.bedsNumber,
                    Price = r.Price
                })
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Room not found."
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Room retrieved successfully.",
                Data = room
            });
        }

        [HttpGet("all-rooms")]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _context.Room
                .Select(r => new
                {
                    r.Id,
                    r.RoomNumber,
                    r.FloorNumber,
                    r.bedsNumber,
                    r.Price
                })
                .ToListAsync();

            if (rooms == null || rooms.Count == 0)
            {
                return NotFound(new
                {
                    statusCode = 404,
                    message = "No rooms available to display."
                });
            }

            return Ok(new
            {
                statusCode = 200,
                message = "Rooms fetched successfully.",
                data = rooms
            });
        }

    }
}
