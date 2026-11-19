# İKYS izin talepleri

Personel, kişisel sayfasındaki **İzin Taleplerim** kartından taleplerini açar. İKYS yetkilileri **Bekleyen İşlemler** sayfasındaki **İzin Talepleri** bağlantısından çalışma durumu **Çalışıyor** olan personelin açık ve sonuçlanmış taleplerini listeleyebilir. Kişisel ekran, kullanıcının kendi kayıtlarını getirir. Kişisel işlemlerde ilgili `IKYS/View`, `Create`, `Edit` ve `Delete` izinleri, İKYS işlem ekranında `IKYS/Manage` denetlenir.

İzin giriş formunda personel seçimi `IKYS/Manage` yetkisiyle açılır. Yetkili kullanıcı çalışan kadrolu veya kadro harici personel adına, diğer kullanıcılar kendi adlarına izin talebi oluşturabilir. **İzin Taleplerim → Personel Adına Talep** üzerinden seçilen personel giriş formunda hazır gelir.

Formun başında, seçilen personelin seçili izin türündeki işlemi devam eden talepleri görünür. Süre hesabında bu taleplerin süresi ayrı gösterilir. Yıllık izin hesabında bu süre toplam bakiyeden, mazeret izninde ilgili dönem bakiyesinden düşülerek tahmini kalan süre hesaplanır.

- **İşlem Bekliyor** veya **Dilekçe Bekleniyor** durumundaki mevcut talep düzenlenebilir, silinebilir, kontrol edilebilir veya reddedilebilir.
- **Kontrol Edildi** durumunda personel **Belge ve Detay → İzin Belgesini Yazdır** adımından A5 çıktıyı alır ve imzalı belgeyi Personel Kısmına teslim eder. Saatlik mazeret izni ayrı şablonla yazdırılır. Süt izni için belge düzenlenmez ve İKYS nihai onayı belge teslimi olmadan verir. **Kayıtlara İşlendi** durumunda aynı ekrandan yeniden çıktı alınabilir. Mazeret izninin yeniden çıktısı için talebe bağlı izin hareketindeki bakiye bilgisi kullanılır.
- Nihai onay aynı işlemde izin hareketini oluşturur. Yıllık ve mazeret izinlerinde bakiye, asker statüsü dışındaki personelin ücretsiz izninde ise izin dönemi başlangıcı güncellenir. Yeni mahsup dilekçesi ve mahsup yönetimi sonraki etabın kapsamındadır.

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
