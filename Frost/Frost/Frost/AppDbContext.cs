using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Windows.System;
using Windows.UI;

namespace Frost
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }
}
