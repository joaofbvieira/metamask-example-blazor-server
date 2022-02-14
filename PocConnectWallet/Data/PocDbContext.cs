using Microsoft.EntityFrameworkCore;
using PocConnectWallet.Models;

namespace PocConnectWallet.Data
{
    public class PocDbContext : DbContext
    {
        public DbSet<UserData> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./Users.sqlite");
        }
    }
}
