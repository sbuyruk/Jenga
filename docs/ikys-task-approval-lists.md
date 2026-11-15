# Genel ve kişisel görev onay listeleri

PortalApps `GorevOnayListesiWP` ve `GorevOnayListesiKisiselWP` esas alınmıştır.

| Davranış | Genel liste | Kişisel liste |
|---|---|---|
| Adres | `/ikys/task-approvals` | `/ikys/task-approvals/personal` |
| Yetki | IKYS/Manage | IKYS/View; düzenleme için ayrıca Edit |
| Kayıt kapsamı | Tüm birimler; bitiş tarihi son bir yıl içinde olan ve gelecek görevler | Sadece oturumdaki personelin tüm geçmişi |
| Sıralama | Başlangıç tarihi azalan; aynı tarihte bekleyenler önce | Bitiş tarihi azalan |
| Rapor ve toplu seçim | Ödenmemiş, onaylanmış veya onay gerekmeyen görevler | Yok |
| Ödendi Yap | Seçilen uygun görevler için onay penceresiyle | Yok |
| Düzenleme | Ödenmemiş; bekleyen, onaylanmış veya onay gerekmeyen görevler | Ödeme değeri açıkça false; bekleyen veya onay gerekmeyen; bitişinden itibaren en fazla 14 gün |

Kişisel düzenleme süresi başlangıca göre değil bitiş tarihine göre hesaplanır. Gelecekte bitecek görevler de bu koşulu sağlar. Reddedilmiş, onaylanmış, ödeme bilgisi belirsiz veya süresi geçmiş görevler kişisel listeden düzenlenemez. Genel liste, Jenga'nın İKYS yönetim izniyle sınırlandırılır; `Auth=IKYS` tek başına yetki sağlamaz.

Doğrudan giriş/düzenleme adreslerinde de aynı servis kontrolleri uygulanır. Düzenleme, silme, rapor ve ödeme istekleri güncel veritabanı durumuyla doğrulanır. Kişisel güncelleme ödeme durumunu değiştiremez ve onayı bekleme durumuna alır. Toplu ödeme seçimin tamamı için tek transaction ile kaydedilir; uygun olmayan bir kayıt varsa hiçbiri değiştirilmez.

Seçimler kullanıcının sayfasında tutulur; başka kullanıcıların `Secildi` alanları sıfırlanmaz. Tablo başlığındaki seçim kutusu yalnızca arama ve sıralama sonrasında görüntülenen sayfadaki uygun görevleri seçer. Excel aktarımı açık başlık/satır eşleştirmesini korur.

## Menü ve yayına alma

Kişisel ana sayfadaki “Tümünü gör” bağlantısı ve iki liste arasındaki bağlantılar hazırdır. Yan menüye de eklemek için `docs/sql/2026-11-15-TaskApproval-Personal-Menu.sql` hedef veritabanında çalıştırılır. Script yeni işlem izni vermez; mevcut IKYS/View veya Manage rollerine kişisel menü görünürlüğü ekler. Bu çalışma sırasında veritabanında uygulanmamıştır.

## Kontrol adımları

1. Normal personelle kişisel listeyi açın; yalnızca kendi kayıtları ve bir yıldan eski geçmişi görünmeli.
2. Bitişi 14 günün içinde olan bekleyen/onay gerekmeyen, ödenmemiş kaydı düzenleyin. Kaydetme sonrası kişisel listeye dönmeli. Onaylanmış, ödenmiş, reddedilmiş veya eski kayda doğrudan düzenleme adresiyle de erişilememeli.
3. IKYS/Manage ile genel listede başka birimlerin görevlerini görün. Bekleyen, reddedilmiş ve ödenmiş satırlarda seçim/rapor düğmesi bulunmamalı.
4. Arama ve sayfalama yapıp başlık seçim kutusunun sadece mevcut sayfadaki uygun kayıtları seçtiğini kontrol edin. Tekli ve toplu rapor alın.
5. Ödendi Yap penceresinde iptal işlemini ve onaylama sonrasında satırın ödenmiş hale gelip seçim/rapor/düzenleme düğmelerinin kapanmasını kontrol edin.
6. Her iki listeden Excel aktarımında başlıkların doğru değerlerle eşleştiğini kontrol edin.
