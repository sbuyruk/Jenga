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

INSERT INTO Auth_RoleMenu_Table (RoleId, MenuId, Aciklama)
SELECT @RoleId, m.Id, N'Sistem Yöneticisi -> ' + m.Title
FROM Auth_MenuItem_Table m
WHERE m.Id IN (1, 2, 3, 4, 5, 6, 7, 8)
  AND NOT EXISTS (
      SELECT 1
      FROM Auth_RoleMenu_Table rm
      WHERE rm.RoleId = @RoleId
        AND rm.MenuId = m.Id
  );