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

        [HttpPut("update-room/{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromForm] RoomUpdateDtocs updateDto)
        {
            var existingRoom = await _context.Room.FindAsync(id);
            if (existingRoom == null)
            {
                return NotFound(new
                {
                    statusCode = 404,
                    message = "Room not found."
                });
            }



            if (updateDto.FloorNumber.HasValue)
                existingRoom.FloorNumber = updateDto.FloorNumber.Value;

            if (updateDto.bedsNumber.HasValue)
                existingRoom.bedsNumber = updateDto.bedsNumber.Value;

            if (updateDto.Price.HasValue)
                existingRoom.Price = updateDto.Price.Value;
            if (!string.IsNullOrWhiteSpace(updateDto.IsOccupied))
                existingRoom.IsOccupied = updateDto.IsOccupied;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "Room updated successfully.",
                data = new
                {
                    existingRoom.Id,
                    existingRoom.RoomNumber,
                    existingRoom.FloorNumber,
                    existingRoom.bedsNumber,
                    existingRoom.Price,
                    existingRoom.IsOccupied
                }
            });
        }



        [HttpDelete("delete-room/{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Room.FindAsync(id);
            if (room == null)
            {
                return NotFound(new
                {
                    statusCode = 404,
                    message = "Room not found."
                });
            }

            _context.Room.Remove(room);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "Room deleted successfully."
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(long id)
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
