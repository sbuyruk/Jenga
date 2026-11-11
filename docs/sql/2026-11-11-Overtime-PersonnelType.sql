-- Mevcut fazla mesai kurulumu için personel tipi anlık görüntüsü.
-- Hedef Jenga veritabanında uygulama güncellenmeden önce çalıştırılır.
-- Tekrar çalıştırılabilir; mesai ve hesaplanan süreleri silmez/değiştirmez.
-- Eski kayıtlarda tarihsel personel tipi tutulmadığından, ilk doldurmada
-- Personel_Table.Tipi kullanılır. Mevcut imzalı dosyalar değiştirilmez.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.OvertimeEntry_Table', N'U') IS NULL
    OR OBJECT_ID(N'dbo.OvertimeResult_Table', N'U') IS NULL
    THROW 50001, N'Önce temel fazla mesai kurulum betiğini uygulayın.', 1;

IF COL_LENGTH(N'dbo.OvertimeEntry_Table', N'PersonnelType') IS NULL
    ALTER TABLE dbo.OvertimeEntry_Table ADD PersonnelType int NULL;
IF COL_LENGTH(N'dbo.OvertimeResult_Table', N'PersonnelType') IS NULL
    ALTER TABLE dbo.OvertimeResult_Table ADD PersonnelType int NULL;

-- Dinamik SQL, yeni eklenen sütunların aynı batch içinde derlenmesini sağlar.
EXEC sp_executesql N'
    UPDATE e SET PersonnelType = p.Tipi
    FROM dbo.OvertimeEntry_Table e
    INNER JOIN dbo.Personel_Table p ON p.Id = e.PersonelId
    WHERE e.PersonnelType IS NULL AND p.Tipi IN (1, 2);

    UPDATE r SET PersonnelType = p.Tipi
    FROM dbo.OvertimeResult_Table r
    INNER JOIN dbo.Personel_Table p ON p.Id = r.PersonelId
    WHERE r.PersonnelType IS NULL AND p.Tipi IN (1, 2);
';

COMMIT;
