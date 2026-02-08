using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapperEngine.Entity
{

    [Table("Bookmark")]
    public partial class Bookmark
    {
        public Bookmark()
        {
            Episode = new HashSet<Episode>();
            BookmarkCreations = new HashSet<BookmarkCreation>();
        }

        public int BookmarkId { get; set; }

        public int CreationId { get; set; }

        public int Completed { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual Creation Creation { get; set; }
        public virtual ICollection<Episode> Episode { get; set; }
        public virtual ICollection<BookmarkCreation> BookmarkCreations { get; set; }
    }
}
