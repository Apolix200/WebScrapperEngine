namespace WebScrapperEngine.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

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
