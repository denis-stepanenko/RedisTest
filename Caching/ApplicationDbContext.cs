using Caching.Models;
using Microsoft.EntityFrameworkCore;

namespace Caching
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set;}
    }
}