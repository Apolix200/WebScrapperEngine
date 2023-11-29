Drop Table IF EXISTS SiteScrapper.dbo.PersonalSetting;

CREATE TABLE SiteScrapper.dbo.PersonalSetting(
	PersonalSettingId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	EpisodePictureSize int NOT NULL,
	CreationPictureSize int NOT NULL 
)

