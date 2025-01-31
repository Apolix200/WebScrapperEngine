namespace WebScrapperEngine.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Bookmark")]
    public partial class Bookmark
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Bookmark()
        {
            Episode = new HashSet<Episode>();
        }

        public int BookmarkId { get; set; }

        public int CreationId { get; set; }

        public int? ConnectedId { get; set; }

        public int Completed { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual Creation Creation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Episode> Episode { get; set; }
    }
}
