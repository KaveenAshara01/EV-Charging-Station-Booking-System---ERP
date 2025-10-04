using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EvChargingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MongoTestController : ControllerBase
    {
        private readonly IMongoDatabase _database;

        public MongoTestController()
        {
            var client = new MongoClient("mongodb+srv://nipunabhashitha76_db_user:UaOU9UeRIsfC1y0h@cluster0.3k61g7v.mongodb.net/"); // Update if needed
            _database = client.GetDatabase("EvBookingSystem"); // Update with your DB name
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            try
            {
                var collections = _database.ListCollectionNames().ToList();
                return Ok(new { status = "Connected", collections });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Connection Failed", error = ex.Message });
            }
        }
    }
}
