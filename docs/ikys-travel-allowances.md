# Harcırah tanımları

PortalApps `IKYS_WebParts/HarcirahGirisWP` ve `HarcirahService` incelenerek Jenga yönetim ekranı oluşturuldu. Mevcut görev onay hesaplamalarıyla aynı `Harcirah_Table` kullanılır.

Adres: `/ikys/travel-allowances`. Eski `/IKYS/HarcirahGirisWP.aspx` ve `/IKYS/HarcirahGiris.aspx` adresleri de desteklenir. Hem sayfa hem servis IKYS/Manage izni gerektirir; yalnızca View/Edit yetkisi mali tanımları yönetmeye yetmez.

## Davranışlar

- Ülkeler mevcut tanımlardan gelir. İlk açılışta Türkiye, yoksa ilk ülke; tarih aralığında seçilen ülkenin en büyük seri numarası gösterilir. Geçmiş seriler ayrıca seçilebilir.
- Tablo kadro grubu, ülke, para birimi, miktar, başlangıç/bitiş ve açıklamayı gösterir. Yalnızca miktar ve açıklama düzenlenir. Tutar sıfır veya pozitif ve en fazla iki ondalık basamak olmalıdır.
- Kaydedilmemiş miktar/açıklama değişiklikleri ülke/seri değişince veya Yenile ile bırakılır. Excel ve arama kayıtlı değerleri esas alır. Excel başlık ve satır alanları açık eşleştirilmiştir; işlem düğmeleri aktarılmaz.
- Başka işlem tarafından değiştirilen miktar/açıklama üzerine eski ekranla yazılamaz. Kaydetme sırasında veritabanında da eşzamanlılık kontrolü yapılır.
- Satır bazında Sil düğmesi ve servis silme metodu kaldırılmıştır. Başlamış ve geçmiş dönemler korunur. Dönem geri alma işlemi aşağıdaki kurallarla yapılır.

## Yeni tarih aralığı

1. Mevcut ülke ve yeni başlangıç tarihi seçilir; onay penceresinde önceki dönemin yeni bitiş tarihi gösterilir.
2. Başlangıç, bu ülkenin mevcut tüm dönem başlangıçlarından sonra olmalıdır. Aynı tarih için tekrar seri açılmaz. Yeni seri numarası ülkenin son seri numarasından bir büyüktür; ekranda eski seri seçili olsa da son seri kapanır.
3. Seçili ülkenin son serisindeki tüm satırlar, yeni başlangıçtan bir gün önce kapatılır. Diğer ülkeler ve daha eski seriler değişmez.
4. Ülkenin geçmişinde tanımlı her kadro grubu için en son grup adı/sırası alınır ve yeni satır açılır. Yeni tutarlar PortalApps davranışı gibi **sıfırdır**; açıklama boş, bitiş tarihi açık (`NULL`) bırakılır. Tutarlar satır bazında Güncelle ile girilir. Yeni dönem kaydedildikten sonra görev hesabına hemen yansır.
5. Türkiye: TL; İngiltere: Sterlin; Avrupa: Euro; Avrupa Harici: USD kullanılır. Diğer mevcut ülke adlarında tutarlı tek para birimi tanımı korunur; belirsiz para birimiyle dönem oluşturulmaz.
6. Önceki seriyi kapatma ve yeni satırların tümünü açma Serializable transaction içindedir. İşlemin ortasında hata olursa kapanış ve eklenen satırlar birlikte geri alınır. Başka kullanıcı yeni seri açmışsa ekranın yenilenmesi istenir.

Güncelleme/oluşturma kullanıcı ve tarih alanları mevcut `ApplicationDbContext` audit mekanizmasıyla doldurulur. Harcırah modeline PortalApps'teki açıklama ve audit sütunları eşlenmiştir; sıfır tutarlı yeni döneme otomatik mali tutar atanmaz.

## Dönemi geri alma

- Seçilen ülkenin yalnızca en büyük seri numaralı, açık bitişli ve başlangıcı bugünden sonra olan dönemi geri alınabilir. Bugün başlayan dönem geri alınamaz.
- Önceki dönem bulunmalı; satırlarının kapanış tarihleri geri alınacak dönem başlangıcından bir gün önce olmalıdır. Kadro grubu/tarih yapısı belirsiz, eksik veya çakışan tanımlarda işlem engellenir.
- Görevler tarife/seri kimliğini saklamadığı için kontrol temkinlidir: başlangıcı veya bitişi dönem başlangıcına/sonrasına uzanan **her görev**, para birimi ve onay/ödeme durumu ayrımı yapılmadan engel kabul edilir. Tarihlerinden biri eksik görevler de geri almayı engeller. Tamamı dönemden önce biten, tarihleri tam görevler engel değildir.
- Ekran uygunluğu ve engelleme nedenini gösterir. “Dönemi Geri Al” onay penceresinde kaldırılacak dönem ve satır sayısı açıklanır. İptal, hiçbir kayıt değiştirmez.
- Servis IKYS/Manage iznini, son dönem/tarih zincirini, görev kayıtlarını ve ekrandaki verilerin halen güncel olduğunu yeniden kontrol eder. Onay penceresi açıldıktan sonra bir görev eklenmesi veya tarife değişmesi geri almayı engeller.
- Başarılı işlemde son dönemin **tüm satırları** kaldırılır; önceki dönemin bitiş tarihleri `NULL` yapılarak yeniden geçerli hale getirilir. Diğer ülkeler ve daha eski dönemler değiştirilmez. Ekran önceki döneme geçer.
- Görev kontrolü, satırların kaldırılması ve önceki dönemin yeniden açılması tek Serializable transaction içindedir. Bir hata olursa bütün değişiklikler geri alınır. Ek veritabanı tablosu veya sütunu gerektirmez.

## Yayına alma ve kontrol

- `docs/sql/2026-11-18-Travel-Allowances-Check.sql` beklenen mevcut tablo sütunlarını salt okunur olarak listeler.
- Yan menü için `docs/sql/2026-11-18-Travel-Allowances-Menu.sql` hedef veritabanında uygulanır. Script yeni modül izni oluşturmaz; mevcut IKYS/Manage rollerine menü görünürlüğü ekler. Eski menü adresi varsa günceller.
- Bu çalışma sırasında canlı veritabanında script çalıştırılmamış ve mali tanımlar değiştirilmemiştir.

Manuel kontrol:

1. Sayfayı yönetim yetkisi olan kullanıcıyla açın. Türkiye/son seri varsayılanını; ülke değiştirince tarih seçenekleri ve satırların yenilenmesini kontrol edin.
2. Miktar/açıklama güncelleyin. Para birimi, grup ve tarihlerin değişmediğini; Excel sütunlarının doğru alanlarla eşleştiğini kontrol edin.
3. Test veritabanında yeni başlangıç seçip onayı iptal edin; sonra onaylayın. Önceki son seri bir gün önce kapanmalı, tüm gruplar sıfır tutarla yeni seride açılmalı. Tutarları güncelleyip görev girişindeki hesabı kontrol edin.
4. Aynı tarih veya eski başlangıçla yeni seri oluşturmayı deneyin; hiçbir tarih değişmemeli. İki sekmede aynı satırı düzenleyip eski sekmeyle kaydetmeyi deneyin; yenileme istenmeli.
5. Görev kaydı olmayan gelecekteki son dönemi seçin. Dönemi Geri Al penceresinde önce iptal, sonra onay akışını deneyin. Önceki dönem yeniden geçerli olmalı ve son dönemin bütün satırları kaldırılmalı.
6. Başlamış dönem, eski seri, görev kaydı bulunan gelecek dönem ve iki sekmede değiştirilmiş tarife ile geri almanın engellendiğini kontrol edin. Yetkisiz kullanıcının sayfa/servis erişimi engellenmeli.

187 servis testi ve 41 ilgili arayüz testi geçti. Servis testleri dönem geçişlerini, para birimlerini, eski ekran kontrolünü, yetkileri, hata halinde geri almayı ve görev hesabıyla entegrasyonu kapsar. Dönem geri alma testlerinde başlangıç günü/geçmiş dönem, son seri kuralı, farklı onay durumları/eksik tarihler, sonradan eklenen görev, eski ekran ve transaction ortasında hata senaryoları doğrulanır. Arayüz testleri filtre, kayıt alanları, Excel, onay/iptal ve tarih doğrulamasını kapsar. Örnek ekran başsız Edge ile 1280 ve 390 piksel genişliklerde incelendi; sayfanın taşmadığı ve girişlerin/düğmelerin hücre içinde kaldığı doğrulandı. Ortak Excel testindeki eski “Periyot” başlık beklentisi, mevcut TBYS ekranındaki “Kontrol Sıklığı” başlığıyla eşleştirilmiştir.
