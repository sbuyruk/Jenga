var dataTable;


$(document).ready(function () {
    loadDataTable(null);

});
function loadDataTable(bastar) {
    var baslangicTarihi;
    if (bastar == null) {
        let d = new Date();
        d.setMonth(d.getMonth() - 3)
        baslangicTarihi = d.toLocaleDateString("tr-TR");
    } else {
        baslangicTarihi= bastar ;
    }
    $.fn.dataTable.moment('DD.MM.YYYY HH:mm'); // Date format

    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/AramaGorusme/GetAllAramaGorusmeWithKatilimci",
            "data": {
                BaslangicTarihi: baslangicTarihi,
            },
        },
        "columns": [
            { "data": "aramaGorusme.id", "width": "4%" },
            {
                "data": "katilimci",
                "width": "15%",
                "render": function (data) {
                    var retval = "";
                    if (data) {
                        retval = (data.adi == null ? '' : data.adi) + ' ' + (data.soyadi == null ? '' : data.soyadi)  ;

                    }
                    return retval;
                },
            },
            { "data": "aramaGorusme.tarih", "width": "10%" },
            { "data": "aramaGorusme.konu", "width": "20%" },
            {
                "data": "katilimci",
                "width": "25%",
                "render": function (data) {
                    var retval = "";
                    if (data) {
                        retval = (data.kurumu == null ? '' : data.kurumu) + '  ' + (data.gorevi == null ? '' : data.gorevi).trim();

                    }
                    return retval;
                },
            },
            {
                "data": "aramaGorusme.faaliyetId",
                "width": "15%",
                "render": function (data, type, row, meta) {
                    if (data ==0) {
                        return''
                    }
                    else {
                        var acik = row.aramaGorusme.faaliyet.acikTarih ? ' (Açık) ' : '';
                        var retval = `
                        <div class="btn-group" role="group">
                            <a href="http://tskgv-portal/Sayfalar/FaaliyetGirisi.aspx?FaaliyetId=${data}&"
                            class="btn btn-warning mx-2"> <i class="bi bi-pencil-square"></i> Faaliyet ` + acik +`</a>
					    </div>
                        `;
                        return retval;
                    }
                },
            },
            {
                "data": "aramaGorusme.id",
                "width": "10%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="http://tskgv-portal/Sayfalar/AramaGorusmeGirisi.aspx?AramaGorusmeId=${data}" 
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Düzenle</a>
					</div>
                        `
                },
            },
        ],
        dom: 'Bfrtip',
        order: [
            [2, 'desc']  // Sort by the third column (date time) in descending order
        ],
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
function loadModalDataTableKisi() {
    if (jQuery.fn.DataTable.isDataTable('#tblDataModalKatilimci')) {
        jQuery('#tblDataModalKatilimci').DataTable().destroy();
    }
    jQuery('#tblDataModalKatilimci tbody').empty();
    dataTable = $('#tblDataModalKatilimci').DataTable({
        "ajax": {
            "url": "/Admin/AramaGorusme/GetAllKatilimci",
            type: 'GET',

        },
        "columns": [
            {
                "data": "adi",
                "width": "60%",
                "render": function (data, type, row, meta) {
                    return `
                        <div class="form-group" >                        
                            <p><a class="link-opacity-50" href="/Admin/Kisi/Edit?id=${row.id}">${row.adi} ${row.soyadi}</a></p>
					    </div>
                        `
                },
            },
            { "data": "kurumu", "width": "40%" },
            {
                "data": "arayanId",
                "width": "60%",
                "render": function (data, type, row, meta) {
                    var retval =
                        `
                        <div class="form-group" >                        
                            <p><a class="btn btn-primary" href="/Admin/AramaGorusme/Create?katilimciId=${row.id}&KatilimciTipi=${row.katilimciTipi}">SEÇ</a></p>
					    </div>
                        `;
                return retval;
                },
            },
        ],
        destroy: true,
        dom: 'frtip',

    });

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
