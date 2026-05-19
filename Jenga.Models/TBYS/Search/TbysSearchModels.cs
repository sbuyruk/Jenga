// SearchResultTipi, SearchResultItem, GlobalSearchSonucu → Jenga.Models.Search namespace'ine taşındı.

namespace Jenga.Models.TBYS.Search
{

    public class KiraciDetayVM
    {
        public Kiraci Kiraci { get; set; } = new();
        public List<KiraSozlesmeDetayVM> Sozlesmeler { get; set; } = [];
    }

    public class KiraSozlesmeDetayVM
    {
        public KiraSozlesme Sozlesme { get; set; } = new();
        public List<Tasinmaz> Tasinmazlar { get; set; } = [];
        public List<OdemePlani> OdemePlanlari { get; set; } = [];
    }

    public class TasinmazDetayVM
    {
        public Tasinmaz Tasinmaz { get; set; } = new();
        public TasinmazBagisci? Bagisci { get; set; }
        public List<KiraSozlesmeDetayVM> Sozlesmeler { get; set; } = [];
    }

    public class BagisciDetayVM
    {
        public TasinmazBagisci Bagisci { get; set; } = new();
        public List<Tasinmaz> Tasinmazlar { get; set; } = [];
        public List<BagisciYakinlari> Yakinlar { get; set; } = [];
        public List<BagisciTalepleri> Talepler { get; set; } = [];
    }
}
