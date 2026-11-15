-- Salt okunur kontrol. Hedef IKYS veritabanında çalıştırılır.
-- Tutar veya şema oluşturmaz/değiştirmez.
SET NOCOUNT ON;

SELECT expected.TableName, expected.ColumnName,
       CASE WHEN c.column_id IS NULL THEN N'EKSİK' ELSE N'VAR' END AS Durum
FROM (VALUES
    (N'Harcirah_Table', N'Id'),
    (N'Harcirah_Table', N'KadroGrupId'),
    (N'Harcirah_Table', N'SeriId'),
    (N'Harcirah_Table', N'Sira'),
    (N'Harcirah_Table', N'KadroGrubu'),
    (N'Harcirah_Table', N'Ulke'),
    (N'Harcirah_Table', N'Miktar'),
    (N'Harcirah_Table', N'ParaBirimi'),
    (N'Harcirah_Table', N'BaslangicTarihi'),
    (N'Harcirah_Table', N'BitisTarihi'),
    (N'GorevTanim_Table', N'HarcirahGrupId')
) expected(TableName, ColumnName)
LEFT JOIN sys.columns c
    ON c.object_id = OBJECT_ID(N'dbo.' + expected.TableName)
   AND c.name = expected.ColumnName
ORDER BY expected.TableName, expected.ColumnName;
