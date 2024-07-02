using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyazBerdroParsing.Models
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Get DB source");
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<FilmComment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // İlişkiyi belirtmek için gerekli olan kod
            modelBuilder.Entity<FilmComment>()
                 .HasOne(c => c.Movie)
                 .WithMany(p => p.Comments)
                 .HasForeignKey(c => c.MovieId)
                 .OnDelete(DeleteBehavior.NoAction);
        }

    }
}
