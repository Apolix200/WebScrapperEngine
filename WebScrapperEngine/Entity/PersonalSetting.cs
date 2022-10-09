namespace WebScrapperEngine.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PersonalSetting")]
    public partial class PersonalSetting
    {
        public int PersonalSettingId { get; set; }

        public int EpisodePictureSize { get; set; }

        public int CreationPictureSize { get; set; }
    }
}
