# GenericTable<TItem> — Developer Manual

> **Konum:** `Jenga.BlazorUI/Components/Common/GenericTable.razor`  
> **Tür:** Generic Blazor bileşeni (`@typeparam TItem`)

Sayfalama, arama, sıralama, panoya kopyalama ve Excel export özelliklerini içeren, projede tüm tablo ihtiyaçları için kullanılan reusable bileşen.

## Parametreler

### Veri ve Kimlik

| Parametre | Tür | Varsayılan | Açıklama |
|---|---|---|---|
| Items | IEnumerable\<TItem\> | [] | Tabloda gösterilecek veri listesi |
| KeySelector | Func\<TItem, object\> | null | Blazor @key için benzersiz tanımlayıcı. Performans için önerilir |

### Şablonlar (Templates)

| Parametre | Tür | Açıklama |
|---|---|---|
| HeaderTemplate | RenderFragment | Özel th içeriği. Verilmezse model property'lerinden otomatik oluşturulur |
| RowTemplate | RenderFragment\<TItem\> | Özel td içeriği. Verilmezse property değerleri otomatik basılır |
| DetailRowTemplate | RenderFragment\<TItem\> | Her satırın altında koşullu genişletilebilir detay satırı. İçerik kendi tr etiketini içermelidir |

### Arama

| Parametre | Tür | Varsayılan | Açıklama |
|---|---|---|---|
| ShowSearch | bool | true | Arama kutusunu göster/gizle |
| PlaceholderSearch | string | "Ara..." | Arama kutusu placeholder metni |
| SearchTextSelector | Func\<TItem, string?\> | null | Aranacak metni döndüren fonksiyon. Verilmezse tüm public property'ler taranır |

### Sayfalama

| Parametre | Tür | Varsayılan | Açıklama |
|---|---|---|---|
| PageSize | int | 10 | Sayfa başına satır sayısı |
| PageSizeOptions | int[] | [5,10,20,50] | Sayfa boyutu seçenekleri |

### Görünüm

| Parametre | Tür | Varsayılan | Açıklama |
|---|---|---|---|
| TableHeight | string | "400px" | Tablo wrapper yüksekliği. "Auto" verilebilir |
| TableClass | string | null | Ek CSS class'ları. Base "table gt-table" otomatik eklenir |
| RowClassSelector | Func\<TItem, string?\> | null | Satır bazlı dinamik CSS class |

### Export ve Kopyalama

| Parametre | Tür | Varsayılan | Açıklama |
|---|---|---|---|
| ShowCopy | bool | true | Panoya kopyalama butonu |
| ShowExport | bool | true | Excel export butonu |
| ExportFileName | string | null | Export dosya adı. null ise export-tarih.xlsx |
| ExportOnlyCurrentPage | bool | false | true ise yalnızca mevcut sayfa export edilir |
| ExportHeaders | IReadOnlyList\<string\> | null | Özel export başlıkları |
| ExportRow | Func\<TItem, IReadOnlyList\<object?\>\> | null | Özel export satır verisi (ID yerine display name basmak için) |

### Olaylar

| Parametre | Tür | Açıklama |
|---|---|---|
| OnRowClicked | EventCallback\<TItem\> | Satıra tıklandığında tetiklenir |

### Public Üyeler (parent erişimi için)

| Üye | Tür | Açıklama |
|---|---|---|
| SortBy(string field) | Metot | HeaderTemplate kullanılırken parent'tan sıralama tetiklemek için |
| CurrentSortField | string | Aktif sıralama alanı |
| CurrentSortDescending | bool | Sıralama yönü |
| CurrentSearchText | string | Mevcut arama metni |

## Kullanım Modları

### 1. Otomatik Mod (Template'siz)

Model property'lerinden otomatik sütun ve satır oluşturur. [DisplayName] veya [Display(Name)] attribute'ları sütun başlığı olarak kullanılır. [Browsable(false)] ile işaretlenen property'ler gizlenir.

    <GenericTable TItem="MyModel" Items="myList" />

### 2. Template Mod (HeaderTemplate + RowTemplate)

Sütun başlıkları ve satır içeriği tamamen tüketiciye bırakılır.

    <GenericTable TItem="MyModel"
                  Items="myList"
                  KeySelector="x => x.Id"
                  TableHeight="Auto"
                  PageSize="10">
        <HeaderTemplate>
            <th>Ad</th>
            <th>Durum</th>
        </HeaderTemplate>
        <RowTemplate Context="item">
            <td>@item.Name</td>
            <td><span class="badge">@item.Status</span></td>
        </RowTemplate>
    </GenericTable>

### 3. Template + Sort (HeaderTemplate ile sıralama)

HeaderTemplate kullanıldığında otomatik sort devre dışı kalır. Parent bileşen @ref ile SortBy() çağırmalıdır:

    <GenericTable TItem="MyModel"
                  @ref="tableRef"
                  Items="myList"
                  KeySelector="x => x.Id">
        <HeaderTemplate>
            <th role="button" @onclick="() => OnSort(nameof(MyModel.Name))">
                <span class="d-inline-flex align-items-center">
                    <span>Ad</span>
                    <i class="@GetSortIcon(nameof(MyModel.Name))"></i>
                </span>
            </th>
        </HeaderTemplate>
        <RowTemplate Context="item">
            <td>@item.Name</td>
        </RowTemplate>
    </GenericTable>

    @code {
        private GenericTable<MyModel>? tableRef;

        private void OnSort(string field) => tableRef?.SortBy(field);

        private string GetSortIcon(string field)
        {
            if (tableRef?.CurrentSortField != field)
                return "bi bi-arrow-down-up ms-2 text-muted";

            return tableRef.CurrentSortDescending
                ? "bi bi-caret-down-fill ms-2"
                : "bi bi-caret-up-fill ms-2";
        }
    }

### 4. DetailRowTemplate (Genişletilebilir satır)

Her satırın altında koşullu ek içerik render eder. İçerik kendi tr etiketini mutlaka içermelidir:

    <GenericTable TItem="MyModel" Items="myList">
        <RowTemplate Context="item">
            <td>@item.Name</td>
            <td>
                <button @onclick="() => ToggleDetail(item.Id)">Detay</button>
            </td>
        </RowTemplate>
        <DetailRowTemplate Context="item">
            @if (expandedId == item.Id)
            {
                <tr>
                    <td colspan="2" class="bg-light p-2">
                        Detay içeriği...
                    </td>
                </tr>
            }
        </DetailRowTemplate>
    </GenericTable>

DetailRowTemplate içinde koşul (@if) kullanılmazsa her satırın altında detay görünür.

### 5. RowClassSelector (Koşullu satır stili)

    <GenericTable TItem="MyModel"
                  Items="myList"
                  RowClassSelector="GetRowClass">
        ...
    </GenericTable>

    @code {
        private string GetRowClass(MyModel item)
            => item.IsActive ? "" : "table-danger";
    }

Base gt-row class'ı her zaman korunur, ek class birleştirilir.

### 6. Özel Excel Export

ID yerine display name'lerle export:

    <GenericTable TItem="MyModel"
                  Items="myList"
                  ShowExport="true"
                  ExportFileName="rapor.xlsx"
                  ExportHeaders="headers"
                  ExportRow="GetRow">
        ...
    </GenericTable>

    @code {
        private static readonly IReadOnlyList<string> headers = new[] { "Ad", "Durum" };

        private IReadOnlyList<object?> GetRow(MyModel item) => new object?[]
        {
            item.Name,
            item.IsActive ? "Aktif" : "Pasif"
        };
    }

## Arama Davranışı

| Senaryo | Davranış |
|---|---|
| SearchTextSelector verildi | Yalnızca döndürülen string içinde aranır |
| SearchTextSelector verilmedi | Tüm public property'ler reflection ile taranır (nested objeler 2 seviye derinliğe kadar) |

## Sıralama Davranışı

| Senaryo | Davranış |
|---|---|
| HeaderTemplate yok | Sütun başlıklarına tıklama ile otomatik sort |
| HeaderTemplate var | Otomatik sort devre dışı. @ref + SortBy() pattern'i kullanılmalı |

## CSS Class'ları

| Class | Açıklama |
|---|---|
| .generic-table | Root wrapper |
| .gt-controls | Üst kontrol barı (arama + sayfa boyutu + butonlar) |
| .gt-search | Arama input'u |
| .gt-table-wrapper | Scroll container (sticky header için) |
| .gt-table | table elementi (table class'ı ile birlikte) |
| .gt-row | Her tr satırı (hover efekti burada) |
| .gt-sort-indicator | Sort ikon stili |
| .gt-pagination | Sayfalama barı |

## Projede Kullanım Örnekleri

| Bileşen | Mod | Özellikler |
|---|---|---|
| AddRemoveListComponent | Template | Basit liste + silme butonu |
| MaterialBrandTable | Template + Sort + Export | @ref ile sort, custom header, Excel export |
| MaterialAssetTable | Template + Sort + Export + DetailRow + RowClass | Tam kapsamlı: inline edit, detay satırı, koşullu row class, özel export |