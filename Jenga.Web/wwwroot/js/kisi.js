var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Kisi/GetAll"
        },
        "columns": [
            { data: "id" },
            { data: "adi" },
            { data: "soyadi" },
            { data: "kurumu" },
            { data: "unvani" },
            { data: "gorevi" },
            { data: "telefon1", "width": "14%" },
            {
                "data": "id",
                "width": "15%",
                "render": function (data) {
                    return `
                        <div class="form-group" role="group">
                        <a href="/Admin/Kisi/Edit?id=${data}"
                        class="btn btn-warning "> <i class="bi bi-pencil-square"></i> Arama/Görüşme</a>
					</div>
                        `
                },
            },
            {
                "data": "id",
                "width": "15%",
                "render": function (data) {
                    return `
                        <div class="form-group" role="group">
                        <a href="/Admin/Kisi/Edit?id=${data}"
                        class="btn btn-info "> <i class="bi bi-pencil-square"></i> Kişi Kartı</a>
					</div>
                        `
                },
            },
            {
                "data": "id",
                "width": "15%",
                "render": function (data) {
                    return `
                        <div class="form-group" role="group">
                        <a href="/Admin/Kisi/Edit?id=${data}"
                        class="btn btn-primary "> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/Kisi/Delete/${data}')
                        class="btn btn-danger "> <i class="bi bi-trash-fill"></i> Sil</a>
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
                type: 'DELETE',
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