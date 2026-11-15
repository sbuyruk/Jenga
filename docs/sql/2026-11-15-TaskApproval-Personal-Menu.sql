-- Kişisel görev listesi menüsünü ekler. Mevcut modül izinlerini değiştirmez.
-- Yeniden çalıştırılabilir; uygulama yayını sonrasında hedef veritabanında uygulanır.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @IkysId int, @ParentId int, @PersonalId int;
SELECT TOP (1) @IkysId = Id FROM dbo.Auth_MenuItem_Table
WHERE Title IN (N'İKYS', N'IKYS', N'İnsan Kaynakları') ORDER BY Id;
IF @IkysId IS NULL
    SELECT TOP (1) @IkysId = ParentId FROM dbo.Auth_MenuItem_Table
    WHERE Url = N'/ikys/personel-dashboard' ORDER BY Id;
IF @IkysId IS NULL
    THROW 50000, N'İKYS menüsü bulunamadı. Önce İKYS ana menüsünü yapılandırın.', 1;
SELECT TOP (1) @ParentId = Id FROM dbo.Auth_MenuItem_Table
WHERE ParentId = @IkysId AND Title IN (N'Kullanıcı İşlemleri', N'Kullanıcı Menüsü', N'Personel İşlemleri') ORDER BY Id;
SET @ParentId = COALESCE(@ParentId, @IkysId);
SELECT TOP (1) @PersonalId = Id FROM dbo.Auth_MenuItem_Table
WHERE Url = N'/ikys/task-approvals/personal' ORDER BY Id;
IF @PersonalId IS NULL
BEGIN
    INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES(N'Kişisel Görev Onay Listem', @ParentId, N'/ikys/task-approvals/personal', 45, 1, N'Tüm kişisel görev geçmişi');
    SET @PersonalId = SCOPE_IDENTITY();
END;
DECLARE @Roles TABLE (Id int PRIMARY KEY);
INSERT @Roles(Id)
SELECT DISTINCT rp.RoleId FROM dbo.Auth_RoleModulePermission_Table rp
JOIN dbo.Auth_ModulePermission_Table p ON p.Id = rp.ModulePermissionId
WHERE p.Module = 3 AND p.Operation IN (0, 4);
;WITH MenuPath AS
(
    SELECT Id, ParentId FROM dbo.Auth_MenuItem_Table WHERE Id = @PersonalId
    UNION ALL
    SELECT m.Id, m.ParentId FROM dbo.Auth_MenuItem_Table m JOIN MenuPath x ON x.ParentId = m.Id
)
INSERT dbo.Auth_RoleMenu_Table(RoleId, MenuId, Aciklama)
SELECT DISTINCT r.Id, m.Id, N'Mevcut İKYS erişimi: Kişisel Görev Onay Listem'
FROM @Roles r CROSS JOIN MenuPath m
WHERE NOT EXISTS (SELECT 1 FROM dbo.Auth_RoleMenu_Table x WHERE x.RoleId = r.Id AND x.MenuId = m.Id)
OPTION (MAXRECURSION 100);
COMMIT;
