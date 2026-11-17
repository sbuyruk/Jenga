# Canlı ortamda geçici kullanıcı değiştirme

`/admin/switch-user` gerçek oturum sahibinin **Admin / Manage** izniyle kullanılabilir. Hedef kullanıcının izinleriyle test yapılır. Yönetici olmayan bir hesaba geçildiğinde de aynı adresten gerçek kullanıcıya dönülebilir.

Development dışında özellik varsayılan olarak kapalıdır. Canlı sunucuda aşağıdaki ortam değişkenlerini tanımlayın veya aynı anahtarları dağıtımın yapılandırmasına ekleyin:

```text
Auth__Impersonation__Enabled=true
Auth__Impersonation__ExpiresAtUtc=2026-11-24T20:59:00Z
```

Bitiş tarihi bir örnektir: 24 Kasım 2026, Türkiye saatiyle 23:59. Test için istediğiniz gelecekteki tarihi UTC biçiminde (`yyyy-MM-ddTHH:mm:ssZ`) girin. Eksik, hatalı veya geçmiş tarih erişimi açmaz. IIS ortam değişkeni değişikliklerinden sonra uygulama havuzunu yeniden başlatın.

Sayfaya gerçek yönetici hesabınızla girin, mevcut bir personelin kullanıcı adını yazın ve **Değiştir** düğmesini kullanın. Geri dönüş için doğrudan `/admin/switch-user` adresine gidip **Sıfırla (Gerçek Kullanıcı)** düğmesine basın; hedef hesabın yönetici menüsüne erişmesi gerekmez.

Test bitince `Auth__Impersonation__Enabled=false` yapın ve uygulamayı yeniden başlatın. Son tarih geçtiğinde mevcut oturumlar da sonraki kullanıcı/yetki çözümlemesinde gerçek hesaba döner. Tarayıcıda kalan kayıt, kapalı veya süresi dolmuş erişimde uygulanmaz.

Başlangıç ve bitiş, gerçek hesap ve hedef hesap bilgisiyle uygulama günlüğüne yazılır. İşlemler hedef kullanıcının izinleriyle gerçekleşir; mevcut denetim alanları hedef kullanıcıyı kaydeder.
