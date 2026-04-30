# Tech Debt — Jenga

Bu dosya, kod tabanında bilinçli olarak ertelenmiş yapısal/kavramsal sorunları kayıt altına alır.
Her madde: **Sorun**, **Etki**, **Önerilen çözüm**, **Önkoşul / Risk**, **Durum** alanlarını içerir.

---

## 1. `MaterialEntry` mutable kullanılıyor (event olması gerekirken)

**Sorun**
`MaterialEntry` kavramsal olarak bir _event / append-only log_ olmalı:
"Şu tarihte, şu malzemeden, şu lokasyona, N adet girdi."
Ancak bugün UI'daki "Düzenle" butonu kullanıcıya `MaterialId`, `LocationId`, `PersonelId`,
`BrandId`, `ModelId`, `Quantity`, `MaterialUnitId` alanlarını **geçmişe dönük** değiştirme
imkânı veriyor. `MaterialEntryService.UpdateMaterialEntryAndInventoryAsync` bu değişikliği
`MaterialInventory`'e delta uygulayarak destekliyor.

**Etki**
- `MaterialEntry`'in tarihsel bütünlüğü yok: "geçmişte nereye, ne kadar girdi" sorusu
  güvenilir şekilde cevaplanamıyor.
- `MaterialAsset` (fiziksel varlık kayıtları), entry düzenlenince **güncellenmiyor**.
  Yani entry'nin lokasyonunu değiştirmek envanter sayacını günceller ama fiziksel
  varlık kayıtlarını eski lokasyonda bırakır → tutarsızlık.
- "Düzeltme" (yazım/tarih hatası) ile "Transfer" (malzemeyi başka yere taşıdık)
  iş akışları aynı butona binmiş durumda; kullanıcı niyeti ayırt edilemiyor.

**Önerilen çözüm**
1. `MaterialEntry`'i **immutable**'a yaklaştır. UI'da "Düzenle"yi sadece gerçek düzeltmelere
   (örn. `Aciklama`, `EntryDate` küçük tarih hatası) daralt.
2. Lokasyon / Personel değişimi için **Transfer** akışı kullan: bu akış zaten var olan
   `MaterialMovement` (ve gerekirse `MaterialTransfer` / `MaterialAssetLog`) üzerinden işler;
   `MaterialEntry`'e dokunmaz, `MaterialInventory` ve `MaterialAsset`'i atomik günceller.
3. Zimmet değişimi için ayrı "Zimmetle / Zimmet Al" akışı; yine event tabanlı.
4. Asset ↔ Entry bağı için `MaterialAsset.SourceMaterialEntryId` (nullable FK) eklenmesi
   (asset'lerin hangi girişten doğduğunu kayda almak için). Mevcut asset'ler için NULL kalır.

**Önkoşul / Risk**
- Production'da kullanıcılar mevcut "Düzenle" akışına alışkın → davranış değişikliği için
  iletişim/eğitim gerekli.
- UI tarafı (Blazor) yeniden tasarım gerektirir: `MaterialEntryEditModal` dışında "Transfer"
  ve "Zimmetle" modalları/sayfaları eklenmeli.
- Geriye dönük veri: mevcut entry'ler üzerinde yapılmış geçmiş "düzenlemeler" kaybedilmiş
  durumda; düzeltilemez. İleriye dönük doğru çalışacak.

**Durum**
- **Kısmen çözüldü (Phase A — 2025).**
  `MaterialAsset.SourceMaterialEntryId` (nullable FK) eklendi.
  `MaterialEntryService.AddAsync` her yeni asset'i bu FK ile damgalıyor.
  `UpdateMaterialEntryAndInventoryAsync` artık bu entry'den doğmuş ve **hareket görmemiş**
  ("el değmemiş": `MaterialAssetLog`'da kaydı yok ve mevcut tuple'ı eski entry tuple'ı ile
  aynı) asset'leri Brand/Model/Location/Personel için günceller; Quantity artışında yeni
  asset'ler üretir, azalışında en yeni el-değmemiş asset'leri siler. Hareket görmüş asset'lere
  asla dokunmaz. `DeleteMaterialEntryAndUpdateInventoryAsync` da aynı kuralla el-değmemiş
  asset'leri temizler. Eski (NULL `SourceMaterialEntryId`) asset'ler dokunulmaz.
- **Açık kalan kısımlar (Phase B / C):**
  1. `MaterialEntry`'i tam immutable yapma; "Düzenle"yi sadece `Aciklama` + küçük tarih
     düzeltmesine indirme.
  2. Lokasyon değişimi için ayrı **Transfer** UI/akışı.
  3. Personel değişimi için ayrı **Zimmetle / Zimmet Al** UI/akışı.
  4. UoW canary refactor serisi tamamlandıktan sonra ele alınacak.

- Canary serisinde `UpdateMaterialEntryAndInventoryAsync` **mevcut davranış korunarak**
  atomikleştirildi; Phase A senkronu da aynı transaction içinde çalışır.

**İlgili dosyalar**
- `Jenga.DataAccess/Services/Inventory/MaterialEntryService.cs`
- `Jenga.BlazorUI/Components/Inventory/MaterialEntryTable.razor`
- `Jenga.BlazorUI/Components/Inventory/MaterialEntryEditModal.razor`
- `Jenga.Models/Inventory/MaterialEntry.cs`
- `Jenga.Models/Inventory/MaterialAsset.cs`
- `Jenga.Models/Inventory/MaterialMovement.cs`
