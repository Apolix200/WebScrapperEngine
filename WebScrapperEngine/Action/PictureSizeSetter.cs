using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapperEngine.Entity;

namespace WebScrapperEngine.Action
{
    class PictureSizeSetter
    {
        private MainWindow mainWindow;
        private Context context;

        private const int bigImageSize = 120;
        private const int smallImageSize = 40;

        public int EpisodePictureSize { get; set; }
        public int CreationPictureSize { get; set; }

        public PictureSizeSetter(MainWindow mainWindow, Context context)
        {
            this.mainWindow = mainWindow;
            this.context = context;


            CreationPictureSize = context.PersonalSettings.FirstOrDefault().CreationPictureSize;
            EpisodePictureSize = context.PersonalSettings.FirstOrDefault().EpisodePictureSize;

            mainWindow.creationsDataGridImage.Width = CreationPictureSize;
            mainWindow.creationsDataGrid.Items.Refresh();

            mainWindow.episodesdataGridImage.Width = EpisodePictureSize;
            mainWindow.episodesDataGrid.Items.Refresh();
        }

        public void ChangeSize(DatasourceFilter datasourceFilter)
        {
            switch (datasourceFilter)
            {
                case DatasourceFilter.Creations:
                    int creationImageSize = context.PersonalSettings.FirstOrDefault().CreationPictureSize == bigImageSize ? smallImageSize : bigImageSize;
                    CreationPictureSize = creationImageSize;

                    mainWindow.creationsDataGridImage.Width = creationImageSize;
                    mainWindow.creationsDataGrid.Items.Refresh();

                    context.PersonalSettings.FirstOrDefault().CreationPictureSize = creationImageSize;
                    context.SaveChanges();
                    break;
                case DatasourceFilter.Episodes:
                    int episodeImageSize = context.PersonalSettings.FirstOrDefault().EpisodePictureSize == bigImageSize ? smallImageSize : bigImageSize;
                    EpisodePictureSize = episodeImageSize;

                    mainWindow.episodesdataGridImage.Width = episodeImageSize;
                    mainWindow.episodesDataGrid.Items.Refresh();

                    context.PersonalSettings.FirstOrDefault().EpisodePictureSize = episodeImageSize;
                    context.SaveChanges();
                    break;
                default:
                    break;
            }
        }
    }
}
