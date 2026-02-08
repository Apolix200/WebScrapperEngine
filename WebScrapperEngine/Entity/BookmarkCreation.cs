using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapperEngine.Entity
{
    [Table("BookmarkCreation")]
    public partial class BookmarkCreation
    {
        public int BookmarkId { get; set; }
        public int CreationId { get; set; }

        public virtual Bookmark Bookmark { get; set; }
        public virtual Creation Creation { get; set; }
    }
}
