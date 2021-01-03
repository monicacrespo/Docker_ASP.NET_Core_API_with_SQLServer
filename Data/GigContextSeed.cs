namespace DockerGigApi.Data
{
    using DockerGigApi.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.EntityFrameworkCore;

    public class GigContextSeed
    {
        public async static Task SeedAsync(GigContext gigContext, ILoggerFactory loggerFactory, int? retry = 0)
        {
            var log = loggerFactory.CreateLogger<GigContextSeed>();
            
            int retryForAvailability = retry.Value;
            try
            {
                log.LogInformation("Applying migrations...");
                
                // TODO: Only run this if using a real database              
                gigContext.Database.Migrate();

                if (!gigContext.Gigs.Any())
                {
                    log.LogInformation("Adding data - seeding...");
                    gigContext.Gigs.AddRange(
                        GetPreconfiguredGigs());

                    await gigContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvailability < 10)
                {
                    retryForAvailability++;    
                    log.LogError(ex.Message);
                    await SeedAsync(gigContext, loggerFactory, retryForAvailability);
                }
                throw;
            }
        }
     
        static IEnumerable<Gig> GetPreconfiguredGigs()
        {
            return new List<Gig>()
            {
                new Gig
                {
                    Name = "David Bowie",
                    GigDate = new DateTime(2020, 12, 29),
                    MusicGenre = "rock"
                },

                new Gig
                {
                    Name = "Muse",
                    GigDate = new DateTime(2020, 12, 2),
                    MusicGenre = "rock"
                },

                new Gig
                {
                    Name = "Ed Sheeran Pop",
                    GigDate = new DateTime(2020, 12, 2),
                    MusicGenre = "pop"
                },

                new Gig
                {
                    Name = "Rozalem",
                    GigDate = new DateTime(2020, 12, 29),
                    MusicGenre = "flamenco"
                },

                new Gig
                {
                    Name = "Bruce Springsteen",
                    GigDate = new DateTime(2020, 12, 2),
                    MusicGenre = "rock"
                }
            };
        }
    }
}