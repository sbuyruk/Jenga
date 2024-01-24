var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/MTSKurumTanim/GetAll"
        },
        "columns": [
            { "data": "adi", "width": "20%" },
            { "data": "kisaAdi", "width": "20%" },
            { "data": "aciklama", "width": "10%" },
            {
                "data": "id",
                "width": "15%",
                "render": function (data) {
                    return `
                        <div class="form-group" >                        
                            <button type="button" class="form-control btn btn-outline-info" onclick=OpenModalKisi(${data});>
                                Kurumdaki Kişiler
                            </button>
					    </div>
                        `
                },
            },
            {
                "data": "id",
                "width": "15%",
                "render": function (data) {
                    return `
                        <div class="form-group" >                        
                            <a class="btn btn-outline-warning" onclick=OpenModalKurum(${data});> Kurumdaki Görevler </a>
					    </div>
                        `
                },
            },
            {
                "data": "id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="/Admin/MTSKurumTanim/Edit?id=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/MTSKurumTanim/Delete/${data}')
                        class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Sil</a>
					</div>
                        `
                },
            },
        ],
        dom: 'Bfrtip',
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
function loadModalDataTable(mtsKurumTanimId) {
    if (jQuery.fn.DataTable.isDataTable('#tblDataModal')) {
        jQuery('#tblDataModal').DataTable().destroy();
    }
    jQuery('#tblDataModal tbody').empty();
    dataTable = $('#tblDataModal').DataTable({
        "ajax": {
            "url": "/Admin/MTSGorevTanim/GetAllByMTSKurumTanimId",
            type: 'GET', 
            data: {
                mtsKurumTanimId: mtsKurumTanimId,
            },
        },
        "columns": [
            {
                "data": "Id",
                "width": "60%",
                "render": function (data, type, row, meta) {
                    return `
                        <div class="form-group" >                        
                            <p><a class="link-opacity-50" href="/Admin/MTSGorevTanim/Edit?id=${row.id}">${row.adi}</a></p>
					    </div>
                        `
                },
            },
            { "data": "kisaAdi", "width": "40%" },
        ],
        destroy: true,
        dom: 'frtip',
      
    });

}
function loadModalDataTableKisi(mtsKurumTanimId) {
    if (jQuery.fn.DataTable.isDataTable('#tblDataModalKisi')) {
        jQuery('#tblDataModalKisi').DataTable().destroy();
    }
    jQuery('#tblDataModalKisi tbody').empty();
    dataTable = $('#tblDataModalKisi').DataTable({
        "ajax": {
            "url": "/Admin/MTSKurumGorev/GetAllByMTSKurumTanimId",
            type: 'GET',
            data: {
                mtsKurumTanimId: mtsKurumTanimId,
            },
        },
        "columns": [
            {
                "data": "Kisi.Adi",
                "width": "60%",
                "render": function (data, type, row, meta) {
                    return `
                        <div class="form-group" >                        
                            <p><a class="link-opacity-50" href="/Admin/Kisi/Edit?id=${row.Kisi.Id}">${row.Kisi.Adi} ${row.Kisi.Soyadi}</a></p>
					    </div>
                        `
                },
            },
            { "data": "MTSGorevTanim.Adi", "width": "40%" },
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
