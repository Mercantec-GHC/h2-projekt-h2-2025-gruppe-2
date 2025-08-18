using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Checks if the API is running and responsive.
        /// </summary>
        /// <returns>Status and a message indicating the API's health.</returns>
        /// <response code="200">The API is running.</response>
        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "OK", message = "The API is running!" });
        }

        /// <summary>
        /// Checks if the database is available.
        /// This is a placeholder until EF Core is configured.
        /// </summary>
        /// <returns>Status and a message about the database connection.</returns>
        /// <response code="200">Database is running or an error message is returned.</response>
        [HttpGet("dbhealthcheck")]
        public IActionResult DBHealthCheck()
        {
            // Until EF Core is set up, just return a message

            try {
                // using (var context = new ApplicationDbContext())
                // {
                //     context.Database.CanConnect();
                // }
                throw new Exception("EF Core is not configured yet! This will be implemented later.");
            }
            catch (Exception ex)
            {
                return Ok(new { status = "Error", message = "Database connection error: " + ex.Message });
            }
            return Ok(new { status = "OK", message = "Database is running!" });
        }

        /// <summary>
        /// Simple ping endpoint to test API responsiveness.
        /// </summary>
        /// <returns>Status and a "Pong" message.</returns>
        /// <response code="200">The API responded with Pong.</response>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { status = "OK", message = "Pong 🏓" });
        }
    }
}
