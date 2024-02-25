var dataTable;

$(document).ready(function () {
    loadDataTable();
});
function format(inputDate) {
    let date, month, year;
    date = inputDate;//.getDate();
    month = inputDate.getMonth() + 1;
    year = inputDate.getFullYear();
    date = date.toString().padStart(2, '0');
    month = month.toString().padStart(2, '0');
    return `${date}.${month}.${year}`;
} //const result = format(new Date('2022', '2', '28')); console.log(result); // 28/03/2022
function loadDataTable() {
    $.fn.dataTable.moment('DD.MM.YYYY');
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/AniObjesiDagitim/GetAll"
        },
        "order": [[3, "desc"]] ,
        "columns": [
            {
                "data": "katilimci",
                "width": "15%",
                "render": function (data) {
                    var retval = "";
                    if (data) {
                        retval = data.adi +' '+ data.soyadi;
                    }
                    return retval;
                },
            },
            { "data": "aniObjesiTanim.adi", "width": "15%" },
            { "data": "adet", "width": "5%" },
            {
                "data": "verilisTarihi",
                "type": "date",
                "width": "10%",
                "render": function (data, type, row, meta) {
                    var retval = "";
                    if (data != null) {


                        retval = data;
                    //}
                    //else if ((data == null) && (row.faaliyet != null)) {
                    //    retval = row.faaliyet.baslangicTarihi;
                    } else {
                        retval = "";
                    }
                    return retval;
                },
            },
            //{
            //    "data": "depoTanim",
            //    "width": "15%",
            //    "render": function (data) {
            //        var retval = "";
            //        if (data) {
            //            retval = data.adi;
            //        }
            //        return retval;
            //    },
            //},
            { "data": "katilimci.kurumu", "width": "15%" },
            { "data": "katilimci.gorevi", "width": "15%" },
            {
                "data": "id",
                "width": "15%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="/Admin/AniObjesiTanim/Edit?id=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/AniObjesiTanim/Delete/${data}')
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
