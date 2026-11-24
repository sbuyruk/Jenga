-- Jenga'nın gelecek dönem yıllık izin mahsup taleplerini tanıması için.
-- Mahsup_Table, PortalApps'in kullandığı mevcut tablodur.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.IzinTalep_Table', N'U') IS NULL OR OBJECT_ID(N'dbo.Mahsup_Table', N'U') IS NULL
    THROW 50001, N'İKYS izin talebi veya mahsup tablosu bulunamadı.', 1;

IF COL_LENGTH(N'dbo.IzinTalep_Table', N'GelecekDonemMahsubu') IS NULL
    ALTER TABLE dbo.IzinTalep_Table ADD GelecekDonemMahsubu bit NULL;

EXEC sys.sp_executesql N'
    UPDATE dbo.IzinTalep_Table
    SET GelecekDonemMahsubu = 1
    WHERE IzinTipi = 1 AND OnayDurumu = 5 AND GelecekDonemMahsubu IS NULL;';

COMMIT;
