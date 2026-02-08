using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapperEngine.Entity
{

    [Table("Episode")]
    public partial class Episode
    {
        public int EpisodeId { get; set; }

        public int BookmarkId { get; set; }

        public double? EpisodeNumber { get; set; }

        public string Link { get; set; }

        public int WatchStatus { get; set; }

        public virtual Bookmark Bookmark { get; set; }
    }
}
