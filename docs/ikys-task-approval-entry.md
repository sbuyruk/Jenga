# Görev onay girişi

PortalApps `IKYS_WebParts/GorevOnayGirisiWP` incelenerek Jenga `/ikys/task-approval-entry` sayfası güncellenmiştir. Eski `.aspx` adresi ve mevcut yeni/düzenle adresleri çalışmaya devam eder.

## Hesaplama ve kayıt

- Harcırah grubu aktif `GorevTanim_Table.HarcirahGrupId` alanından; tanımlar `Harcirah_Table` üzerinden alınır. Doğrudan personel görev tanımı yoksa iş bilgilerindeki görev tanımı kullanılır. Sabit veya varsayımsal yevmiye tutarı kullanılmaz.
- Ülkeler Türkiye, Avrupa, İngiltere ve Avrupa Harici; varsayılan saatler 08:00–17:00; saat seçenekleri beşer dakikadır. Tarih aralığı en fazla 40 gündür. Bitiş başlangıçtan önceyse veya aralık 40 günü aşarsa arayüz bitiş tarihini başlangıca çeker; servis de geçersiz aralığı reddeder.
- Tam günlerin ardından kalan süre yukarı tam saate yuvarlanır. Kalan süre 12 saate kadar yarım, 12 saati aşarsa tam yevmiyedir. Seri geçişleri gerçek başlangıç/bitiş saatleriyle ayrı hesaplanır; ikiden fazla seri desteklenir. Başlangıç/bitiş günleri tanım kapsamına dahildir.
- Bitiş tarihi boş tarife, daha yeni bir tarife başlayana kadar geçerlidir. Eksik harcırah grubu/tanımı, tarih kapsamındaki boşluk veya farklı para birimleri kaydetmeyi engeller. Harcırah kapalıysa tutar sıfır, süre ve günlük tanım korunur.
- Grup 1 ve 2 için onay gerekmez; diğer gruplar amir onayına düşer. Kaydetme ve güncelleme sırasında hesap sunucuda yeniden yapılır; ekrandan gelen tutar, onay ve ödeme değerlerine güvenilmez.
- Çakışan görevler engellenir; reddedilmiş görevler çakışmaya dahil edilmez. Bitişi yeni başlangıca eşit görev çakışma sayılmaz.
- Yeni kayıt ve silme onay penceresiyle gerçekleşir. Ödenmiş kayıtlar değiştirilemez. Güncelleme onayı grubun gerektirdiği duruma döndürür.

## Kapsam ve imzacılar

Kişisel görünüm kendi personeline, `Auth=BIRIM` amirin yönettiği aktif birimler ve alt birimlerine, `Auth=IKYS` ise IKYS/Manage izniyle çalışan kadrolu personele erişir. Sorgu parametresiyle kapsam genişletilemez. Oluşturma/düzenleme/silme işlem izinleri ayrıca denetlenir. Kişisel düzenlemede genel ve kişisel listeyle aynı ödeme, onay ve son 14 gün kuralları uygulanır.

İmzacılar personel birimindeki çalışan kadrolu personelin `IsBilgileri_Table.GorevId` ile bağlı görev adlarından seçilir. Per.Dir.: Direktör/Başuzman/Kıdemli Uzman; Per.Uzm.: Uzman/Kıdemli Uzman. Başka personel veya pasif çalışan imzacı olarak kaydedilemez.

## Yapılandırma ve bildirim

İsteğe bağlı `TaskApproval` ayarları:

| Anahtar | Varsayılan | Kullanım |
|---|---|---|
| `PersonnelUnitId` | `5` | İmza seçeneklerinin personel birimi |
| `DefaultSpecialistId` | `124` | Geçerli imzacı listesindeyse yeni kayıtta seçilir |
| `ManagerApprovalEnabled` | `true` | `false` olduğunda onay gerekmez ve bildirim gönderilmez |
| `NotificationGroup` | `gorevonaymailgrubu@tskgv.org.tr` | Noktalı virgülle ayrılan ek alıcılar; boş verilerek kapatılabilir |
| `BaseUrl` | boş | E-posta bağlantıları için Jenga'nın gerçek HTTP/HTTPS kök adresi |

Mevcut `Email:SmtpHost` ve `Email:From` ayarları kullanılır. Yeni kayıtta personele ve amir/grup alıcılarına ayrı bildirim gönderilir. IKYS görünümünde ek bildirim grubu kullanılmaz. Personel bağlantısı kişisel listeye; onay bekleyen görevlerde amir bağlantısı bekleyen işlemlere gider. Güncellemede yeni kayıt bildirimi gönderilmez.

E-posta gönderilemese de kayıt korunur ve uyarı gösterilir. Ekran oluşan kayıt kimliğini saklar; Kaydet'e yeniden basılarak ikinci kayıt oluşturulmaz. Canlı SMTP gönderimi bu geliştirme sırasında denenmemiştir.

## Veritabanı ve kontrol

`docs/sql/2026-11-15-TaskApproval-Entry-Check.sql` salt okunur şema kontrolüdür. PortalApps ile ortak tanımların bulunduğu veritabanı beklenir; bu çalışmada canlı veritabanına script uygulanmamış ve harcırah tutarı eklenmemiştir.

1. Kişisel girişte yalnızca kendi personelinizin seçildiğini kontrol edin. Aynı gün 08:00–17:00 için yarım, 08:00–20:01 için tam yevmiye görünmeli.
2. Ülkeyi ve tarihleri değiştirin; günlük tutarı veritabanı tanımıyla karşılaştırın. Seri sınırını geçen görevde hesap açıklamasında dönemler ayrı görünmeli.
3. Harcırahı kapatıp sıfır tutarı, Kaydet penceresinde iptal/onay akışını ve kişisel listeye dönüşü kontrol edin.
4. Birim amiriyle alt birim personeli; IKYS/Manage ile tüm çalışan kadrolu personel ve uygun imzacıları kontrol edin.
5. Çakışan görev, eksik tanım, ödenmiş kayıt ve yetkisiz doğrudan düzenleme adreslerini deneyin.
6. Test SMTP ortamında bildirimleri ve bağlantıları; SMTP kapalıyken kaydın korunup uyarı gösterilmesini kontrol edin.
