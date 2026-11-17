# Canlı ortamda geçici kullanıcı değiştirme

`/admin/switch-user` gerçek oturum sahibinin **Admin / Manage** izniyle kullanılabilir. Hedef kullanıcının izinleriyle test yapılır. Yönetici olmayan bir hesaba geçildiğinde de aynı adresten gerçek kullanıcıya dönülebilir.

Projedeki `appsettings.json`, geçici test için 24 Kasım 2026 Türkiye saatiyle 23:59'a kadar etkinleştirilmiştir. Bu süre sonunda erişim otomatik kapanır. Canlı sunucuda ortam değişkenleri veya `appsettings.Production.json` aynı anahtarları tanımlıyorsa proje ayarını geçersiz kılabilir. Gerekirse bu değerleri dağıtım yapılandırmasına uygulayın:

```text
Auth__Impersonation__Enabled=true
Auth__Impersonation__ExpiresAtUtc=2026-11-24T20:59:00Z
```

Bitiş tarihi UTC biçimindedir (`yyyy-MM-ddTHH:mm:ssZ`). Eksik, hatalı veya geçmiş tarih erişimi açmaz. IIS ortam değişkeni değişikliklerinden sonra uygulama havuzunu yeniden başlatın. Sayfa, erişimin kapalı olması, geçersiz bitiş tarihi, süre dolması ve gerçek hesabın yönetici izninin eksik olması durumlarını ayrı mesajlarla gösterir.

Sayfaya gerçek yönetici hesabınızla girin, mevcut bir personelin kullanıcı adını yazın ve **Değiştir** düğmesini kullanın. Geri dönüş için doğrudan `/admin/switch-user` adresine gidip **Sıfırla (Gerçek Kullanıcı)** düğmesine basın; hedef hesabın yönetici menüsüne erişmesi gerekmez.

Test bitince `Auth__Impersonation__Enabled=false` yapın ve uygulamayı yeniden başlatın. Son tarih geçtiğinde mevcut oturumlar da sonraki kullanıcı/yetki çözümlemesinde gerçek hesaba döner. Tarayıcıda kalan kayıt, kapalı veya süresi dolmuş erişimde uygulanmaz.

Başlangıç ve bitiş, gerçek hesap ve hedef hesap bilgisiyle uygulama günlüğüne yazılır. İşlemler hedef kullanıcının izinleriyle gerçekleşir; mevcut denetim alanları hedef kullanıcıyı kaydeder.
