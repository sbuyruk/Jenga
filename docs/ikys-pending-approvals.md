# Bekleyen İşlemler

`/ikys/pending-approvals`, PortalApps içindeki `IKYS_WebParts/BekleyenIslemlerWP` davranışını izler.

- Normal görünümde, amir olunan aktif birimlerin görevleri onay tablosunda gösterilir. Önce bekleyen görevler, ardından diğer durumlar ve en yeni görev tarihleri gelir.
- Amirlikten bağımsız olarak kullanıcının kendi onay bekleyen görevleri ayrıca listelenir. Son beş onaylanmış görev açılabilir bir bölümde gösterilir.
- Son beş kayıt, onay zamanına değil başlangıç tarihi, bitiş tarihi ve kayıt kimliğinin azalan sırasına göre seçilir. Bu listelerde tarih aralığı filtresi yoktur.
- `?Auth=IKYS`, IKYS/Manage izni olan kullanıcıya tüm personeli kapsayan görünümü açar. Bu kapsam iş bilgisi kaydı bulunmayan personelin görevlerini de içerir. Yetkisiz kullanıcıda sorgu parametresi genel erişim sağlamaz.
- Jenga'nın mevcut yetki kuralı korunur: sayfa için IKYS/View, onay ve ret için IKYS/Manage gerekir. Normal görünümde karar işlemleri ayrıca amir olunan birimlerle sınırlıdır.
- Ret gerekçesi zorunludur. Onay ve ret sonrasında üç liste yeniden yüklenir. Kararı verilmiş görev için yeniden karar kaydedilemez.
- Excel aktarımı, başlıkları ve değerleri aynı sütun sırasıyla üretir. Kimlikler ve işlem düğmeleri aktarılmaz; onay tablosunda durum ile ret gerekçesi ayrı sütunlarda yer alır. Kişisel listelerde tarihlerin saat bilgisi de korunur.

## Doğrulama

`Jenga.Overtime.Tests/PendingApprovalsTests.cs`, SQLite üzerinde kişisel kapsamı, amir kapsamını, genel IKYS erişimini, son beş kayıt sınırını, sıralamayı, yetki reddini ve kararın veritabanına yazılmasını kontrol eder. Bildirimler test e-posta servisine gönderilir.

Test ortamında aşağıdaki senaryoları kontrol edin:

1. Amir olmayan personelde kendi bekleyen görevleri ve son beş onaylanmış görevi görünmeli; karar düğmeleri bulunmamalı.
2. Amir görünümünde ilgili birimin görevleri gelmeli; bekleyen kayıtlar önde, ret gerekçesi satırda görünmeli.
3. IKYS/Manage yetkisiyle `?Auth=IKYS` görünümünde farklı birimlerin görevleri de görünmeli. Yetkisiz kullanıcı aynı adresle genel listeye erişememeli.
4. Onay ve gerekçeli ret sonrası kayıt durumu değişmeli; kişisel listeler ve son beş görev yeniden hesaplanmalı.
5. Açıklama ve görev metinleri HTML olarak çalıştırılmamalı; tarih ve saatler, arama ve Excel dışa aktarımı kontrol edilmeli.
