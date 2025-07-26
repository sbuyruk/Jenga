using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.MTS;
using Jenga.Utility;


namespace Jenga.Web.Areas.Admin.Services
{
    public class KatilimciService
    {
        private readonly IUnitOfWork _unitOfWork;

        public KatilimciService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Katilimci GetKatilimci(int katilimciId, int katilimciTipi)
        {
            if (katilimciId == 0)
                return null;
            Katilimci retVal = new();
            try
            {
                if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_DIS_INT)
                {

                    //Kisi kisi = _unitOfWork.Kisi.GetFirstOrDefault(u => u.Id == katilimciId);
                    Kisi kisi = _unitOfWork.Kisi.IncludeThis(katilimciId);
                    if (kisi != null)
                    {
                        Katilimci katilimci = new Katilimci()
                        {
                            Id = kisi.Id,
                            Adi = kisi.Adi,
                            Aciklama = kisi.Aciklama,
                            Adres = kisi.Adres,
                            Dahili1 = kisi.Dahili1,
                            Dahili2 = kisi.Dahili2,
                            Dahili3 = kisi.Dahili3,
                            DogumTarihi = kisi.DogumTarihi,
                            Gorevi = (kisi.MTSKurumGorevs == null) || (kisi.MTSKurumGorevs.Count == 0) ? string.Empty : kisi.MTSKurumGorevs[0].MTSGorevTanim.Adi,
                            Ilcesi = kisi.Ilcesi,
                            Ili = kisi.Ili,
                            Kurumu = (kisi.MTSKurumGorevs == null) || (kisi.MTSKurumGorevs.Count == 0) ? string.Empty : kisi.MTSKurumGorevs[0].MTSKurumTanim.Adi,
                            Kutlama = kisi.Kutlama,
                            RandevuKisiti = kisi.RandevuKisiti,
                            KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_DIS_INT,
                            Soyadi = kisi.Soyadi,
                            Unvani = kisi.Unvani,
                            TCKimlikNo = kisi.TCKimlikNo,
                            Telefon1 = kisi.Telefon1,
                            Telefon2 = kisi.Telefon2,
                            Telefon3 = kisi.Telefon3,
                            TelAciklama1 = kisi.TelAciklama1,
                            TelAciklama2 = kisi.TelAciklama2,
                            TelAciklama3 = kisi.TelAciklama3,
                            Eposta = kisi.EPosta,

                        };
                        retVal = katilimci;
                    }
                    else
                    {
                        return new Katilimci();
                    }
                }
                else if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_IC_INT)
                {
                    //Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "IsBilgileri");
                    //var personel = _unitOfWork.IsBilgileri.GetFirstOrDefault(u => u.Id == katilimciId, includeProperties: "Personel");
                    var personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == katilimciId, includeProperties: "Kimlik,IsBilgileri,IletisimBilgileri");
                    if (personel != null)
                    {
                        Katilimci katilimci = new Katilimci()
                        {
                            Id = katilimciId == null ? 0 : katilimciId,
                            Adi = personel.Adi,
                            Aciklama = personel.Aciklama,
                            Adres = personel.IletisimBilgileri.Adres,
                            Dahili1 = personel.IletisimBilgileri.DahiliTelefonu,
                            //Dahili2 = personel.Dahili2,
                            //Dahili3 = personel.Dahili3,
                            DogumTarihi = personel.Kimlik.DogumTar,
                            Eposta = personel.IletisimBilgileri.InternetEPosta,
                            //Gorevi = personel.Gorevi,
                            Ilcesi = personel.IletisimBilgileri.Ilcesi,
                            Ili = personel.IletisimBilgileri.Ili,
                            Kutlama = personel.Kimlik.DogumGunuKutlama,
                            KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_IC_INT,
                            Kurumu = ProjectConstants.FAALIYET_KATILIMCI_IC,
                            Telefon1 = personel.IletisimBilgileri.CepTelefonu,
                            Telefon2 = personel.IletisimBilgileri.CepTelefonu2,
                            Telefon3 = personel.IletisimBilgileri.EvTelefonu,
                            Unvani = personel.IsBilgileri == null || personel.IsBilgileri.UnvanTanim == null ? string.Empty : personel.IsBilgileri.UnvanTanim.Adi,
                            RandevuKisiti = false,
                            Soyadi = personel.Soyadi,
                            TCKimlikNo = personel.Kimlik.TCKimlikNo,

                        };
                        retVal = katilimci;
                    }
                    else
                    {
                        return new Katilimci();
                    }
                }
                else if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT)
                {
                    var nakitBagisci = _unitOfWork.NakitBagisci.GetFirstOrDefault(u => u.Id == katilimciId);
                    if (nakitBagisci != null)
                    {
                        Katilimci katilimci = new Katilimci()
                        {
                            Id = katilimciId == null ? 0 : katilimciId,
                            Adi = nakitBagisci.Adi,
                            Aciklama = nakitBagisci.Aciklama,
                            Adres = nakitBagisci.Adres,
                            //Dahili1 = personel.Dahili1,
                            //Dahili2 = personel.Dahili2,
                            //Dahili3 = personel.Dahili3,
                            //DogumTarihi = personel.DogumTarihi,
                            //Gorevi = personel.Gorevi,
                            Eposta = nakitBagisci.Eposta,
                            TCKimlikNo = nakitBagisci.TCKimlikNo,
                            Telefon1 = nakitBagisci.Telefon1,
                            Telefon2 = nakitBagisci.Telefon2,
                            Ilcesi = nakitBagisci.Ilcesi,
                            Ili = nakitBagisci.Ili,
                            Kurumu = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI,
                            //Kutlama = personel.Kutlama,
                            KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT,
                            Soyadi = nakitBagisci.Soyadi,
                            //Unvani = personel.Unvani,
                            RandevuKisiti = false,
                        };
                        retVal = katilimci;
                    }
                    else
                    {
                        return new Katilimci();
                    }

                }
                else if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT)
                {
                    var tasinmazBagisci = _unitOfWork.TasinmazBagisci.GetFirstOrDefault(u => u.Id == katilimciId);
                    if (tasinmazBagisci != null)
                    {
                        Katilimci katilimci = new Katilimci()
                        {
                            Id = katilimciId == null ? 0 : katilimciId,
                            Adi = tasinmazBagisci.Adi,
                            Aciklama = tasinmazBagisci.Aciklama,
                            Adres = tasinmazBagisci.Adres,
                            DogumTarihi = tasinmazBagisci.DogumTarihi,
                            Eposta = tasinmazBagisci.EPosta,
                            Ili = GetIliByName(tasinmazBagisci.Ili),
                            Ilcesi = GetIlcesiiByName(tasinmazBagisci.Ilcesi),
                            TCKimlikNo = tasinmazBagisci.TCKimlikNo,
                            Telefon1 = tasinmazBagisci.Telefon1,
                            Telefon2 = tasinmazBagisci.Telefon2,
                            Kurumu = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI,
                            //Kutlama = personel.Kutlama,
                            KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT,
                            Soyadi = tasinmazBagisci.Soyadi,

                            RandevuKisiti = false,
                        };
                        retVal = katilimci;
                    }
                    else
                    {
                        return new Katilimci();
                    }
                }
            }
            catch (Exception e)
            {

                throw e;
            }
            return retVal;
        }

        private int? GetIlcesiiByName(string? ilceAdi)
        {
            var ilcesi = _unitOfWork.Ilce.GetFirstOrDefault(u => u.IlceAdi == ilceAdi);
            return ilcesi == null ? 0 : ilcesi.Id;
        }

        private int? GetIliByName(string? ilAdi)
        {
            var ili = _unitOfWork.Il.GetFirstOrDefault(u => u.IlAdi == ilAdi);
            return ili == null ? 0 : ili.Id;
        }

        public List<Katilimci> GetKatilimciList(int katilimciTipi)
        {
            var KatilimciList = new List<Katilimci>();
            switch (katilimciTipi)
            {
                case ProjectConstants.FAALIYET_KATILIMCI_IC_INT:
                    {
                        var personeller = _unitOfWork.Personel.GetAll(includeProperties: "Kimlik");
                        List<Katilimci> katilimciPersonelList = personeller.Select(a => new Katilimci()
                        {
                            Id = a.Id,
                            Adi = a.Adi,
                            Soyadi = a.Soyadi,
                            KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_IC_INT,
                            Kurumu = ProjectConstants.FAALIYET_KATILIMCI_IC,
                            Aciklama = a.Aciklama,
                            TCKimlikNo = a.Kimlik.TCKimlikNo,

                        }).ToList();
                        KatilimciList = katilimciPersonelList;
                        break;
                    }
                case ProjectConstants.FAALIYET_KATILIMCI_DIS_INT:
                    {
                        var kisiler = _unitOfWork.Kisi.IncludeIt();
                        List<Katilimci> katilimciKisiList = kisiler.Select(a => new Katilimci()
                        {
                            Id = a.Id,
                            Adi = a.Adi,
                            Soyadi = a.Soyadi,
                            KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_DIS_INT,
                            Kurumu = (a.MTSKurumGorevs == null) || (a.MTSKurumGorevs.Count == 0) ? string.Empty : a.MTSKurumGorevs[0].MTSKurumTanim.Adi,
                            Gorevi = (a.MTSKurumGorevs == null) || (a.MTSKurumGorevs.Count == 0) ? string.Empty : a.MTSKurumGorevs[0].MTSGorevTanim.Adi,
                            Ili = a.Ili,
                            Ilcesi = a.Ilcesi,
                            Adres = a.Adres,
                            Telefon1 = a.Telefon1,
                            Telefon2 = a.Telefon2,
                            Telefon3 = a.Telefon3,
                            TelAciklama1 = a.TelAciklama1,
                            TelAciklama2 = a.TelAciklama2,
                            TelAciklama3 = a.TelAciklama3,
                            Dahili1 = a.Dahili1,
                            Dahili2 = a.Dahili2,
                            Dahili3 = a.Dahili3,
                            TCKimlikNo = a.TCKimlikNo,
                            DogumTarihi = a.DogumTarihi,
                            Kutlama = a.Kutlama,
                            RandevuKisiti = a.RandevuKisiti,
                            Unvani = a.Unvani,
                            Aciklama = a.Aciklama,

                        }).ToList();
                        KatilimciList = katilimciKisiList;
                        break;
                    }
                case ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT:
                    {
                        var nakitBagiscilar = _unitOfWork.NakitBagisci.GetByFilter(m => !string.IsNullOrEmpty(m.Adi) || !m.Adi.Equals("BİLİNMEYEN"));


                        List<Katilimci> katilimciNakitBagisciList = nakitBagiscilar.Select(a => new Katilimci()
                        {
                            Id = a.Id,
                            Adi = a.Adi,
                            Soyadi = a.Soyadi,
                            KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT,
                            Kurumu = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI,
                            TCKimlikNo = a.TCKimlikNo,
                            Aciklama = a.Aciklama,
                            Adres = a.Adres

                        }).ToList();
                        KatilimciList = katilimciNakitBagisciList;

                        break;
                    }
                case ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT:
                    {
                        var tasinmazBagiscilar = _unitOfWork.TasinmazBagisci.GetAll();

                        List<Katilimci> katilimciTasinmazBagisciList = tasinmazBagiscilar.Select(a => new Katilimci()
                        {
                            Id = a.Id,
                            Adi = a.Adi,
                            Soyadi = a.Soyadi,
                            KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT,
                            Kurumu = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI,
                            TCKimlikNo = a.TCKimlikNo,
                            Aciklama = a.Aciklama,
                            Adres = a.Adres,

                        }).ToList();
                        KatilimciList = katilimciTasinmazBagisciList;
                        break;
                    }
                case ProjectConstants.FAALIYET_KATILIMCI_FTK_INT:
                    {

                        break;
                    }

                default:
                    break;
            }

            return KatilimciList;

        }

        internal List<MTSKurumGorev> CreateKurumGorev(Kisi kisi)
        {
            List<MTSKurumGorev> kurumGorevList = new List<MTSKurumGorev>();
            var kurumAdi = string.Empty;
            var gorevAdi = string.Empty;
            //Kurum yoksa yarat
            switch (kisi.KatilimciTipi)
            {
                case ProjectConstants.FAALIYET_KATILIMCI_IC_INT:
                    {
                        //Kurum Yarat
                        kurumAdi = ProjectConstants.FAALIYET_KATILIMCI_IC;
                        gorevAdi = ProjectConstants.FAALIYET_KATILIMCI_IC;
                        var personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == kisi.KatilimciId, includeProperties: "IsBilgileri");
                        if (personel != null && personel.IsBilgileri != null && personel.IsBilgileri.CalismaDurumu != null)
                        {
                            gorevAdi = (int.Parse(personel.IsBilgileri.CalismaDurumu) == ProjectConstants.PER_CALISIYOR_INT)
                                ? ProjectConstants.FAALIYET_KATILIMCI_IC
                                : ProjectConstants.FAALIYET_KATILIMCI_IC_ESKI;
                        }

                        break;
                    }
                case ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT:
                    {
                        //Kurum Yarat
                        kurumAdi = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI;
                        gorevAdi = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI;
                        break;
                    }
                case ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT:
                    {
                        //Kurum Yarat
                        kurumAdi = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI;
                        gorevAdi = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI;
                        break;
                    }
                default:
                    {
                        //Kurum Yarat
                        kurumAdi = ProjectConstants.FAALIYET_KATILIMCI_DIS;
                        gorevAdi = ProjectConstants.FAALIYET_KATILIMCI_DIS;
                        break;
                    }
                    break;
            }
            MTSKurumTanim mTSKurumTanim = _unitOfWork.MTSKurumTanim.GetFirstOrDefault(u => u.Adi == kurumAdi);
            if (mTSKurumTanim == null)
            {
                mTSKurumTanim = new()
                {
                    Adi = kurumAdi,
                    KisaAdi = kurumAdi,

                };
                _unitOfWork.MTSKurumTanim.Add(mTSKurumTanim);
                _unitOfWork.Save();
            }
            //Gorev yarat
            MTSGorevTanim mTSGorevTanim = _unitOfWork.MTSGorevTanim.GetFirstOrDefault(u => u.Adi == gorevAdi);
            if (mTSGorevTanim == null)
            {
                mTSGorevTanim = new()
                {
                    Adi = gorevAdi,
                    KisaAdi = gorevAdi,
                    MTSKurumTanimId = mTSKurumTanim.Id,

                };
                _unitOfWork.MTSGorevTanim.Add(mTSGorevTanim);
                _unitOfWork.Save();
            }
            MTSKurumGorev mTSKurumGorev = _unitOfWork.MTSKurumGorev.GetFirstOrDefault(u => u.KisiId == kisi.Id);
            if (mTSKurumGorev == null)
            {
                mTSKurumGorev = new MTSKurumGorev()
                {
                    MTSKurumTanimId = mTSKurumTanim.Id,
                    MTSGorevTanimId = mTSGorevTanim.Id,
                    KisiId = kisi.Id,
                };
                kurumGorevList.Add(mTSKurumGorev);
            }
            return kurumGorevList;
            //Gorev yarat
            //KurumGoreve kayıt ekle
            //geriye list dondur

        }

        internal int? CreateUnvan(int katilimciTipi)
        {
            int unvan = 0;
            switch (katilimciTipi)
            {
                case ProjectConstants.FAALIYET_KATILIMCI_IC_INT:
                    {

                        break;
                    }
                case ProjectConstants.FAALIYET_KATILIMCI_DIS_INT:
                    {

                        break;
                    }
                case ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT:
                    {
                        unvan = ProjectConstants.MTSUNVAN_NAKITBAGISCI_INT;
                        break;
                    }
                case ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT:
                    {
                        unvan = ProjectConstants.MTSUNVAN_TASINMAZBAGISCI_INT;
                        break;
                    }
                case ProjectConstants.FAALIYET_KATILIMCI_FTK_INT:
                    {

                        break;
                    }

                default:
                    break;
            }
            return unvan;
        }
    }

}