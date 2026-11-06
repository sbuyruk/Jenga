-- Mevcut İKYS menü erişimlerini kullanır. Yeni rol veya işlem izni oluşturmaz.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @ParentId int, @MenuId int;
SELECT TOP (1) @ParentId = ParentId FROM dbo.Auth_MenuItem_Table
WHERE Url = N'/ikys/personel-dashboard' ORDER BY Id;
IF @ParentId IS NULL
    SELECT TOP (1) @ParentId = Id FROM dbo.Auth_MenuItem_Table WHERE Title IN (N'İKYS', N'IKYS', N'İnsan Kaynakları') ORDER BY Id;
IF @ParentId IS NULL
BEGIN
    INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES(N'İKYS', NULL, N'#', 30, 1, N'İnsan Kaynakları');
    SET @ParentId = SCOPE_IDENTITY();
END;
SELECT TOP (1) @MenuId = Id FROM dbo.Auth_MenuItem_Table WHERE Url = N'/ikys/overtime' ORDER BY Id;
IF @MenuId IS NULL
BEGIN
    INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES(N'Fazla Mesai', @ParentId, N'/ikys/overtime', 50, 1, N'Personel, amir ve İK fazla mesai işlemleri');
    SET @MenuId = SCOPE_IDENTITY();
END;

DECLARE @Roles TABLE (Id int PRIMARY KEY);
INSERT @Roles(Id)
SELECT RoleId FROM dbo.Auth_RoleMenu_Table WHERE MenuId = @ParentId
UNION
SELECT rp.RoleId FROM dbo.Auth_RoleModulePermission_Table rp
JOIN dbo.Auth_ModulePermission_Table p ON p.Id = rp.ModulePermissionId
WHERE p.Module = 3 AND p.Operation IN (0,4);

-- Menü ağacının üst düğümleri de mevcut roller için görünür olmalıdır.
DECLARE @Node int = @MenuId;
DECLARE @Visited TABLE (Id int PRIMARY KEY);
WHILE @Node IS NOT NULL AND NOT EXISTS (SELECT 1 FROM @Visited WHERE Id = @Node)
BEGIN
    INSERT @Visited(Id) VALUES(@Node);
    INSERT dbo.Auth_RoleMenu_Table(RoleId, MenuId, Aciklama)
    SELECT r.Id, @Node, N'Mevcut İKYS erişimi: Fazla Mesai'
    FROM @Roles r WHERE NOT EXISTS (SELECT 1 FROM dbo.Auth_RoleMenu_Table m WHERE m.RoleId = r.Id AND m.MenuId = @Node);
    SET @Node = (SELECT ParentId FROM dbo.Auth_MenuItem_Table WHERE Id = @Node);
END;
COMMIT;
