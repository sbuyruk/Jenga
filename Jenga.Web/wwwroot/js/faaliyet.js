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

    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Faaliyet/GetAllFaaliyetsWithKatilimci",
            "data": {
                BaslangicTarihi: baslangicTarihi,
            },
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
            { "data": "katilimcilar", "width": "28%" },
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
