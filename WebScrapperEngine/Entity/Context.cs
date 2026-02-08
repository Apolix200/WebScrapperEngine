using System.Data.Entity;

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
        public virtual DbSet<BookmarkCreation> BookmarkCreations { get; set; }
        public virtual DbSet<Episode> Episodes { get; set; }
        public virtual DbSet<PersonalSetting> PersonalSettings { get; set; }

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

            modelBuilder.Entity<BookmarkCreation>()
                .HasKey(bc => new { bc.BookmarkId, bc.CreationId });

            modelBuilder.Entity<BookmarkCreation>()
                .HasRequired(bc => bc.Bookmark)
                .WithMany(b => b.BookmarkCreations)
                .HasForeignKey(bc => bc.BookmarkId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<BookmarkCreation>()
                .HasRequired(bc => bc.Creation)
                .WithMany(c => c.BookmarkCreations)
                .HasForeignKey(bc => bc.CreationId)
                .WillCascadeOnDelete(true);
        }
    }
}
