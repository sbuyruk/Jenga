# İKYS izin talepleri

Personel, kişisel sayfasındaki **İzin Taleplerim** kartından taleplerini açar. İKYS yetkilileri **Bekleyen İşlemler** sayfasındaki **İzin Talepleri** bağlantısından çalışma durumu **Çalışıyor** olan personelin açık ve sonuçlanmış taleplerini listeleyebilir. Kişisel ekran, kullanıcının kendi kayıtlarını getirir. Kişisel işlemlerde ilgili `IKYS/View`, `Create`, `Edit` ve `Delete` izinleri, İKYS işlem ekranında `IKYS/Manage` denetlenir.

Yönetim listesi **Genel İzinler** ile açılır. **Mazeret İzinleri** seçeneği saatlik mazeret taleplerini, **Tümü** ise her iki grubu gösterir. **Filtreleri Temizle** yönetim listesini **Genel İzinler** seçimine döndürür. Çalışma durumu, yönetim ekranında talep kimliğiyle detay açılırken ve karar kaydedilirken de kontrol edilir.

İzin giriş formunda personel seçimi `IKYS/Manage` yetkisiyle açılır. Yetkili kullanıcı çalışan kadrolu veya kadro harici personel adına, diğer kullanıcılar kendi adlarına izin talebi oluşturabilir. **İzin Taleplerim → Personel Adına Talep** üzerinden seçilen personel giriş formunda hazır gelir.

Formun başında, seçilen personelin seçili izin türündeki işlemi devam eden talepleri görünür. Süre hesabında bu taleplerin süresi ayrı gösterilir. Yıllık izin hesabında bu süre toplam bakiyeden, mazeret izninde ilgili dönem bakiyesinden düşülerek tahmini kalan süre hesaplanır.

- **İşlem Bekliyor** veya **Dilekçe Bekleniyor** durumundaki mevcut talep düzenlenebilir, silinebilir, kontrol edilebilir veya reddedilebilir.
- **Kontrol Edildi** durumunda personel **Belge ve Detay → İzin Belgesini Yazdır** adımından A5 çıktıyı alır ve imzalı belgeyi Personel Kısmına teslim eder. Saatlik mazeret izni ayrı şablonla yazdırılır. Süt izni için belge düzenlenmez ve İKYS nihai onayı belge teslimi olmadan verir. **Kayıtlara İşlendi** durumunda aynı ekrandan yeniden çıktı alınabilir. Mazeret izninin yeniden çıktısı için talebe bağlı izin hareketindeki bakiye bilgisi kullanılır.
- Nihai onay aynı işlemde izin hareketini oluşturur. Yıllık ve mazeret izinlerinde bakiye, asker statüsü dışındaki personelin ücretsiz izninde ise izin dönemi başlangıcı güncellenir.

## Gelecek dönemden yıllık izin mahsubu

İznin ait olduğu dönem ile geçmiş dönemlerden kalan bakiye talebi karşılamıyorsa, **Süreyi Hesapla** adımından sonra **Mahsup Dilekçesi Oluştur** seçilebilir. Talep **Dilekçe Bekleniyor** durumunda açılır. Talep sahibi **Mahsup Dilekçesini Yazdır** ile A5 çıktıyı alıp imzalatarak Personel Kısmına teslim eder.

İKYS talebi **Kontrol Edildi** durumuna aldığında personel **İzin Belgesini Yazdır** adımından standart izin belgesini alır. İKYS nihai onayda imzalı izin belgesinin teslimini doğrular ve eksik günleri karşılayan bir gelecek dönem seçer. Örneğin 1 gün mevcut bakiyeyle 3 gün izin istenirse dilekçede 2 gün yazılır. Onayda 1 gün mevcut bakiyeden, 2 gün seçilen gelecek dönemden düşülür. İki bakiye de kullanıldığında talebe bağlı iki hareket oluşturulur ve mahsup kaydı yalnızca gelecek döneme ait harekete bağlanır. İlk gelecek dönem henüz açılmamışsa ve hak edişi yeterliyse dönem onay sırasında oluşturulabilir.

Bu sürümü yayımlamadan önce aynı İKYS veritabanında [yıllık izin mahsup SQL betiğini](sql/2026-11-24-IKYS-YillikIzinMahsup.sql) çalıştırın. Betik mevcut `Mahsup_Table` tablosunu kullanır ve yıllık izin türündeki açık **Dilekçe Bekleniyor** kayıtlarını işaretler. PortalApps’te daha önce **Kontrol Edildi** durumuna geçirilmiş mahsup kayıtlarını Personel Kısmıyla doğrulayın. Eski sistem bu durumda dilekçe bilgisi için ayrı bir alan tutmadığından bu kayıtlar otomatik olarak ayırt edilemez.

Ardından [mahsup gün sayısı betiğini](sql/2026-11-24-IKYS-YillikIzinMahsupGun.sql) uygulayın. Önceki sürümde oluşturulmuş **Dilekçe Bekleniyor** durumundaki talepler için dilekçe çıktısını yeniden alın.

## Yapılandırma

| Ayar | Kullanım |
|---|---|
| `LeaveRequests:PortalDocumentBaseUrl` | İsteğe bağlı eski belge adresi. Jenga ekranı kendi A5 belgelerini açar. Jenga kök adresi tanımsızsa e-postadaki belge bağlantısı için PortalApps adresi kullanılabilir. |
| `LeaveRequests:BaseUrl` | E-postalarda kullanılacak Jenga kök adresi. |
| `LeaveRequests:NotificationGroup` | Yeni talep alıcıları. Varsayılan değer PortalApps’teki `izinonaymailgrubu@tskgv.local`. Boş değer grup bildirimini kapatır. Birden fazla adres noktalı virgülle ayrılır. |
| `LeaveRequests:PersonnelEmailDomain` | Personelin iletişim kaydında e-posta bulunmadığında kullanılan alan adı. Varsayılan değer `tskgv.local`. |

Bildirimler mevcut `Email:SmtpHost` ve `Email:From` ayarları ile kayıt tamamlandıktan sonra gönderilir. Kişisel girişte bildirim açıktır. İKYS yetkilisi personel adına talep girerken bildirimi formdan seçebilir.

## Test ortamında geçiş kontrolü

1. İki uygulamanın aynı izin tablolarını kullandığını, eski talep kimlikleri ve durumlarının eşleştiğini, dönem gerektirmeyen yeni taleplerde `IzinTalep_Table.IzinDonemId` ve hareket karşılığının boş değer kabul ettiğini doğrulayın.
2. Mevcut bir bekleyen talebi Jenga’dan kontrol edin. Belge ekranını talep sahibiyle ve erişimi olmayan bir test hesabıyla açmayı deneyin. Ücretli ve mazeret izin belgelerinde tarih, imzacı, süre ve kalan izin değerlerini karşılaştırın.
3. Test SMTP ayarlarıyla talep, kontrol, ret ve nihai onay bildirimlerini doğrulayın. İki oturumdan aynı talebi kayıtlara işleyerek tek izin hareketi ve tek bakiye değişimi oluştuğunu kontrol edin.
4. Geçişte izin taleplerine ve onay kayıtlarına yalnızca Jenga’dan yazın. PortalApps belge ekranı kullanılıyorsa belge erişimini koruyun.
