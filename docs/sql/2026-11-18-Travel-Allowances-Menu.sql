-- Harcırah tanımları menüsünü ekler/günceller. Yeni modül izni vermez.
-- Mevcut IKYS/Manage rollerine menü görünürlüğü tanımlar. Yeniden çalıştırılabilir.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @IkysId int, @ParentId int, @MenuId int;
SELECT TOP (1) @IkysId = Id FROM dbo.Auth_MenuItem_Table
WHERE Title IN (N'İKYS', N'IKYS', N'İnsan Kaynakları') ORDER BY Id;
IF @IkysId IS NULL
    SELECT TOP (1) @IkysId = ParentId FROM dbo.Auth_MenuItem_Table WHERE Url = N'/ikys/personel-dashboard' ORDER BY Id;
IF @IkysId IS NULL
    THROW 50000, N'İKYS ana menüsü bulunamadı. Önce ana menüyü yapılandırın.', 1;
SELECT TOP (1) @ParentId = Id FROM dbo.Auth_MenuItem_Table
WHERE ParentId = @IkysId AND Title IN (N'Tanımlar', N'Tanımlamalar', N'İKYS Tanımları') ORDER BY Id;
SET @ParentId = COALESCE(@ParentId, @IkysId);
SELECT TOP (1) @MenuId = Id FROM dbo.Auth_MenuItem_Table
WHERE Url IN (N'/ikys/travel-allowances', N'/IKYS/HarcirahGirisWP.aspx', N'/IKYS/HarcirahGiris.aspx') ORDER BY Id;
IF @MenuId IS NULL
BEGIN
    INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES(N'Harcırah Tanımları', @ParentId, N'/ikys/travel-allowances', 55, 1, N'Ülke, tarih serisi ve kadro grubuna göre harcırah tanımları');
    SET @MenuId = SCOPE_IDENTITY();
END
ELSE
    UPDATE dbo.Auth_MenuItem_Table SET Url = N'/ikys/travel-allowances', Title = N'Harcırah Tanımları' WHERE Id = @MenuId;
DECLARE @Roles TABLE (Id int PRIMARY KEY);
INSERT @Roles(Id)
SELECT DISTINCT rp.RoleId FROM dbo.Auth_RoleModulePermission_Table rp
JOIN dbo.Auth_ModulePermission_Table p ON p.Id = rp.ModulePermissionId
WHERE p.Module = 3 AND p.Operation = 4;
;WITH MenuPath AS
(
    SELECT Id, ParentId FROM dbo.Auth_MenuItem_Table WHERE Id = @MenuId
    UNION ALL
    SELECT m.Id, m.ParentId FROM dbo.Auth_MenuItem_Table m JOIN MenuPath x ON x.ParentId = m.Id
)
INSERT dbo.Auth_RoleMenu_Table(RoleId, MenuId, Aciklama)
SELECT DISTINCT r.Id, m.Id, N'Mevcut İKYS yönetim erişimi: Harcırah Tanımları'
FROM @Roles r CROSS JOIN MenuPath m
WHERE NOT EXISTS (SELECT 1 FROM dbo.Auth_RoleMenu_Table x WHERE x.RoleId = r.Id AND x.MenuId = m.Id)
OPTION (MAXRECURSION 100);
COMMIT;
