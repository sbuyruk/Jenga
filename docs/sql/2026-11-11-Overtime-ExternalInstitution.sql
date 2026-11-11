-- Mevcut Jenga veritabanında uygulama güncellenmeden önce çalıştırılır.
-- Personel tipi betiğini de kapsar; iki betiği ayrıca çalıştırmak gerekmez.
-- Tekrar çalıştırılabilir. Kayıt veya imzalı dosya silmez, süreleri değiştirmez.
-- Kadrolu personelde kurum boş olabilir. Kadro haricide zorunluluk servislerde uygulanır.
-- Eski kayıtlarda tarihsel tip/kurum yoksa mevcut personel ve iş bilgisiyle doldurulur.
-- Kurumu bilinmeyen personele uydurma bir kurum atanmaz; İK ekranından tamamlanır.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.IsBilgileri_Table', N'U') IS NULL
    OR OBJECT_ID(N'dbo.OvertimeEntry_Table', N'U') IS NULL
    OR OBJECT_ID(N'dbo.OvertimeResult_Table', N'U') IS NULL
    THROW 50001, N'İş bilgileri ve temel fazla mesai tabloları bulunmalıdır.', 1;

IF COL_LENGTH(N'dbo.IsBilgileri_Table', N'KadroHariciKurum') IS NULL
    ALTER TABLE dbo.IsBilgileri_Table ADD KadroHariciKurum nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.OvertimeEntry_Table', N'PersonnelType') IS NULL
    ALTER TABLE dbo.OvertimeEntry_Table ADD PersonnelType int NULL;
IF COL_LENGTH(N'dbo.OvertimeResult_Table', N'PersonnelType') IS NULL
    ALTER TABLE dbo.OvertimeResult_Table ADD PersonnelType int NULL;
IF COL_LENGTH(N'dbo.OvertimeEntry_Table', N'KadroHariciKurum') IS NULL
    ALTER TABLE dbo.OvertimeEntry_Table ADD KadroHariciKurum nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.OvertimeResult_Table', N'KadroHariciKurum') IS NULL
    ALTER TABLE dbo.OvertimeResult_Table ADD KadroHariciKurum nvarchar(200) NULL;

EXEC sp_executesql N'
    UPDATE e SET PersonnelType = p.Tipi
    FROM dbo.OvertimeEntry_Table e INNER JOIN dbo.Personel_Table p ON p.Id = e.PersonelId
    WHERE e.PersonnelType IS NULL AND p.Tipi IN (1, 2);
    UPDATE r SET PersonnelType = p.Tipi
    FROM dbo.OvertimeResult_Table r INNER JOIN dbo.Personel_Table p ON p.Id = r.PersonelId
    WHERE r.PersonnelType IS NULL AND p.Tipi IN (1, 2);

    UPDATE e SET KadroHariciKurum = LTRIM(RTRIM(i.KadroHariciKurum))
    FROM dbo.OvertimeEntry_Table e INNER JOIN dbo.IsBilgileri_Table i ON i.PersonelId = e.PersonelId
    WHERE e.PersonnelType = 2 AND NULLIF(LTRIM(RTRIM(e.KadroHariciKurum)), N'''') IS NULL
        AND NULLIF(LTRIM(RTRIM(i.KadroHariciKurum)), N'''') IS NOT NULL;

    UPDATE r SET KadroHariciKurum = s.Kurum
    FROM dbo.OvertimeResult_Table r
    INNER JOIN (
        SELECT PeriodId, PersonelId, MIN(KadroHariciKurum) AS Kurum
        FROM dbo.OvertimeEntry_Table
        WHERE PersonnelType = 2 AND NULLIF(LTRIM(RTRIM(KadroHariciKurum)), N'''') IS NOT NULL
        GROUP BY PeriodId, PersonelId HAVING COUNT(DISTINCT KadroHariciKurum) = 1
    ) s ON s.PeriodId = r.PeriodId AND s.PersonelId = r.PersonelId
    WHERE r.PersonnelType = 2 AND NULLIF(LTRIM(RTRIM(r.KadroHariciKurum)), N'''') IS NULL;

    SELECT p.Id AS PersonelId, p.Adi, p.Soyadi, i.KadroHariciKurum
    FROM dbo.Personel_Table p LEFT JOIN dbo.IsBilgileri_Table i ON i.PersonelId = p.Id
    WHERE p.Tipi = 2 AND NULLIF(LTRIM(RTRIM(i.KadroHariciKurum)), N'''') IS NULL;
';

COMMIT;
