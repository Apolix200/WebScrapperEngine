using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapperEngine.Entity
{

    [Table("PersonalSetting")]
    public partial class PersonalSetting
    {
        public int PersonalSettingId { get; set; }

        public int EpisodePictureSize { get; set; }

        public int CreationPictureSize { get; set; }

        public int Filter { get; set; }

        public int DatasourceFilter { get; set; }
       
    }
}
