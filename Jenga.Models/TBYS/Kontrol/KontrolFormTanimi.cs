using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jenga.Models.TBYS.Kontrol
{
    /// <summary>
    /// Bir form sürümünün tam tanımı. <see cref="TasinmazKontrolForm.TanimJson"/> içinde saklanır;
    /// yayımlanan sürümde bütün olarak değişmez kalır.
    /// </summary>
    public sealed class KontrolFormTanimi
    {
        public List<KontrolBolumTanimi> Bolumler { get; set; } = new();

        /// <summary>Excel → form eşleştirme notları (serbest metin, bilgi amaçlı).</summary>
        public List<string> Notlar { get; set; } = new();

        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter() }
        };

        public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

        public static KontrolFormTanimi FromJson(string? json)
            => string.IsNullOrWhiteSpace(json)
                ? new KontrolFormTanimi()
                : JsonSerializer.Deserialize<KontrolFormTanimi>(json, JsonOptions) ?? new KontrolFormTanimi();

        /// <summary>Derin kopya (yeni sürüm / kopyalama için).</summary>
        public KontrolFormTanimi Kopyala() => FromJson(ToJson());

        /// <summary>Tüm alanları (grup içindekiler dâhil) düz liste olarak döner.</summary>
        public IEnumerable<KontrolAlanTanimi> TumAlanlar()
        {
            foreach (var b in Bolumler)
            {
                foreach (var a in b.Alanlar) yield return a;
                foreach (var g in b.Gruplar)
                    foreach (var a in g.Alanlar) yield return a;
            }
        }

        public KontrolAlanTanimi? AlanBul(string kod)
            => TumAlanlar().FirstOrDefault(a => string.Equals(a.Kod, kod, StringComparison.OrdinalIgnoreCase));

        public KontrolGrupTanimi? GrupBul(string kod)
            => Bolumler.SelectMany(b => b.Gruplar)
                       .FirstOrDefault(g => string.Equals(g.Kod, kod, StringComparison.OrdinalIgnoreCase));
    }

    public sealed class KontrolBolumTanimi
    {
        public string Kod { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public int Sira { get; set; }
        public List<KontrolAlanTanimi> Alanlar { get; set; } = new();
        public List<KontrolGrupTanimi> Gruplar { get; set; } = new();
    }

    /// <summary>
    /// Alt alanları olan grup. <see cref="Tekrarlanabilir"/> ise kullanıcı birden fazla satır girebilir
    /// (ör. emsaller, irtibat kişileri); değilse tek satırlık alt alan kümesidir (ör. incelenen belge).
    /// </summary>
    public sealed class KontrolGrupTanimi
    {
        public string Kod { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public int Sira { get; set; }
        public bool Tekrarlanabilir { get; set; }
        public int? EnAzSatir { get; set; }
        public List<KontrolAlanTanimi> Alanlar { get; set; } = new();
    }

    public sealed class KontrolAlanTanimi
    {
        /// <summary>Sürümler arasında korunan kalıcı kimlik. Anlamı değişen soruya yeni kod verilir.</summary>
        public string Kod { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public int Sira { get; set; }
        public KontrolAlanTipi Tip { get; set; }
        public bool Zorunlu { get; set; }
        public string? Birim { get; set; }
        public List<KontrolSecenekTanimi> Secenekler { get; set; } = new();

        public KontrolVeriKaynagiTipi VeriKaynagi { get; set; } = KontrolVeriKaynagiTipi.Manuel;

        /// <summary><see cref="TasinmazVeriKaynagi"/> içinde izin verilen anahtarlardan biri.</summary>
        public string? TasinmazAlani { get; set; }

        public bool SecenekliMi => Tip is KontrolAlanTipi.TekliSecim or KontrolAlanTipi.CokluSecim;
    }

    public sealed class KontrolSecenekTanimi
    {
        public string Kod { get; set; } = string.Empty;
        public string Etiket { get; set; } = string.Empty;
        public int Sira { get; set; }
    }

    /// <summary>
    /// Bir kontrol kaydının cevapları. <see cref="TasinmazKontrol.CevaplarJson"/> içinde saklanır.
    /// Anahtar: alan kodu. Grup cevapları satır listesi olarak tutulur.
    /// Boş cevap = anahtar yok ya da null; EvetHayır alanında Hayır ve Uygulanamaz ayrı değerlerdir.
    /// </summary>
    public sealed class KontrolCevaplari
    {
        public Dictionary<string, string?> Alanlar { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Grup kodu → satırlar; her satır alan kodu → değer.</summary>
        public Dictionary<string, List<Dictionary<string, string?>>> Gruplar { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public string ToJson() => JsonSerializer.Serialize(this, KontrolFormTanimi.JsonOptions);

        public static KontrolCevaplari FromJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new KontrolCevaplari();
            var c = JsonSerializer.Deserialize<KontrolCevaplari>(json, KontrolFormTanimi.JsonOptions) ?? new KontrolCevaplari();
            c.Alanlar = new Dictionary<string, string?>(c.Alanlar, StringComparer.OrdinalIgnoreCase);
            c.Gruplar = new Dictionary<string, List<Dictionary<string, string?>>>(c.Gruplar, StringComparer.OrdinalIgnoreCase);
            return c;
        }

        public string? Al(string kod) => Alanlar.TryGetValue(kod, out var v) ? v : null;

        public void Koy(string kod, string? deger)
        {
            if (string.IsNullOrWhiteSpace(deger)) Alanlar.Remove(kod);
            else Alanlar[kod] = deger;
        }

        public List<Dictionary<string, string?>> GrupSatirlari(string grupKodu)
        {
            if (!Gruplar.TryGetValue(grupKodu, out var satirlar))
            {
                satirlar = new List<Dictionary<string, string?>>();
                Gruplar[grupKodu] = satirlar;
            }
            return satirlar;
        }

        /// <summary>Çoklu seçim cevapları ';' ile ayrılmış seçenek kodlarıdır.</summary>
        public const char CokluSecimAyirici = ';';
    }
}
