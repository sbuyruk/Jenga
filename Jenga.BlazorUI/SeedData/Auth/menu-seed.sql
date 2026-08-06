SET NOCOUNT ON;

IF OBJECT_ID(N'Auth_MenuItem_Table', N'U') IS NULL
BEGIN
    RAISERROR('Tablo bulunamadı: Auth_MenuItem_Table', 16, 1);
    RETURN;
END;

SET IDENTITY_INSERT Auth_MenuItem_Table ON;

MERGE INTO Auth_MenuItem_Table AS target
USING (
    VALUES
        (1, N'Yetkilendirme', NULL, N'#', 1, 1, N'Yetkilendirme ana menüsü'),
        (2, N'Menü Yönetimi', 1, N'/common/menu-item-page', 2, 1, N'Menü yönetimi'),
        (3, N'Rol Yönetimi', 1, N'/common/role-page', 3, 1, N'Rol yönetimi'),
        (4, N'Tek ekranda Rol Yönetimi', 1, N'/common/role-management', 4, 1, N'Tek ekranda rol yönetimi'),
        (5, N'Rol Modüle Yetkilendirme', 1, N'/common/role-module-permissions', 5, 1, N'Rol modül yetkilendirme'),
        (6, N'Online Users', 1, N'/admin/online', 6, 1, N'Online kullanıcılar'),
        (7, N'Kullanıcı Erişimleri', 1, N'/admin/navigation-summary', 7, 1, N'Kullanıcı erişimleri'),
        (8, N'Kullanıcı Değiştir', 1, N'/admin/switch-user', 8, 1, N'Kullanıcı değiştir')
) AS source(Id, Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET
        Title = source.Title,
        ParentId = source.ParentId,
        Url = source.Url,
        DisplayOrder = source.DisplayOrder,
        IsVisible = source.IsVisible,
        Aciklama = source.Aciklama
WHEN NOT MATCHED THEN
    INSERT (Id, Title, ParentId, Url, DisplayOrder, IsVisible, Aciklama)
    VALUES (source.Id, source.Title, source.ParentId, source.Url, source.DisplayOrder, source.IsVisible, source.Aciklama);

SET IDENTITY_INSERT Auth_MenuItem_Table OFF;