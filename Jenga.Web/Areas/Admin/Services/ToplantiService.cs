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
    public class ToplantiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FaaliyetService _faaliyetService;

        public ToplantiService(IUnitOfWork unitOfWork,FaaliyetService faaliyetService)
        {
            _unitOfWork = unitOfWork;
            _faaliyetService = faaliyetService;
        }

        internal void GetVakifToplantiList(DateTime startDate, DateTime endDate, List<CalendarEvent> events)
        {
            //var list = _unitOfWork.Toplanti.GetByFilter(a => startDate <= a.BaslangicTarihi && a.BitisTarihi <= endDate, includeProperties:"ToplantiKatilim");

            //var toplantiList = _unitOfWork.Toplanti.GetAll();//GetByFilter(a=>a.BaslangicTarihi>=baslangicTarihi);

            var list = GetToplantiList(ProjectConstants.GENELMUDUR_PERSONELID);
            if (list!=null)
            {
                foreach (var item in list)
                {
                    CalendarEvent anEvent = new CalendarEvent();
                    anEvent.state = ProjectConstants.FAALIYET_DURUMU_ONAYLANDI_INT.ToString();

                    anEvent.id = item.Id;
                    anEvent.purpose = ProjectConstants.FAALIYET_AMACI_VAKIF_TOPLANISI_INT.ToString();
                    anEvent.title = item.ToplantiKonusu;
                    anEvent.start = string.Format("{0:s}", item.BaslangicTarihi);
                    anEvent.end = string.Format("{0:s}", item.BitisTarihi);
                    anEvent.url = "http://tskgv-portal/Sayfalar/ToplantiListesi.aspx?SecilenToplantiId=" + anEvent.id;
                    anEvent.startEditable = false;
                    anEvent.description = item.Aciklama;
                    _faaliyetService.RenkBelirle(anEvent);
                    //item.className = "iptal-edildi";
                    events.Add(item: anEvent);
                } 
            }
        }
        private List<Toplanti> GetToplantiList(int kisiId)
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