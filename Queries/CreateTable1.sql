Drop Table IF EXISTS SiteScrapper.dbo.Creation;

CREATE TABLE SiteScrapper.dbo.Creation(
	CreationId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	CreationType int NOT NULL,
	SiteName int NOT NULL,
	Title nvarchar(max) NULL,
	Link nvarchar(max) NULL,
	Image nvarchar(max) NULL,
	NewStatus int NOT NULL,
	UpdatedAt DATETIME NOT NULL
);
