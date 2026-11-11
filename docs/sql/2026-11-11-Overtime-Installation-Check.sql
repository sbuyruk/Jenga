-- SALT OKUNUR: SSMS'te production Jenga veritabanını seçip çalıştırın.
-- Hiçbir tablo veya veri oluşturmaz/değiştirmez.
SELECT DB_NAME() AS SeciliVeritabani;

SELECT s.name AS Sema, t.name AS Tablo
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.name LIKE N'Overtime%'
   OR t.name IN (N'Personel_Table', N'IsBilgileri_Table')
ORDER BY s.name, t.name;

SELECT required.Tablo,
       CASE WHEN OBJECT_ID(N'dbo.' + required.Tablo, N'U') IS NULL
            THEN N'YOK' ELSE N'VAR' END AS Durum
FROM (VALUES (N'Personel_Table'), (N'IsBilgileri_Table'),
             (N'OvertimePeriod_Table'), (N'OvertimeEntry_Table'),
             (N'OvertimeInterval_Table'), (N'OvertimeDelivery_Table'),
             (N'OvertimeSchedule_Table'), (N'OvertimeResult_Table'),
             (N'OvertimeEvent_Table'), (N'OvertimeDelegation_Table')) required(Tablo);
