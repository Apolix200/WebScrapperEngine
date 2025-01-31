namespace WebScrapperEngine.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Creation")]
    public partial class Creation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Creation()
        {
            Bookmark = new HashSet<Bookmark>();
        }

        public int CreationId { get; set; }

        public int CreationType { get; set; }

        public int SiteName { get; set; }

        public string Title { get; set; }

        public string Link { get; set; }

        public string Image { get; set; }

        public int NewStatus { get; set; }

        public DateTime UpdatedAt { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bookmark> Bookmark { get; set; }
    }
}
