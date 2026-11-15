# Görev onay raporu

`/ikys/task-approvals/report` raporu, kullanıcı tarafından paylaşılan PortalApps görev onay raporu ekran görüntüsüne göre güncellenmiştir. PortalApps reposunda görüntüleyici/rapor şablonu bulunmadığından görsel düzen bu örneği esas alır. Brüt hesap formülü kullanıcı tarafından doğrulanmıştır.

## İçerik ve hesap

Tek görev kaydı için kullanıcı tarafından paylaşılan ikinci görseldeki kişisel form düzeni kullanılır: personel tablosu (adı, avans, yevmiye, para birimi ve görev), görev süresi/tarihleri, görev sebebi, gidilecek yer, araç talebi/plakası, başlangıç tarihi ve diğer hususlar. Araç talebi `AracTahsisi` alanından Var/Yok olarak gösterilir; boşsa “—” kullanılır. Diğer hususlar `Aciklama` alanıdır. Bu formda yalnızca uzman ve personel direktörü imzaları bulunur. Birden fazla görev kaydında aşağıdaki toplu tablo düzeni kullanılır.

- Başlık: TSKGV GÖREV ONAYI; kurum amblemi ve raporun hazırlanma tarihi.
- Sütunlar: Adı Soyadı, Görev, Görevin Sebebi, Net Yevmiye, Brüt Yevmiye, İş Avansı, Gidiş Tarihi / Saati, Dönüş Tarihi / Saati, Süre, Günlük Yevmiye, Görevin Yeri, Ulaşım Aracı, Araç Plakası.
- Net tutar kayıttaki `Yevmiye` değeridir; harcırah yeniden hesaplanmaz. Brüt tutar `net × 1,00759`, satır başına iki ondalığa yuvarlanarak hesaplanır. Toplamlar yuvarlanmış satır değerlerinin toplamıdır.
- Tutarlar Türkçe sayı biçiminde okunur. Boş, bozuk veya negatif net tutar ve eksik para birimi raporu/yazdırmayı engeller; bu kayıtlar sıfır kabul edilmez. Sıfır yevmiye geçerlidir.
- Her personel için para birimi bazında ara toplam ve belge genel toplamı gösterilir. TL/TRY/₺ aynı gruptadır; dövizler birbirine eklenmez ve kur dönüşümü yapılmaz.
- Görev adı iş bilgilerindeki görev tanımının kısaltmasından, yoksa görev/unvan adından alınır. İş avansı `Avans`, araç plakası `AracPlakasi` alanıdır. Boş günlük yevmiye, çoklu seri kaydında olduğu gibi boş kalır.

## İmzalar ve yetki

İmza bölümü “GENEL MÜDÜRLÜK MAKAMINA” başlığı ve seyahat müsaadesi talebini içerir. Tekli formda `OnayImza` ve `PerSubeImza` kullanılır. Toplu tabloda sağa aktif Muhasebe ve Finans Direktörlüğünün `AmirId` personeli eklenir. İlk iki imzacının görevleri personel kayıtlarından alınır; `PerSubeVekil` işaretliyse personel direktörünün görevi sonuna “V.” eklenir. Üçüncü imzada “Muh. ve Fin. Dir.” unvanı gösterilir; `OnayMakam` bu imza için kullanılmaz. Finans direktörü tek bir aktif birim amiri/çalışan personel olarak belirlenemezse toplu raporda isim boş bırakılır ve ekranda tanım uyarısı gösterilir. Farklı imzacı kümeleri ayrı belgelerde hazırlanır.

Rapor IKYS/Manage yetkisi gerektirir; `Auth` parametresi tek başına yetki sağlamaz. Ödenmiş, bekleyen, reddedilmiş veya durumu belirsiz kayıt rapora alınmaz. İstenen kayıtlardan biri uygun değilse kısmi rapor üretilmez.

`taskApprovalId`, eski `GorevOnayId` ve virgülle ayrılmış `ids` sorguları desteklenir. `/IKYS/GorevOnayBelgesiViewer.aspx` ve `/IKYS/GorevOnayBelgesiTopluViewer.aspx` adresleri de aynı sayfaya yönlenir; toplu rapor için seçilen kimlikler `ids` ile gönderilir. Veritabanındaki ortak `Secildi` işaretleri kullanılmaz.

## Yazdırma ve kontrol

Yazdır düğmesi raporu stilleri ve görselleriyle ayrı bir iframe içinde açar. Ana sayfanın DOM'u değiştirilmez ve sayfa yenilenmez. Tekli form, yazdırma penceresinde seçilen kâğıdın yazdırılabilir genişliğini doldurur; dikey yön ve 10 mm kenar boşluğu kullanır. Sabit A4 boyutu veya mm cinsinden form genişliği zorlanmaz; A5 seçimi ek küçültmeye neden olmaz. Toplu tablo A4 yatay ve 10 mm kenar boşluğuyla hazırlanır. Uzun tabloda tekrar eden sütun başlıkları ve bölünmemesi tercih edilen personel/imza blokları kullanılır. Dar ekranda tekli form ekran genişliğine uyar; toplu tablonun yatay kaydırması rapor alanıyla sınırlıdır.

1. Genel listeden tekli rapor açın; görev, avans, plaka ve imzacıları kayıtla karşılaştırın.
2. Birden fazla personeli seçip toplu rapor açın. Aynı personelin kayıtları tek isim altında ve ara toplamla görünmeli.
3. Örnek tutarlar `2.025,00 + 1.635,00 + 2.025,00 TL` için net `5.685,00`, brüt `5.728,15 TL` olmalı.
4. Birden fazla para biriminde ayrı toplamları; farklı imzacılarda ayrı belgeleri kontrol edin.
5. Yazdır / PDF olarak kaydet önizlemesinde yatay A4 düzenini, uzun raporlarda başlıkları ve imza alanlarını kontrol edin. İptal sonrası sayfa çalışmaya devam etmeli.

Otomatik doğrulama: 158 servis testi ve 35 ilgili arayüz testi geçti. Tekli ve toplu örnek raporlar başsız Edge ile ekran/yazdırma stillerinde görsel olarak incelendi; masaüstü ve dar ekran genişlikleri ile yazdırma sonrasında ana sayfanın korunması doğrulandı. Canlı veritabanında rapor açma ve fiziksel yazıcı çıktısı kullanıcı ortamında doğrulanmalıdır.
