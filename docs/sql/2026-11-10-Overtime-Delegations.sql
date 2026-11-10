-- Mevcut fazla mesai kurulumu için kişi bazlı mesai giriş yetkileri.
-- Tekrar çalıştırılabilir. Mevcut roller ve işlem izinleri korunur.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.OvertimeDelegation_Table', N'U') IS NULL
CREATE TABLE dbo.OvertimeDelegation_Table (
    Id int IDENTITY PRIMARY KEY,
    DelegatePersonelId int NOT NULL, TargetPersonelId int NOT NULL, IsActive bit NOT NULL,
    Aciklama nvarchar(max) NULL, Olusturan nvarchar(max) NULL, OlusturmaTarihi datetime2 NULL,
    Degistiren nvarchar(max) NULL, DegistirmeTarihi datetime2 NULL,
    CONSTRAINT FK_OvertimeDelegation_Delegate FOREIGN KEY (DelegatePersonelId) REFERENCES dbo.Personel_Table(Id),
    CONSTRAINT FK_OvertimeDelegation_Target FOREIGN KEY (TargetPersonelId) REFERENCES dbo.Personel_Table(Id),
    CONSTRAINT CK_OvertimeDelegation_OtherPerson CHECK (DelegatePersonelId <> TargetPersonelId)
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OvertimeDelegation_Table') AND name = N'UX_OvertimeDelegation_DelegateTarget')
    CREATE UNIQUE INDEX UX_OvertimeDelegation_DelegateTarget ON dbo.OvertimeDelegation_Table(DelegatePersonelId, TargetPersonelId);

DECLARE @HrParentId int, @HrMenuId int, @DelegationMenuId int;
SELECT TOP (1) @HrMenuId = Id, @HrParentId = ParentId
FROM dbo.Auth_MenuItem_Table WHERE Url = N'/ikys/overtime/hr' ORDER BY Id;
IF @HrMenuId IS NOT NULL
BEGIN
    SELECT TOP (1) @DelegationMenuId = Id FROM dbo.Auth_MenuItem_Table
    WHERE Url = N'/ikys/overtime/delegations' ORDER BY Id;
    IF @DelegationMenuId IS NULL
    BEGIN
        INSERT dbo.Auth_MenuItem_Table(Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
        VALUES(N'Mesai Giriş Yetkileri', @HrParentId, N'/ikys/overtime/delegations', 60, 1, N'Personel adına mesai giriş yetkilerini tanımlama, değiştirme ve kaldırma');
        SET @DelegationMenuId = SCOPE_IDENTITY();
    END;
    INSERT dbo.Auth_RoleMenu_Table(RoleId, MenuId, Aciklama)
    SELECT DISTINCT m.RoleId, @DelegationMenuId, N'Mevcut İK menü erişimi: Mesai Giriş Yetkileri'
    FROM dbo.Auth_RoleMenu_Table m WHERE m.MenuId = @HrMenuId
        AND NOT EXISTS (SELECT 1 FROM dbo.Auth_RoleMenu_Table x WHERE x.RoleId = m.RoleId AND x.MenuId = @DelegationMenuId);
END;

COMMIT;
