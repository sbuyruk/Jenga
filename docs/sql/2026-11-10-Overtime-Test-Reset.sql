/*
FAZLA MESAİ TEST VERİLERİNİ TEMİZLEME

1. SSMS'te test için kullandığınız veritabanını seçin.
2. Önce bu haliyle çalıştırıp tablo/kayıt sayılarını inceleyin; @Sil = 0 veri silmez.
3. @BeklenenVeritabani değerine seçtiğiniz test veritabanının tam adını yazın.
4. Seçenekleri kontrol edip @Sil = 1 yaparak betiğin TAMAMINI yeniden çalıştırın.

KAPSAM: Aşağıdaki fazla mesai tablolarındaki TÜM kayıtlar silinir.
Test/gerçek kayıt ayrımı yapan bir alan yoktur; betik bu ayrımı yapamaz.
Dönemlerle birlikte kesinleşmiş dönemler ve yüklenen imzalı belgeler de silinir.
Personel, iş bilgileri, birim, unvan, menü ve genel rol/izin tablolarına dokunulmaz.
Tablolar, ilişkiler ve identity sayaçları korunur.
*/
SET NOCOUNT ON;

DECLARE @BeklenenVeritabani sysname = N'BURAYA_TEST_VERITABANI_ADI';
DECLARE @Sil bit = 0;                         -- 0: sadece önizleme, 1: silme
DECLARE @MesaiGirisYetkileriniSil bit = 1;    -- Başkası adına giriş atamalarını da temizle.
DECLARE @HaftalikSureTanimlariniSil bit = 0;  -- 40/45 saat tanımlarını yeniden girecekseniz 1 yapın.

DECLARE @Plan TABLE (
    Sira int PRIMARY KEY,
    Tablo sysname NOT NULL,
    Silinecek bit NOT NULL,
    TabloVar bit NOT NULL DEFAULT 0,
    KayitSayisi bigint NOT NULL DEFAULT 0,
    SilinenKayit bigint NOT NULL DEFAULT 0
);

-- Önce bağımlı kayıtlar, ardından dönemler temizlenir.
INSERT @Plan(Sira, Tablo, Silinecek) VALUES
    (1, N'OvertimeEvent_Table', 1),
    (2, N'OvertimeInterval_Table', 1),
    (3, N'OvertimeResult_Table', 1),
    (4, N'OvertimeEntry_Table', 1),
    (5, N'OvertimeDelivery_Table', 1),
    (6, N'OvertimePeriod_Table', 1),
    (7, N'OvertimeDelegation_Table', @MesaiGirisYetkileriniSil),
    (8, N'OvertimeSchedule_Table', @HaftalikSureTanimlariniSil);

DECLARE @Sira int = 1, @Tablo sysname, @Sql nvarchar(max), @Sayi bigint, @Silinecek bit, @TabloVar bit;

WHILE @Sira <= 8
BEGIN
    SELECT @Tablo = Tablo FROM @Plan WHERE Sira = @Sira;
    IF OBJECT_ID(N'dbo.' + @Tablo, N'U') IS NOT NULL
    BEGIN
        SET @Sayi = 0;
        SET @Sql = N'SELECT @Sonuc = COUNT_BIG(*) FROM dbo.' + QUOTENAME(@Tablo) + N';';
        EXEC sys.sp_executesql @Sql, N'@Sonuc bigint OUTPUT', @Sonuc = @Sayi OUTPUT;
        UPDATE @Plan SET TabloVar = 1, KayitSayisi = @Sayi WHERE Sira = @Sira;
    END;
    SET @Sira += 1;
END;

SELECT DB_NAME() AS SeciliVeritabani, @BeklenenVeritabani AS BeklenenVeritabani,
    @Sil AS SilmeEtkin, @MesaiGirisYetkileriniSil AS GirisYetkileriSilinecek,
    @HaftalikSureTanimlariniSil AS HaftalikSureTanimlariSilinecek;

SELECT Tablo, KayitSayisi,
    CASE WHEN TabloVar = 0 THEN N'Tablo bulunmuyor, atlanacak'
         WHEN Silinecek = 1 THEN N'Tüm kayıtları silinecek'
         ELSE N'Korunacak' END AS Islem
FROM @Plan ORDER BY Sira;

IF @Sil = 0
BEGIN
    PRINT N'ÖNİZLEME TAMAMLANDI. Hiçbir veri değiştirilmedi.';
    RETURN;
END;

IF @BeklenenVeritabani = N'BURAYA_TEST_VERITABANI_ADI'
    OR NULLIF(LTRIM(RTRIM(@BeklenenVeritabani)), N'') IS NULL
    OR DB_NAME() <> @BeklenenVeritabani
BEGIN
    ;THROW 51001, N'Silme yapılmadı: @BeklenenVeritabani alanına seçili test veritabanının tam adını yazın.', 1;
END;

IF DB_ID() <= 4
BEGIN
    ;THROW 51002, N'Sistem veritabanlarında temizleme yapılamaz.', 1;
END;

IF @@TRANCOUNT <> 0
BEGIN
    ;THROW 51003, N'Bu betiği açık bir transaction bulunmayan ayrı bir sorgu penceresinde çalıştırın.', 1;
END;

SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    SET @Sira = 1;
    WHILE @Sira <= 8
    BEGIN
        SELECT @Tablo = Tablo, @Silinecek = Silinecek, @TabloVar = TabloVar
        FROM @Plan WHERE Sira = @Sira;
        IF @Silinecek = 1 AND @TabloVar = 1
        BEGIN
            SET @Sayi = 0;
            -- Tablo adları yalnızca yukarıdaki sabit listeden alınır.
            SET @Sql = N'DELETE FROM dbo.' + QUOTENAME(@Tablo) + N' WITH (TABLOCKX); SET @Sonuc = @@ROWCOUNT;';
            EXEC sys.sp_executesql @Sql, N'@Sonuc bigint OUTPUT', @Sonuc = @Sayi OUTPUT;
            UPDATE @Plan SET SilinenKayit = @Sayi WHERE Sira = @Sira;
        END;
        SET @Sira += 1;
    END;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT Tablo, SilinenKayit FROM @Plan WHERE Silinecek = 1 ORDER BY Sira;
PRINT N'Fazla mesai test verileri temizlendi. Uygulamadaki açık ekranları yenileyin.';
