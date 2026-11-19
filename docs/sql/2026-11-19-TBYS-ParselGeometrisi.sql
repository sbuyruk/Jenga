SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Tasinmaz_Table', N'U') IS NULL
    THROW 51000, N'dbo.Tasinmaz_Table bulunamadı.', 1;

IF COL_LENGTH(N'dbo.Tasinmaz_Table', N'ParselGeometrisiGeoJson') IS NULL
    ALTER TABLE dbo.Tasinmaz_Table ADD ParselGeometrisiGeoJson NVARCHAR(MAX) NULL;
