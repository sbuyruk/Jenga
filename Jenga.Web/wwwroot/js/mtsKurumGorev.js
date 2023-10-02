var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/MTSKurumGorev/GetAll"
        },
        "columns": [
            {
                "data": "kisi",
                "width": "15%",
                "render": function (data) {
                    return data.adi+' '+data.soyadi;
                },
            },
            { "data": "mtsKurumTanim.adi", "width": "10%" },
            { "data": "mtsGorevTanim.adi", "width": "10%" },
            { "data": "baslamaTarihi", "width": "10%" },
            { "data": "durum", "width": "10%" },
            {
                "data": "durum",
                "width": "20%",
                "render": function (data, type, row, meta) {
                    var button =`
                        <div class="btn-group" role="group">
                        <a href="/Admin/MTSKurumGorev/Edit?id=${row.id}"
                        class="btn btn-primary mx-2"> <i class="bi bi-arrow-down-circle me-1"></i>Görevi Bitir</a>
					</div>
                        `
                    if (row.durum == "Ayrıldı") {
                        button = `
                        <div class="btn-group" role="group">
                        <a href="/Admin/MTSKurumGorev/Edit?id=${row.id}"
                        class="btn btn-success mx-2"> <i class="bi bi-arrow-up-circle me-1"></i> Yeni Görev</a>
					</div>
                        `
                    }

                    return button;
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
