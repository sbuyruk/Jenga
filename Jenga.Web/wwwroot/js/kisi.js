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
            { data: "Id", "width": "4%" },
            {
                "data": "Id",
                "width": "15%",
                "render": function (data, type, row, meta) {
                    return row.Adi + ' ' + row.Soyadi;
                },
            },
            { data: "MTSKurumGorevs[0].MTSKurumTanim.Adi" },
            { data: "MTSKurumGorevs[0].MTSGorevTanim.Adi" },
            { data: "Unvani" },
            { data: "Telefon1" },
            {
                "data": "Id",
                "width": "15%",
                "render": function (data, type, row, meta) {
                    var button = "";
                    if (row.MTSKurumGorevs.length < 1) {
                        button = `
                        <div class="form-group" role="group">
                        <a href="/Admin/MTSKurumGorev/Create?kisiId=${data}"
                        class="btn btn-success "> <i class="bi bi-pencil-square"></i> Kurum/Görev Seç</a>
					    </div>
                        `;
                    } else {
                        var kurumGorevId = row.MTSKurumGorevs[0].Id;
                        button = `
                        <div class="form-group" role="group">
                        <a  href="/Admin/MTSKurumGorev/Edit?id=${kurumGorevId}"
                        class="btn btn-danger "> <i class="bi bi-pencil-square"></i> Görev Bitir</a>
					    </div>
                        `;
                    }
                    return button
                },
            },
            {
                "data": "Id",
                "width": "15%",
                "render": function (data) {
                    return `
                        <div class="form-group" role="group" style="pointer-events: none">
                        <a href="/Admin/Kisi/Edit?id=${data}"
                        class="btn btn-secondary "> <i class="bi bi-pencil-square"></i> Arama/Görüşme</a>
					</div>
                        `
                },
            },
            {
                "data": "Id",
                "width": "15%",
                "render": function (data) {
                    return `
                        <div class="form-group" role="group" style="pointer-events:none">
                        <a href="/Admin/Kisi/Edit?id=${data}"
                        class="btn btn-secondary "> <i class="bi bi-pencil-square"></i> Kişi Kartı</a>
					</div>
                        `
                },
            },
            {
                "data": "Id",
                "width": "15%",
                "render": function (data) {
                    return `
                        <div class="form-group" role="group">
                        <a href="/Admin/Kisi/Edit?id=${data}"
                        class="btn btn-primary "> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/Kisi/Delete/${data}')
                        class="btn btn-secondary " style="pointer-events:none"> <i class="bi bi-trash-fill"></i> Sil</a>
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