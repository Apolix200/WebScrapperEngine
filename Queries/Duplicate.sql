/****** Script for SelectTopNRows command from SSMS  ******/
SELECT Title, SiteName, COUNT(*)
FROM [SiteScrapper].[dbo].[AnimeCreation]
GROUP BY Title, SiteName
HAVING COUNT(*) > 1