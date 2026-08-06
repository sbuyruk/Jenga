SET NOCOUNT ON;

INSERT INTO Auth_Role_Table (Name, Aciklama)
SELECT v.Name, v.Aciklama
FROM (
    VALUES
        (N'Sistem Yöneticisi', N'Sistem yöneticisi rolü'),
        (N'Testçi', N'Testçi rolü'),
        (N'TBYS Veri Sorumlusu', N'TBYS veri sorumlusu rolü'),
        (N'IKYS Veri Sorumlusu', N'IKYS veri sorumlusu rolü'),
        (N'NBYS Veri Sorumlusu', N'NBYS veri sorumlusu rolü'),
        (N'Yönetici Özeti Görüntüleyenler', N'Yönetici özeti görüntüleyenler rolü')
) AS v(Name, Aciklama)
WHERE NOT EXISTS (
    SELECT 1
    FROM Auth_Role_Table r
    WHERE r.Name = v.Name
);