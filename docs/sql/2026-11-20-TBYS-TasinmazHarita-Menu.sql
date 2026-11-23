SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @EskiAdres nvarchar(50) = N'tbys/tasinmaz-kontrol/harita';

IF EXISTS (SELECT 1 FROM dbo.Auth_MenuItem_Table WHERE Url IN (@EskiAdres, N'/' + @EskiAdres))
BEGIN
    DECLARE @TbysId int;
    SELECT TOP (1) @TbysId = Id FROM dbo.Auth_MenuItem_Table
    WHERE Title = N'TBYS' AND ParentId IS NULL ORDER BY Id;

    IF @TbysId IS NULL
        SELECT TOP (1) @TbysId = ParentId FROM dbo.Auth_MenuItem_Table
        WHERE Url = N'/tbys/tasinmaz-kontrol' AND ParentId IS NOT NULL ORDER BY Id;

    IF @TbysId IS NULL
        THROW 50000, N'TBYS ana menüsü bulunamadı.', 1;

    UPDATE dbo.Auth_MenuItem_Table
    SET Url = N'/tbys/tasinmaz-harita', ParentId = @TbysId
    WHERE Url IN (@EskiAdres, N'/' + @EskiAdres);
END;

COMMIT;
