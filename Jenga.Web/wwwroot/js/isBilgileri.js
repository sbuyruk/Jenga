var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/IsBilgileri/GetAll"
        },
        "columns": [
            { "data": "personel.adi", "width": "10%" },
            { "data": "personel.soyadi", "width": "10%" },
            { "data": "personel.sicilNo", "width": "5%" },
            { "data": "birimTanim.adi", "width": "20%" },
            { "data": "unvanTanim.adi", "width": "15%" },
            { "data": "gorevTanim.adi", "width": "20%" },
            {
                "data": "id",
                "width": "10%",
                "render": function (data) {
                    return `
                        <div class="form-group">
                        <a href="/Admin/IsBilgileri/Edit?id=${data}"
                        class="btn btn-primary "> <i class="bi bi-pencil-square"></i> Düzenle</a>                        
					</div>
                        `
                },
            },
            {
                "data": "id",
                "width": "10%",
                "render": function (data) {
                    return `
                    <div class="form-group">
                        <a onClick=Delete('/Admin/IsBilgileri/Delete/${data}')
                        class="btn btn-danger "> <i class="bi bi-trash-fill"></i> Sil</a>
					</div>
                        `
                },
            },
        ]
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
