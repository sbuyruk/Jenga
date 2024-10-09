var dataTable;

$(document).ready(function () {
    //loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Ozellik/GetAll",
        },
        "columns": [
            { "data": "adi", "width": "15%" },
            { "data": "malzemeCinsi.adi", "width": "15%" },
            { "data": "miktar", "width": "15%" },
            { "data": "olcuBirimi", "width": "10%" },
            { "data": "ozellik", "width": "10%" },
            { "data": "aciklama", "width": "15%" },
            {
                "data": "id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="/Admin/Ozellik/Edit?id=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/Ozellik/Delete/${data}')
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
function loadDataTableOzellik(malzemeCinsiId) {

    $.fn.dataTable.moment('DD.MM.YYYY HH:mm'); // Date format
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Ozellik/GetByMalzemeCinsi",
            "data": {
                'malzemeCinsiId': malzemeCinsiId,
            },
        },
        "columns": [
            { "data": "adi", "width": "15%" },
            { "data": "miktar", "width": "15%" },
            { "data": "olcuBirimi", "width": "10%" },
            { "data": "ozellik", "width": "10%" },
            { "data": "aciklama", "width": "15%" },
            {
                "data": "id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="/Admin/Ozellik/Edit?id=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/Ozellik/Delete/${data}')
                        class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Sil</a>
					</div>
                        `
                },
            },
        ],
        "order": [[0, "desc"]],
        dom: 'frtip',
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
function loadOzellikDataTable(malzemeCinsiId) {
    dataTable = $('#tblData').DataTable({
        "url": "/Admin/Ozellik/GetByMalzemeCinsi",
        "data": {
            malzemeCinsiId: malzemeCinsiId,
        },
        "columns": [
            { "data": "adi", "width": "15%" },
            { "data": "malzemeCinsi.adi", "width": "15%" },
            { "data": "miktar", "width": "15%" },
            { "data": "olcuBirimi", "width": "10%" },
            { "data": "ozellik", "width": "10%" },
            { "data": "aciklama", "width": "15%" },
            {
                "data": "id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="/Admin/Ozellik/Edit?id=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/Ozellik/Delete/${data}')
                        class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Sil</a>
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
