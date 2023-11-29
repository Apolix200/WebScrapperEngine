Drop Table IF EXISTS SiteScrapper.dbo.Bookmark;

CREATE TABLE SiteScrapper.dbo.Bookmark(
	BookmarkId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	CreationId int NOT NULL,
	ConnectedId int NULL,
	Completed int NOT NULL,
	UpdatedAt DATETIME NOT NULL
)

ALTER TABLE SiteScrapper.dbo.Bookmark
ADD CONSTRAINT FK_CreationBookmark
FOREIGN KEY (CreationId) REFERENCES SiteScrapper.dbo.Creation(CreationId);