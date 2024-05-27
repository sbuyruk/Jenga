var dataTable;

function loadDataTable() {
    var baslangicTarihi;
    //if (bastar == null) {
    //    let d = new Date();
    //    d.setMonth(d.getMonth() - 3)
    //    baslangicTarihi = d.toLocaleDateString("tr-TR");
    //} else {
    //    baslangicTarihi= bastar ;
    //}
    $.fn.dataTable.moment('DD.MM.YYYY HH:mm'); // Date format
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Faaliyet/GetFaaliyetList",
            //"data": {
            //    BaslangicTarihi: baslangicTarihi,
            //},
        },
        "columns": [
            { "data": "faaliyet.id", "width": "4%" },
            { "data": "faaliyet.baslangicTarihi", "width": "10%" },
            { "data": "faaliyet.bitisTarihi", "width": "10%" },
            { "data": "faaliyet.faaliyetYeriStr", "width": "10%" },
            { "data": "faaliyet.faaliyetKonusu", "width": "20%" },
            //{ "data": "faaliyet.faaliyetTipi", "width": "8%" },
            //{ "data": "faaliyet.faaliyetAmaci", "width": "7%" },
            //{ "data": "faaliyet.faaliyetDurumu", "width": "8%" },
            { "data": "kisiler", "width": "28%" },
            {
                "data": "faaliyet.id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="http://tskgv-portal/Sayfalar/FaaliyetGirisi.aspx?FaaliyetId=${data}" 
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Düzenle</a>
					</div>
                        `
                },
            },
        ],
        dom: 'Bfrtip',
        destroy:true,
        buttons: [
            {
                extend: 'print',
                exportOptions: {
                    columns: ':visible'
                }
            },
            {
                extend: 'excel',
                exportOptions: {
                    columns: ':visible'
                }
            },
            {
                extend: 'pdf',
                exportOptions: {
                    columns: ':visible'
                }
            },
            {
                extend: 'copy',
                exportOptions: {
                    columns: ':visible'
                }
            },
            , 'pageLength', "colvis"
        ],
    });

}
function loadDataTableAcik() {
    
    $.fn.dataTable.moment('DD.MM.YYYY HH:mm'); // Date format
    dataTable = $('#tblDataAcik').DataTable({
        "ajax": {
            "url": "/Admin/Faaliyet/GetAllAcikTarihliFaaliyetList",
           
        },
        "columns": [
            { "data": "faaliyet.id", "width": "4%" },
            { "data": "faaliyet.baslangicTarihi", "width": "10%" },
            { "data": "faaliyet.bitisTarihi", "width": "10%" },
            { "data": "faaliyet.faaliyetYeriStr", "width": "10%" },
            { "data": "faaliyet.faaliyetKonusu", "width": "20%" },
            //{ "data": "faaliyet.faaliyetTipi", "width": "8%" },
            //{ "data": "faaliyet.faaliyetAmaci", "width": "7%" },
            //{ "data": "faaliyet.faaliyetDurumu", "width": "8%" },
            { "data": "kisiler", "width": "28%" },
            {
                "data": "faaliyet.id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="http://tskgv-portal/Sayfalar/FaaliyetGirisi.aspx?FaaliyetId=${data}" 
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Düzenle</a>
					</div>
                        `
                },
            },
        ],
        dom: 'Bfrtip',
        destroy: true,
        buttons: [
            {
                extend: 'print',
                exportOptions: {
                    columns: ':visible'
                }
            },
            {
                extend: 'excel',
                exportOptions: {
                    columns: ':visible'
                }
            },
            {
                extend: 'pdf',
                exportOptions: {
                    columns: ':visible'
                }
            },
            {
                extend: 'copy',
                exportOptions: {
                    columns: ':visible'
                }
            },
            , 'pageLength', "colvis"
        ],
    });

}
function LoadCalendar(resmiTatiller, vakifToplantilari) {
    const date = new Date();

    var day = ""+date.getDate();
    var month = ""+ (date.getMonth() + 1);
    var year = ""+date.getFullYear();
    if (month.length < 2)
        month = '0' + month;
    if (day.length < 2)
        day = '0' + day;
    // This arrangement can be altered based on how we want the date's format to appear.
    let currentDate = [year, month, day].join('-');

    var calenderView = 'dayGridMonth';
    var viewDate = currentDate;

    var calendarEl = document.getElementById('calendar');
    var calendar = new FullCalendar.Calendar(calendarEl, {
        locale: 'tr',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'multiMonthYear,dayGridMonth,timeGridWeek,timeGridDay,listWeek'
        },
        dayMaxEvents: true,
        initialView: calenderView,
        initialDate: viewDate,
        eventDidMount: function (info) {
            const dt = info.event.start;
            const et = info.event.end;

            const padL = (nr, len = 2, chr = `0`) => `${nr}`.padStart(2, chr);

            var startDateTime = `${padL(dt.getDate())}.${padL(dt.getMonth() + 1)}.${dt.getFullYear()} ${padL(dt.getHours())}:${padL(dt.getMinutes())}`;
            var endDateTime = "";
            if (et != null)
                endDateTime = `${padL(et.getDate())}.${padL(et.getMonth() + 1)}.${et.getFullYear()} ${padL(et.getHours())}:${padL(et.getMinutes())}`;
            $(info.el).popover({
                title: info.event.title,
                placement: 'bottom',
                trigger: 'hover',
                content: startDateTime + " - " + endDateTime + "\n" + "\n" + info.event.extendedProps.description,
                container: 'body',
                
            });
        },
        events: {
            url: '/Admin/Faaliyet/GetEvents',
            method: 'POST',
            extraParams: function () {
                return {
                    resmiTatiller: resmiTatiller,
                    vakifToplantilari: vakifToplantilari,
                };
            }
        }
        

    });
    calendar.render();
}
function Delete(url) {
    Swal.fire({
        title: 'Silmek istediğinize emin misiniz?',
        text: "Silinen kayıt geri getirilemez!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Evet, sil!',
        cancelButtonText: 'Vazgeç'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'POST',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}
