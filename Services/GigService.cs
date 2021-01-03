namespace DockerGigApi.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DockerGigApi.Data;
    using DockerGigApi.Entities;

    public class GigService : IGigService
    {
        private readonly IAsyncRepository<Gig> gigRepository;

        public GigService(IAsyncRepository<Gig> gigRepository)
        {
            this.gigRepository = gigRepository;
        }

        public async Task<IEnumerable<Gig>> GetGigs(int page, int pageSize)
        {
            var gigs = await this.gigRepository.ListAllAsync();

            var orderedGigs = gigs
                .OrderByDescending(b => b.GigDate)
                .Skip(pageSize * (page - 1)).Take(pageSize)
                .ToList();

            return orderedGigs;
        }

        public async Task<IEnumerable<Gig>> GetGigs()
        {
            var gigs = await this.gigRepository.ListAllAsync();

            var orderedGigs = gigs
                .OrderBy(b => b.Name)
                .ToList();

            return orderedGigs;
        }

        public async Task<Gig> GetGig(int id)
        {
            var gig = await this.gigRepository.GetByIdAsync(id);
            return gig;
        }

        public async Task CreateGig(Gig gig)
        {
            await this.gigRepository.AddAsync(gig);
        }

        public async Task UpdateGig(Gig gig)
        {
            await this.gigRepository.UpdateAsync(gig);
        }

        public async Task DeleteGig(int gigId)
        {
            var gig = await this.gigRepository.GetByIdAsync(gigId);
            await this.gigRepository.DeleteAsync(gig);
        }
    }
}
