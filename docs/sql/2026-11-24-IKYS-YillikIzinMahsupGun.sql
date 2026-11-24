-- Dilekçede yazan gelecek dönem mahsup süresini kaydeder.
SET XACT_ABORT ON;

IF COL_LENGTH(N'dbo.IzinTalep_Table', N'GelecekDonemMahsubu') IS NULL
    THROW 50001, N'Önce yıllık izin mahsup betiğini çalıştırınız.', 1;

IF COL_LENGTH(N'dbo.IzinTalep_Table', N'GelecekDonemMahsupGun') IS NULL
    ALTER TABLE dbo.IzinTalep_Table ADD GelecekDonemMahsupGun int NULL;
