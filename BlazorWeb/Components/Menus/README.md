# Menus Bileşenleri

Bu klasör, Nexor projesinde menü sistemini oluşturan bileşenleri barındırır. Amaç, yeniden kullanılabilir, hiyerarşik ve kullanıcı rolüne göre filtrelenebilir bir menü altyapısı sunmaktır.

## İçerik

- **RecursiveMenu.razor**  
  Hiyerarşik yapıdaki menü öğelerini özyinelemeli olarak render eder. Aktif durum yönetimi, hover efektleri ve geçiş animasyonlarıyla modern bir kullanıcı deneyimi hedeflenmiştir.

- **RecursiveMenu.razor.css**  
  `RecursiveMenu` bileşeni için scoped CSS içerir. Menü öğeleri arasındaki hiyerarşi ve etkileşimli tasarım bu dosya ile şekillendirilmiştir.

- **TreeViewSidebar.razor**  
  `RecursiveMenu` bileşenini yan menü şeklinde sunar. Tooltip desteği, daraltılabilir yapı ve kullanıcı dostu gezinme sağlar.

- **TreeViewSidebar.razor.css**  
  `TreeViewSidebar` bileşeni için scoped stilleri barındırır. Estetik geçişler ve konumlandırma düzenlemeleri içerir.

- **Sidebar.razor**  
  TreeView veya diğer menü stillerini dış dünyaya açan ana bileşendir. Uygulama geneline entegrasyonu bu bileşen sağlar.

## Kullanım

Bu bileşenler role-based filtreleme desteğiyle birlikte çalışır. Menü öğeleri, kullanıcının yetkilerine göre dinamik olarak filtrelenebilir.

## Geliştirici Notları

- Scoped CSS ile stil çatışmalarının önüne geçildi.
- EF Core ile entegre veri kaynakları kullanılarak dinamik menü oluşturma desteği planlanmaktadır.
- Bileşenler Blazor Web App mimarisiyle uyumlu hale getirilmiştir.

---

Her bileşen, bağımsız olarak test edilebilir ve geliştirme sürecinde kolayca genişletilebilir. Geri bildirimlerinizi ve katkılarınızı bekliyoruz!
