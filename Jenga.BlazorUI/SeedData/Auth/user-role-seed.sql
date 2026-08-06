SET NOCOUNT ON;

DECLARE @RoleId INT;
DECLARE @PersonelId INT = 127;

SELECT TOP 1 @RoleId = Id
FROM Auth_Role_Table
WHERE Name = N'Sistem Yöneticisi'
ORDER BY Id;

IF @RoleId IS NULL
BEGIN
    PRINT 'Sistem Yöneticisi rolü bulunamadı.';
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM Auth_PersonnelRole_Table
    WHERE PersonelId = @PersonelId
      AND RoleId = @RoleId
)
BEGIN
    INSERT INTO Auth_PersonnelRole_Table (PersonelId, RoleId, Aciklama)
    VALUES (@PersonelId, @RoleId, N'asbuyruk -> Sistem Yöneticisi');
END;