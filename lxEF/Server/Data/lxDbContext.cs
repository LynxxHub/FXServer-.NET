using lxEF.Server.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace lxEF.Server.Data
{
    public class lxDbContext : DbContext
    {

        public DbSet<DBUser> DBUsers { get; set; }

        public DbSet<Character> Characters { get; set; }

        // Other DbSets can be added here

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //change this
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("server=localhost;database=lx_ef;user=root;password=;");
            }
        }
    }
}
