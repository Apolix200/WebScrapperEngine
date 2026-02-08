using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapperEngine.Entity
{   

    [Table("Creation")]
    public partial class Creation
    {
        public Creation()
        {
            Bookmark = new HashSet<Bookmark>();
            BookmarkCreations = new HashSet<BookmarkCreation>();
        }

        public int CreationId { get; set; }

        public int CreationType { get; set; }

        public int SiteName { get; set; }

        public string Title { get; set; }

        public string Link { get; set; }

        public string Image { get; set; }

        public int NewStatus { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Bookmark> Bookmark { get; set; }

        public virtual ICollection<BookmarkCreation> BookmarkCreations { get; set; }
    }
}
