namespace DockerGigApi.Data
{
    using Microsoft.EntityFrameworkCore;
    using System.Reflection;
    using DockerGigApi.Entities;
    public class GigContext : DbContext
    {
        public GigContext(DbContextOptions<GigContext> options)
            : base(options)
        {
        }
        
        public virtual DbSet<Gig> Gigs { get; set; }
    }
}
