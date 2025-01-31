Drop Table IF EXISTS SiteScrapper.dbo.Episode;

CREATE TABLE SiteScrapper.dbo.Episode(
	EpisodeId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	BookmarkId int NOT NULL,
	EpisodeNumber DOUBLE PRECISION NULL,
	Link nvarchar(max) NULL,
	WatchStatus int NOT NULL
)

ALTER TABLE SiteScrapper.dbo.Episode
ADD CONSTRAINT FK_EpisodeBookmark
FOREIGN KEY (BookmarkId) REFERENCES SiteScrapper.dbo.Bookmark(BookmarkId);

