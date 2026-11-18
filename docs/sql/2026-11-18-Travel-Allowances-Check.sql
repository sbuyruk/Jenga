-- Salt okunur şema kontrolü. Tablo/tutar oluşturmaz, mevcut kaydı değiştirmez.
SET NOCOUNT ON;
SELECT expected.ColumnName, TYPE_NAME(c.user_type_id) AS VeriTipi, c.is_nullable AS BosOlabilir,
       CASE WHEN c.column_id IS NULL THEN N'EKSİK' ELSE N'VAR' END AS Durum
FROM (VALUES
    (N'Id'), (N'KadroGrupId'), (N'SeriId'), (N'Sira'), (N'KadroGrubu'), (N'Ulke'),
    (N'Miktar'), (N'ParaBirimi'), (N'BaslangicTarihi'), (N'BitisTarihi'), (N'Aciklama'),
    (N'Olusturan'), (N'OlusturmaTarihi'), (N'Degistiren'), (N'DegistirmeTarihi')
) expected(ColumnName)
LEFT JOIN sys.columns c ON c.object_id = OBJECT_ID(N'dbo.Harcirah_Table') AND c.name = expected.ColumnName
ORDER BY expected.ColumnName;
