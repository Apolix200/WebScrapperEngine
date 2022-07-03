using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace WebScrapperEngine.Entity
{
    public partial class Context : DbContext
    {
        public Context()
            : base("name=ContextCon")
        {
        }

        public virtual DbSet<Bookmark> Bookmarks { get; set; }
        public virtual DbSet<Creation> Creations { get; set; }
        public virtual DbSet<Episode> Episodes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bookmark>()
                .HasMany(e => e.Episode)
                .WithRequired(e => e.Bookmark)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Creation>()
                .HasMany(e => e.Bookmark)
                .WithRequired(e => e.Creation)
                .WillCascadeOnDelete(false);
        }
    }
}
