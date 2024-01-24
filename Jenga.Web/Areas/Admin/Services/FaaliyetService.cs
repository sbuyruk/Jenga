using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Jenga.Utility;


namespace Jenga.Web.Areas.Admin.Services
{
    public class FaaliyetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FaaliyetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Katilimci GetKatilimci(int faaliyetId, int katilimciId, int katilimciTipi)
        {
            Katilimci retVal = new();
            if (katilimciTipi == ProjectConstants.RANDEVU_KATILIMCI_DIS_INT)
            {

                Kisi kisi = _unitOfWork.Kisi.GetFirstOrDefault(u => u.Id == katilimciId);
                Katilimci katilimci = new Katilimci()
                {
                    FaaliyetId = faaliyetId,
                    Id = kisi.Id,
                    Adi = kisi.Adi,
                    Aciklama = kisi.Aciklama,
                    Adres = kisi.Adres,
                    Dahili1 = kisi.Dahili1,
                    Dahili2 = kisi.Dahili2,
                    Dahili3 = kisi.Dahili3,
                    DogumTarihi = kisi.DogumTarihi,
                    Gorevi = kisi.Gorevi,
                    Ilcesi = kisi.Ilcesi,
                    Ili = kisi.Ili,
                    Kurumu = kisi.Kurumu,
                    Kutlama = kisi.Kutlama,
                    KatilimciTipi = ProjectConstants.RANDEVU_KATILIMCI_DIS_INT,
                    Soyadi = kisi.Soyadi,
                    Unvani = kisi.Unvani,
                };
                retVal = katilimci;
            }
            else if (katilimciTipi == ProjectConstants.RANDEVU_KATILIMCI_IC_INT)
            {
                //Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "IsBilgileri");
                var personel = _unitOfWork.IsBilgileri.GetFirstOrDefault(u => u.Id == katilimciId, includeProperties: "Personel");
                Katilimci katilimci = new Katilimci()
                {
                    FaaliyetId = faaliyetId,
                    Id = katilimciId,
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
                    Kurumu = ProjectConstants.RANDEVU_KATILIMCI_IC,
                    //Kutlama = personel.Kutlama,
                    KatilimciTipi = ProjectConstants.RANDEVU_KATILIMCI_IC_INT,
                    Soyadi = personel.Personel.Soyadi,
                    //Unvani = personel.Unvani,
                };
                retVal = katilimci;

            }
            else if (katilimciTipi == ProjectConstants.RANDEVU_KATILIMCI_NAKITBAGISCI_INT)
            {
                var nakitBagisci = _unitOfWork.NakitBagisci.GetFirstOrDefault(u => u.Id == katilimciId);
                if (nakitBagisci != null)
                {
                    Katilimci katilimci = new Katilimci()
                    {
                        FaaliyetId = faaliyetId,
                        Id = katilimciId,
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
                        Kurumu = ProjectConstants.RANDEVU_KATILIMCI_NAKITBAGISCI,
                        //Kutlama = personel.Kutlama,
                        KatilimciTipi = ProjectConstants.RANDEVU_KATILIMCI_NAKITBAGISCI_INT,
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
            else if (katilimciTipi == ProjectConstants.RANDEVU_KATILIMCI_TASINMAZBAGISCI_INT)
            {
                //Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "IsBilgileri");
                var tasinmazBagisci = _unitOfWork.TasinmazBagisci.GetFirstOrDefault(u => u.Id == katilimciId);
                Katilimci katilimci = new Katilimci()
                {
                    FaaliyetId = faaliyetId,
                    Id = katilimciId,
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
                    Kurumu = ProjectConstants.RANDEVU_KATILIMCI_TASINMAZBAGISCI,
                    //Kutlama = personel.Kutlama,
                    KatilimciTipi = ProjectConstants.RANDEVU_KATILIMCI_TASINMAZBAGISCI_INT,
                    Soyadi = tasinmazBagisci.Soyadi,
                    //Unvani = personel.Unvani,
                };
                retVal = katilimci;

            }
            return retVal;
        }


        public List<FaaliyetKatilim> FillKatilimciIntoFaaliyetKatilim(List<FaaliyetKatilim> faaliyetKatilimList)
        {
            foreach (var item in faaliyetKatilimList)
            {
                Katilimci katilimci = GetKatilimci(item.FaaliyetId,item.KatilimciId, item.KatilimciTipi);
                item.Katilimci = katilimci;
            }
            return faaliyetKatilimList;
        }
        public List<FaaliyetKatilim> GetAllKatilimci(List<FaaliyetKatilim> faaliyetKatilimList)
        {
            foreach (var item in faaliyetKatilimList)
            {
                Katilimci katilimci = GetKatilimci(item.FaaliyetId, item.KatilimciId, item.KatilimciTipi);
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
                        Katilimci katilimci = GetKatilimci(fk.FaaliyetId, fk.KatilimciId, fk.KatilimciTipi);
                        katilimciList.Add(katilimci);
                        fk.Katilimci = katilimci;
                    } 
                }
            }
            return katilimciList;
        }
    }

}