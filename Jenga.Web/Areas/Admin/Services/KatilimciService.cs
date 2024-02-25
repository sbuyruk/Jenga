using Jenga.DataAccess.Repository;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.EntityFrameworkCore;


namespace Jenga.Web.Areas.Admin.Services
{
    public class KatilimciService
    {
        private readonly IUnitOfWork _unitOfWork;

        public KatilimciService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Katilimci GetKatilimci( int? katilimciId, int katilimciTipi)
        {
            Katilimci retVal = new();
            if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_DIS_INT)
            {

                //Kisi kisi = _unitOfWork.Kisi.GetFirstOrDefault(u => u.Id == katilimciId);

                Kisi kisi = _unitOfWork.Kisi.IncludeThis(katilimciId);
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
                    Gorevi = (kisi.MTSKurumGorevs==null)||(kisi.MTSKurumGorevs.Count ==0) ?string.Empty: kisi.MTSKurumGorevs[0].MTSGorevTanim.Adi,
                    Ilcesi = kisi.Ilcesi,
                    Ili = kisi.Ili,
                    Kurumu = (kisi.MTSKurumGorevs == null)|| (kisi.MTSKurumGorevs.Count == 0) ? string.Empty : kisi.MTSKurumGorevs[0].MTSKurumTanim.Adi,
                    Kutlama = kisi.Kutlama,
                    RandevuKisiti = kisi.RandevuKisiti,
                    KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_DIS_INT,
                    Soyadi = kisi.Soyadi,
                    Unvani = kisi.Unvani,
                };
                retVal = katilimci;
            }
            else if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_IC_INT)
            {
                //Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "IsBilgileri");
                var personel = _unitOfWork.IsBilgileri.GetFirstOrDefault(u => u.Id == katilimciId, includeProperties: "Personel");
                Katilimci katilimci = new Katilimci()
                {
                    Id = katilimciId==null?0:katilimciId.Value,
                    Adi = personel.Personel.Adi,
                    Aciklama = personel.Personel.Aciklama,
                    //Adres = personel.Adres,
                    //Dahili1 = personel.Dahili1,
                    //Dahili2 = personel.Dahili2,
                    //Dahili3 = personel.Dahili3,
                    //DogumTarihi = personel.DogumTarihi,
                    //Gorevi = personel.Gorevi,
                    //Ilcesi = personel.Ilcesi,
                    //Ili = personel.Ili,
                    Kurumu = ProjectConstants.FAALIYET_KATILIMCI_IC,
                    //Kutlama = personel.Kutlama,
                    KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_IC_INT,
                    Soyadi = personel.Personel.Soyadi,
                    //Unvani = personel.Unvani,
                };
                retVal = katilimci;

            }
            else if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT)
            {
                var nakitBagisci = _unitOfWork.NakitBagisci.GetFirstOrDefault(u => u.Id == katilimciId);
                if (nakitBagisci != null)
                {
                    Katilimci katilimci = new Katilimci()
                    {
                        Id = katilimciId == null ? 0 : katilimciId.Value,
                        Adi = nakitBagisci.Adi,
                        Aciklama = nakitBagisci.Aciklama,
                        Adres = nakitBagisci.Adres,
                        //Dahili1 = personel.Dahili1,
                        //Dahili2 = personel.Dahili2,
                        //Dahili3 = personel.Dahili3,
                        //DogumTarihi = personel.DogumTarihi,
                        //Gorevi = personel.Gorevi,
                        Ilcesi = nakitBagisci.Ilcesi,
                        Ili = nakitBagisci.Ili,
                        Kurumu = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI,
                        //Kutlama = personel.Kutlama,
                        KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT,
                        Soyadi = nakitBagisci.Soyadi,
                        //Unvani = personel.Unvani,
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
                //Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "IsBilgileri");
                var tasinmazBagisci = _unitOfWork.TasinmazBagisci.GetFirstOrDefault(u => u.Id == katilimciId);
                Katilimci katilimci = new Katilimci()
                {
                    Id = katilimciId==null?0:katilimciId.Value,
                    Adi = tasinmazBagisci.Adi,
                    Aciklama = tasinmazBagisci.Aciklama,
                    //Adres = tasinmazBagisci.Adres,
                    //Dahili1 = personel.Dahili1,
                    //Dahili2 = personel.Dahili2,
                    //Dahili3 = personel.Dahili3,
                    //DogumTarihi = personel.DogumTarihi,
                    //Gorevi = personel.Gorevi,
                    //Ilcesi = personel.Ilcesi,
                    //Ili = tasinmazBagisci.Ili,
                    Kurumu = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI,
                    //Kutlama = personel.Kutlama,
                    KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT,
                    Soyadi = tasinmazBagisci.Soyadi,
                    //Unvani = personel.Unvani,
                };
                retVal = katilimci;

            }
            return retVal;
        }
        //public Katilimci GetKatilimci(int? katilimciId, int katilimciTipi)
        //{
        //    Katilimci retVal = new();
        //    if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_DIS_INT)
        //    {

        //        //Kisi kisi = _unitOfWork.Kisi.GetFirstOrDefault(u => u.Id == katilimciId);
        //        Kisi kisi = _unitOfWork.Kisi.IncludeThis(katilimciId);
        //        if (kisi == null)
        //        {
        //            retVal = new Katilimci(); 
        //        }
        //        else{
        //            Katilimci katilimci = new Katilimci()
        //            {
        //                Id = kisi.Id,
        //                Adi = kisi.Adi,
        //                Aciklama = kisi.Aciklama,
        //                Adres = kisi.Adres,
        //                Dahili1 = kisi.Dahili1,
        //                Dahili2 = kisi.Dahili2,
        //                Dahili3 = kisi.Dahili3,
        //                DogumTarihi = kisi.DogumTarihi,
        //                Gorevi = (kisi.MTSKurumGorevs == null) || (kisi.MTSKurumGorevs.Count == 0) ? string.Empty : kisi.MTSKurumGorevs[0].MTSGorevTanim.Adi,
        //                Ilcesi = kisi.Ilcesi,
        //                Ili = kisi.Ili,
        //                Kurumu = (kisi.MTSKurumGorevs == null) || (kisi.MTSKurumGorevs.Count == 0) ? string.Empty : kisi.MTSKurumGorevs[0].MTSKurumTanim.Adi,
        //                Kutlama = kisi.Kutlama,
        //                RandevuKisiti = kisi.RandevuKisiti,
        //                KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_DIS_INT,
        //                Soyadi = kisi.Soyadi,
        //                Unvani = kisi.Unvani,
        //            };
        //            retVal =  katilimci;
        //        }
        //    }
        //    else if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_IC_INT)
        //    {
        //        //Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "IsBilgileri");
        //        var personel = _unitOfWork.IsBilgileri.GetFirstOrDefault(u => u.Id == katilimciId, includeProperties: "Personel");
        //        if (personel == null)
        //        {
        //            retVal = new Katilimci();
        //        }
        //        else
        //        {
        //            Katilimci katilimci = new Katilimci()
        //            {
        //                Id = katilimciId == null ? 0 : katilimciId.Value,
        //                Adi = personel.Personel.Adi,
        //                Aciklama = personel.Personel.Aciklama,
        //                //Adres = personel.Adres,
        //                //Dahili1 = personel.Dahili1,
        //                //Dahili2 = personel.Dahili2,
        //                //Dahili3 = personel.Dahili3,
        //                //DogumTarihi = personel.DogumTarihi,
        //                //Gorevi = personel.Gorevi,
        //                //Ilcesi = personel.Ilcesi,
        //                //Ili = personel.Ili,
        //                Kurumu = ProjectConstants.FAALIYET_KATILIMCI_IC,
        //                //Kutlama = personel.Kutlama,
        //                KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_IC_INT,
        //                Soyadi = personel.Personel.Soyadi,
        //                //Unvani = personel.Unvani,
        //            };
        //            retVal = katilimci;
        //        }
        //    }
        //    else if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT)
        //    {
        //        var nakitBagisci = _unitOfWork.NakitBagisci.GetFirstOrDefault(u => u.Id == katilimciId);
        //        if (nakitBagisci != null)
        //        {
        //            Katilimci katilimci = new Katilimci()
        //            {
        //                Id = katilimciId == null ? 0 : katilimciId.Value,
        //                Adi = nakitBagisci.Adi,
        //                Aciklama = nakitBagisci.Aciklama,
        //                Adres = nakitBagisci.Adres,
        //                //Dahili1 = personel.Dahili1,
        //                //Dahili2 = personel.Dahili2,
        //                //Dahili3 = personel.Dahili3,
        //                //DogumTarihi = personel.DogumTarihi,
        //                //Gorevi = personel.Gorevi,
        //                Ilcesi = nakitBagisci.Ilcesi,
        //                Ili = nakitBagisci.Ili,
        //                Kurumu = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI,
        //                //Kutlama = personel.Kutlama,
        //                KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT,
        //                Soyadi = nakitBagisci.Soyadi,
        //                //Unvani = personel.Unvani,
        //            };
        //            retVal = katilimci;
        //        }
        //        else
        //        {
        //            return new Katilimci();
        //        }

        //    }
        //    else if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT)
        //    {
        //        //Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "IsBilgileri");
        //        var tasinmazBagisci = _unitOfWork.TasinmazBagisci.GetFirstOrDefault(u => u.Id == katilimciId);
        //        if (tasinmazBagisci == null)
        //        {
        //            retVal = new Katilimci();
        //        }
        //        else
        //        {

        //            Katilimci katilimci = new Katilimci()
        //            {
        //                Id = katilimciId == null ? 0 : katilimciId.Value,
        //                Adi = tasinmazBagisci.Adi,
        //                Aciklama = tasinmazBagisci.Aciklama,
        //                //Adres = tasinmazBagisci.Adres,
        //                //Dahili1 = personel.Dahili1,
        //                //Dahili2 = personel.Dahili2,
        //                //Dahili3 = personel.Dahili3,
        //                //DogumTarihi = personel.DogumTarihi,
        //                //Gorevi = personel.Gorevi,
        //                //Ilcesi = personel.Ilcesi,
        //                //Ili = tasinmazBagisci.Ili,
        //                Kurumu = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI,
        //                //Kutlama = personel.Kutlama,
        //                KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT,
        //                Soyadi = tasinmazBagisci.Soyadi,
        //                //Unvani = personel.Unvani,
        //            };
        //            retVal = katilimci;
        //        }
        //    }
        //    return retVal;
        //}

        public List<FaaliyetKatilim> FillKatilimciIntoFaaliyetKatilim(List<FaaliyetKatilim> faaliyetKatilimList)
        {
            foreach (var item in faaliyetKatilimList)
            {
                Katilimci katilimci = GetKatilimci(item.KatilimciId, item.KatilimciTipi);
                item.Katilimci = katilimci;
            }
            return faaliyetKatilimList;
        }
        public List<FaaliyetKatilim> GetAllKatilimci(List<FaaliyetKatilim> faaliyetKatilimList)
        {
            foreach (var item in faaliyetKatilimList)
            {
                Katilimci katilimci = GetKatilimci( item.KatilimciId, item.KatilimciTipi);
                item.Katilimci = katilimci;
            }
            return faaliyetKatilimList;
        }
        public List<Katilimci> GetAllFaaliyetWithKatilimci(List<Faaliyet> faaliyetList)
        {
            List<Katilimci> katilimciList= new List<Katilimci>();
            foreach (var item in faaliyetList)
            {
                if (item!=null && item.FaaliyetKatilims!=null)
                {
                    foreach (var fk in item.FaaliyetKatilims)
                    {
                        Katilimci katilimci = GetKatilimci( fk.KatilimciId, fk.KatilimciTipi);
                        katilimciList.Add(katilimci);
                        fk.Katilimci = katilimci;
                    } 
                }
            }
            return katilimciList;
        }
        public List<Katilimci> GetAllKatilimci()
        {
            var kisiler = _unitOfWork.Kisi.IncludeIt();
            var personeller = _unitOfWork.Personel.GetAll();
            //var nakitBagiscilar = _unitOfWork.NakitBagisci.GetByFilter(m=> !string.IsNullOrEmpty(m.Adi) || !m.Adi.Equals("BİLİNMEYEN"));
            var tasinmazBagiscilar = _unitOfWork.TasinmazBagisci.GetAll();

            List<Katilimci> KatilimciKisiList = kisiler.Select(a => new Katilimci() 
            { 
                Id = a.Id,
                Adi = a.Adi, 
                Soyadi = a.Soyadi,
                KatilimciTipi= ProjectConstants.FAALIYET_KATILIMCI_DIS_INT,
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

            List<Katilimci> KatilimciPersonelList = personeller.Select(a => new Katilimci()
            { 
                Id= a.Id,
                Adi = a.Adi,
                Soyadi = a.Soyadi,
                KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_IC_INT,
                Kurumu = ProjectConstants.FAALIYET_KATILIMCI_IC,
                Aciklama = a.Aciklama,

            }).ToList();
            List<Katilimci> KatilimciTasinmazBagisciList = tasinmazBagiscilar.Select(a => new Katilimci()
            {
                Id = a.Id,
                Adi = a.Adi,
                Soyadi = a.Soyadi,
                KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT,
                Kurumu = ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI,
                TCKimlikNo = a.TCKimlikNo,
                Aciklama = a.Aciklama,
                Adres= a.Adres,

            }).ToList();

            //List<Katilimci> KatilimciNakitBagisciList = nakitBagiscilar.Select(a => new Katilimci()
            //{
            //    Id = a.Id,
            //    Adi = a.Adi,
            //    Soyadi = a.Soyadi,
            //    KatilimciTipi = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT,
            //    Kurumu = ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI,
            //    TCKimlikNo = a.TCKimlikNo,
            //    Aciklama = a.Aciklama,
            //    Adres = a.Adres

            //}).ToList();
            var allKatilimcilar = new List<Katilimci>();
            allKatilimcilar.AddRange(KatilimciKisiList);
            allKatilimcilar.AddRange(KatilimciPersonelList);
            allKatilimcilar.AddRange(KatilimciTasinmazBagisciList);
            //allKatilimcilar.AddRange(KatilimciNakitBagisciList);

            return allKatilimcilar;
  
        }
        public List<Katilimci> GetAllAramaGorusmeWithKatilimci()
        {
            var list = _unitOfWork.AramaGorusme.GetAll().ToList();  
            List<Katilimci> katilimciList = new List<Katilimci>();
            foreach (var item in list)
            {
                if (item != null )
                {
                        Katilimci katilimci = GetKatilimci( item.ArayanId, item.KatilimciTipi);
                        katilimciList.Add(katilimci);
                }
            }
            return katilimciList;
        }
    }

}