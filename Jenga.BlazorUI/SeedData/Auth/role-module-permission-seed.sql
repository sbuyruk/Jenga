SET NOCOUNT ON;

DECLARE @RoleId INT;

SELECT TOP 1 @RoleId = Id
FROM Auth_Role_Table
WHERE Name = N'Sistem Yöneticisi'
ORDER BY Id;

IF @RoleId IS NULL
BEGIN
    PRINT 'Sistem Yöneticisi rolü bulunamadı.';
    RETURN;
END;

INSERT INTO Auth_RoleModulePermission_Table (RoleId, ModulePermissionId, Aciklama)
SELECT @RoleId, mp.Id, N'Sistem Yöneticisi -> ' + mp.Aciklama
FROM Auth_ModulePermission_Table mp
WHERE NOT EXISTS (
    SELECT 1
    FROM Auth_RoleModulePermission_Table rmp
    WHERE rmp.RoleId = @RoleId
      AND rmp.ModulePermissionId = mp.Id
);