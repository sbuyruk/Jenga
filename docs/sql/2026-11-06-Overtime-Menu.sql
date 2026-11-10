-- Mesailerim ve IK Donem Islemleri ayri menulere tasinir.
-- Mevcut /ikys/overtime kaydi korunur; yeni rol veya islem izni olusturulmaz.
-- Daha once uygulanmis kurulumlarda da tekrar calistirilabilir.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @IkysId int, @UserParentId int, @HrParentId int, @UserMenuId int, @HrMenuId int, @DelegationMenuId int;
SELECT TOP (1) @IkysId = Id FROM dbo.Auth_MenuItem_Table
WHERE Title IN (N'İKYS', N'IKYS', N'İnsan Kaynakları') ORDER BY Id;
IF @IkysId IS NULL
    SELECT TOP (1) @IkysId = ParentId FROM dbo.Auth_MenuItem_Table
    WHERE Url = N'/ikys/personel-dashboard' ORDER BY Id;
IF @IkysId IS NULL
BEGIN
    INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES(N'İKYS', NULL, N'#', 30, 1, N'İnsan Kaynakları');
    SET @IkysId = SCOPE_IDENTITY();
END;
SELECT TOP (1) @UserParentId = Id FROM dbo.Auth_MenuItem_Table
WHERE ParentId = @IkysId AND Title IN (N'Kullanıcı İşlemleri', N'Kullanıcı Menüsü', N'Personel İşlemleri') ORDER BY Id;
IF @UserParentId IS NULL
BEGIN
    INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES(N'Kullanıcı İşlemleri', @IkysId, N'#', 10, 1, N'Personelin kendi işlemleri ve amir incelemeleri');
    SET @UserParentId = SCOPE_IDENTITY();
END;
SELECT TOP (1) @HrParentId = Id FROM dbo.Auth_MenuItem_Table
WHERE ParentId = @IkysId AND Title IN (N'İK Personeli', N'IK Personeli', N'İK Personel İşlemleri') ORDER BY Id;
IF @HrParentId IS NULL
BEGIN
    INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES(N'İK Personeli', @IkysId, N'#', 20, 1, N'İnsan Kaynakları personelinin işlemleri');
    SET @HrParentId = SCOPE_IDENTITY();
END;
SELECT TOP (1) @UserMenuId = Id FROM dbo.Auth_MenuItem_Table WHERE Url = N'/ikys/overtime' ORDER BY Id;
IF @UserMenuId IS NULL
BEGIN
    INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES(N'Mesailerim', @UserParentId, N'/ikys/overtime', 50, 1, N'Mesai girişi, takip ve amir işlemleri');
    SET @UserMenuId = SCOPE_IDENTITY();
END
ELSE
    UPDATE dbo.Auth_MenuItem_Table SET Title = N'Mesailerim', ParentId = @UserParentId,
        Aciklama = N'Mesai girişi, takip ve amir işlemleri' WHERE Id = @UserMenuId;
SELECT TOP (1) @HrMenuId = Id FROM dbo.Auth_MenuItem_Table WHERE Url = N'/ikys/overtime/hr' ORDER BY Id;
IF @HrMenuId IS NULL
BEGIN
    INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES(N'Fazla Mesai Dönem İşlemleri', @HrParentId, N'/ikys/overtime/hr', 50, 1, N'Toplama çağrısı, teslim, hesaplama, ıslak imza ve arşiv');
    SET @HrMenuId = SCOPE_IDENTITY();
END
ELSE
    UPDATE dbo.Auth_MenuItem_Table SET ParentId = @HrParentId WHERE Id = @HrMenuId;
SELECT TOP (1) @DelegationMenuId = Id FROM dbo.Auth_MenuItem_Table WHERE Url = N'/ikys/overtime/delegations' ORDER BY Id;
IF @DelegationMenuId IS NULL
BEGIN
    INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES(N'Mesai Giriş Yetkileri', @HrParentId, N'/ikys/overtime/delegations', 60, 1, N'Personel adına mesai giriş yetkilerini yönetme');
    SET @DelegationMenuId = SCOPE_IDENTITY();
END;
DECLARE @UserRoles TABLE (Id int PRIMARY KEY);
INSERT @UserRoles(Id)
SELECT RoleId FROM dbo.Auth_RoleMenu_Table WHERE MenuId IN (@UserMenuId, @UserParentId, @IkysId)
UNION
SELECT rp.RoleId FROM dbo.Auth_RoleModulePermission_Table rp
JOIN dbo.Auth_ModulePermission_Table p ON p.Id = rp.ModulePermissionId
WHERE p.Module = 3 AND p.Operation IN (0,4);
DECLARE @HrRoles TABLE (Id int PRIMARY KEY);
INSERT @HrRoles(Id)
SELECT DISTINCT rp.RoleId FROM dbo.Auth_RoleModulePermission_Table rp
JOIN dbo.Auth_ModulePermission_Table p ON p.Id = rp.ModulePermissionId
WHERE p.Module = 3 AND p.Operation = 4;
-- Menu gorunurlugu mevcut rollerden gelir; asil islem yetkileri serviste kontrol edilir.
INSERT dbo.Auth_RoleMenu_Table(RoleId, MenuId, Aciklama)
SELECT r.Id, m.Id, N'Mevcut İKYS erişimi: Mesailerim'
FROM @UserRoles r CROSS JOIN (VALUES(@UserMenuId), (@UserParentId), (@IkysId)) m(Id)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Auth_RoleMenu_Table x WHERE x.RoleId = r.Id AND x.MenuId = m.Id);
INSERT dbo.Auth_RoleMenu_Table(RoleId, MenuId, Aciklama)
SELECT r.Id, m.Id, N'Mevcut İKYS yönetim erişimi: Dönem İşlemleri'
FROM @HrRoles r CROSS JOIN (VALUES(@HrMenuId), (@DelegationMenuId), (@HrParentId), (@IkysId)) m(Id)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Auth_RoleMenu_Table x WHERE x.RoleId = r.Id AND x.MenuId = m.Id);
COMMIT;
