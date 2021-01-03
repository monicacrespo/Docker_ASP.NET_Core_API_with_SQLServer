namespace DockerGigApi.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using DockerGigApi.Entities;
    using DockerGigApi.Services;

    [Route("api/[controller]")]
    [ApiController]
    public class GigsController : ControllerBase
    {
        private readonly IGigService gigService;
        private readonly ILogger<GigsController> _logger;

        public GigsController(IGigService gigService, ILogger<GigsController> logger)
        {
            this.gigService = gigService;
            this._logger = logger;
        }

        // GET: api/Gigs?page=1&pageSize=5
        [HttpGet]
        public async Task<IActionResult> Get(int page = 0, int pageSize = 5)
        {
            var result = await this.gigService.GetGigs(page, pageSize);
            return Ok(result);
        }

        // GET: api/Gigs/5
        [HttpGet("{gigId}", Name = "GetGig")]
        public async Task<IActionResult> GetGig(int gigId)
        {
            var result = await this.gigService.GetGig(gigId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // PUT: api/Gigs/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Gig gig)
        {
            if (id != gig.GigId)
            {
                return this.BadRequest();
            }

            try
            {
                await this.gigService.UpdateGig(gig);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GigExists(id))
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

        // POST: api/Gigs
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Gig gig)
        {
            if (ModelState.IsValid)
            {
                await this.gigService.CreateGig(gig);
                return CreatedAtAction(nameof(GetGig), new { gig.GigId }, gig);
            }
            return BadRequest(ModelState);
        }

        // DELETE: api/Gigs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await this.gigService.GetGig(id);
            if (result == null)
            {
                return NotFound();
            }

            await this.gigService.DeleteGig(id);

            return Ok();
        }

        private bool GigExists(int gigId)
        {
            var result = this.gigService.GetGig(gigId);
            return (result != null);
        }
    }
}