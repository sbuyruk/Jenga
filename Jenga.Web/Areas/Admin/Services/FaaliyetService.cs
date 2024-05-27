using Jenga.DataAccess.Repository;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Jenga.Models.TYS;
using Jenga.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Emit;

namespace Jenga.Web.Areas.Admin.Services
{
    public class FaaliyetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FaaliyetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        internal void GetFaaliyetList(DateTime startDate, DateTime endDate, List<CalendarEvent> events)
        {
            var faaliyetList = _unitOfWork.Faaliyet.GetByFilter(a => startDate <= a.BaslangicTarihi && a.BitisTarihi <= endDate);
            foreach (var faaliyet in faaliyetList)
            {
                CalendarEvent anEvent = new CalendarEvent();
                anEvent.state = faaliyet.FaaliyetDurumu.ToString();

                anEvent.id = faaliyet.Id;
                int faaliyetAmaci = faaliyet.FaaliyetAmaci;
                anEvent.purpose = faaliyetAmaci.ToString();
                anEvent.title = faaliyet.FaaliyetKonusu;
                //item.description = item.title;
                anEvent.start = string.Format("{0:s}", faaliyet.BaslangicTarihi);
                anEvent.end = string.Format("{0:s}", faaliyet.BitisTarihi);
                anEvent.url = "http://tskgv-portal/Sayfalar/FaaliyetGirisi.aspx?DestinationApp=Duzenle&FaaliyetId=" + anEvent.id;
                anEvent.allDay = faaliyet.TumGun.Value;
                anEvent.startEditable = true;
                anEvent.description = faaliyet.Aciklama;
                RenkBelirle(anEvent);
                //item.className = "iptal-edildi";
                events.Add(item: anEvent);
            }
        }
        
        internal void GetResmiTatilList(DateTime startDate, DateTime endDate, List<CalendarEvent> events)
        {
            var resmiTatilList = _unitOfWork.ResmiTatil.GetAll();



            foreach (var resmiTatil in resmiTatilList)
            {
                if (resmiTatil.BaslamaTarihi.Year < 1900)
                {
                    for (int i = 0; i <= (endDate.Year - startDate.Year); i++)
                    {
                        string bassaat = resmiTatil.BaslamaTarihi.Hour + ":" + resmiTatil.BaslamaTarihi.Minute;
                        string bitsaat = resmiTatil.BitisTarihi.Hour + ":" + resmiTatil.BitisTarihi.Minute;

                        resmiTatil.BaslamaTarihi = new DateTime(startDate.Year + i, resmiTatil.BaslamaTarihi.Month, resmiTatil.BaslamaTarihi.Day).Add(new TimeSpan(resmiTatil.BaslamaTarihi.Hour, resmiTatil.BaslamaTarihi.Minute, 0));
                        resmiTatil.BitisTarihi = new DateTime(endDate.Year + i, resmiTatil.BitisTarihi.Month, resmiTatil.BitisTarihi.Day).Add(new TimeSpan(resmiTatil.BitisTarihi.Hour, resmiTatil.BitisTarihi.Minute, 0));

                        CalendarEvent item = new CalendarEvent();
                        item.state = ProjectConstants.FAALIYET_DURUMU_ONAYLANDI_INT.ToString();
                        item.id = 999;//999 önemli taşınamayan event
                        item.purpose = ProjectConstants.FAALIYET_AMACI_RESMITATIL_INT;
                        item.title = resmiTatil.Tatil;
                        item.description = resmiTatil.Tatil;
                        item.start = string.Format("{0:s}", resmiTatil.BaslamaTarihi);
                        item.end = string.Format("{0:s}", resmiTatil.BitisTarihi);
                        item.url = "";
                        if (resmiTatil.Yil < 1900)
                            item.allDay = true;
                        if (resmiTatil.BitisTarihi.Day > resmiTatil.BaslamaTarihi.Day)
                        {
                            item.allDay = false;
                        }

                        item.startEditable = false;

                        RenkBelirle(item);
                        events.Add(item);
                    }
                }
                else
                {
                    CalendarEvent item = new CalendarEvent();
                    item.state = ProjectConstants.FAALIYET_DURUMU_ONAYLANDI_INT.ToString();
                    item.id = 999;//999 önemli taşınamayan event
                    item.purpose = ProjectConstants.FAALIYET_AMACI_RESMITATIL_INT;
                    item.title = resmiTatil.Tatil;
                    item.description = resmiTatil.Tatil;
                    item.start = string.Format("{0:s}", resmiTatil.BaslamaTarihi);
                    item.end = string.Format("{0:s}", resmiTatil.BitisTarihi);
                    item.url = "";
                    item.allDay = true;
                    item.startEditable = false;

                    RenkBelirle(item);
                    events.Add(item);
                }


            }

        }
        public void RenkBelirle(CalendarEvent item)
        {
            //switch (item.purpose)
            //{
            //    case ProjectConstants.FAALIYET_AMACI_TOPLANTI_INT:
            //        {
            //            item.backgroundColor = Color.Orange.Name;
            //            item.textColor = Color.White.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_TOPLANTI;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_ZIYARET_INT:
            //        {
            //            item.backgroundColor = Color.Blue.Name;
            //            item.textColor = Color.White.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_ZIYARET;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_DAVET_INT:
            //        {
            //            item.backgroundColor = Color.Green.Name;
            //            item.textColor = Color.White.Name;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_YILDONUMU_INT:
            //        {
            //            item.backgroundColor = Color.Aqua.Name;
            //            item.textColor = Color.Black.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_YILDONUMU;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_DOGUMGUNU_INT:
            //        {
            //            item.backgroundColor = Color.Aquamarine.Name;
            //            item.textColor = Color.Black.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_DOGUMGUNU;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_OZELCALISMA_INT:
            //        {
            //            item.backgroundColor = Color.LightBlue.Name;
            //            item.textColor = Color.White.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_OZELCALISMA;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_IZIN_INT:
            //        {
            //            item.backgroundColor = Color.Yellow.Name;
            //            item.textColor = Color.Black.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_IZIN;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_RESMITATIL_INT:
            //        {
            //            item.backgroundColor = Color.MediumVioletRed.Name;
            //            item.textColor = Color.White.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_RESMITATIL;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_GORUSME_INT:
            //        {
            //            item.backgroundColor = Color.DeepSkyBlue.Name;
            //            item.textColor = Color.White.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_GORUSME;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_SEYAHAT_INT:
            //        {
            //            item.backgroundColor = Color.Coral.Name;
            //            item.textColor = Color.White.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_SEYAHAT;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_BILGI_INT:
            //        {
            //            item.backgroundColor = Color.DimGray.Name;
            //            item.textColor = Color.White.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_BILGI;
            //            break;
            //        }
            //    case ProjectConstants.FAALIYET_AMACI_VAKIF_TOPLANISI_INT:
            //        {
            //            item.backgroundColor = Color.Red.Name;
            //            item.textColor = Color.White.Name;
            //            item.purpose = ProjectConstants.FAALIYET_AMACI_VAKIF_TOPLANISI;
            //            break;
            //        }
            //    default:
            //        break;
            //}
            //if (item.state.Equals(ProjectConstants.FAALIYET_DURUMU_PLANLANDI_INT.ToString()))
            //{
            //    item.backgroundColor = Color.LightGray.Name;
            //    item.textColor = Color.Black.Name;
            //}
            switch (item.purpose)
            {
                case ProjectConstants.FAALIYET_AMACI_TOPLANTI_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_TOPLANTI_CLASS + " legand";
                        item.purpose = ProjectConstants.FAALIYET_AMACI_TOPLANTI;
                        //item.color = Color.DarkOrange.Name;
                        item.borderColor = Color.DarkOrange.Name;
                        item.backgroundColor = Color.DarkOrange.Name;
                        item.textColor = Color.Black.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_ZIYARET_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_ZIYARET_CLASS + " legand";
                        item.purpose = ProjectConstants.FAALIYET_AMACI_ZIYARET;
                        item.borderColor = Color.Red.Name;
                        item.backgroundColor = Color.Red.Name;
                        item.textColor = Color.White.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_DAVET_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_DAVET_CLASS;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_DAVET;
                        item.borderColor = Color.Green.Name;
                        item.backgroundColor = Color.Green.Name;
                        item.textColor = Color.White.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_YILDONUMU_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_YILDONUMU_CLASS;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_YILDONUMU;
                        item.borderColor = Color.Aqua.Name;
                        item.backgroundColor = Color.Aqua.Name;
                        item.textColor = Color.Black.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_DOGUMGUNU_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_DOGUMGUNU_CLASS;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_DOGUMGUNU;
                        item.borderColor = Color.Aquamarine.Name;
                        item.backgroundColor = Color.Aquamarine.Name;
                        item.textColor = Color.Black.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_OZELCALISMA_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_OZELCALISMA_CLASS;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_OZELCALISMA;
                        item.borderColor = Color.LightBlue.Name;
                        item.backgroundColor = Color.LightBlue.Name;
                        item.textColor = Color.Black.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_IZIN_INT:
                    {
                        item.purpose = ProjectConstants.FAALIYET_AMACI_IZIN;
                        item.borderColor = Color.Yellow.Name;
                        item.backgroundColor = Color.Yellow.Name;
                        item.textColor = Color.Black.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_RESMITATIL_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_RESMITATIL_CLASS;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_RESMITATIL;
                        item.borderColor = Color.Purple.Name;
                        item.backgroundColor = Color.Purple.Name;
                        item.textColor = Color.White.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_GORUSME_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_GORUSME_CLASS;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_GORUSME;
                        item.borderColor = Color.DeepSkyBlue.Name;
                        item.backgroundColor = Color.DeepSkyBlue.Name;
                        item.textColor = Color.Black.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_SEYAHAT_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_SEYAHAT_CLASS;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_SEYAHAT;
                        item.borderColor = Color.Coral.Name;
                        item.backgroundColor = Color.Coral.Name;
                        item.textColor = Color.Black.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_BILGI_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_BILGI_CLASS;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_BILGI;
                        item.borderColor = Color.Silver.Name;
                        item.backgroundColor = Color.Silver.Name;
                        item.textColor = Color.Black.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_VAKIF_TOPLANISI_INT:
                    {
                        //item.className = ProjectConstants.FAALIYET_AMACI_VAKIF_TOPLANISI_CLASS;
                        item.borderColor = Color.Red.Name;
                        item.backgroundColor = Color.Red.Name;
                        item.textColor = Color.White.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_VAKIF_TOPLANISI;
                        break;
                    }
                default:
                    break;
            }
            if (item.state.Equals(ProjectConstants.FAALIYET_DURUMU_PLANLANDI_INT.ToString()))
            {
                item.className = ProjectConstants.FAALIYET_DURUMU_PLANLANDI_CLASS;
                item.textColor = Color.Blue.Name;
            }
            if (item.state.Equals(ProjectConstants.FAALIYET_DURUMU_IPTALEDILDI_INT.ToString()))
            {
                item.className = ProjectConstants.FAALIYET_DURUMU_IPTALEDILDI_CLASS;
            }
        }
        public List<Toplanti> GetToplantiList(int kisiId)
        {

            var katilimciToplantilar = _unitOfWork.ToplantiKatilim.GetAll()
                .Where(tk => tk.KatilimciId == kisiId)
                .Select(tk => tk.ToplantiId)
                .ToList();


            var toplantiList = _unitOfWork.Toplanti.GetAll()
                .Where(t => katilimciToplantilar.Contains(t.Id))
                .ToList();

            return toplantiList;
        }
    }

}