var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/MalzemeHareket/GetAll"
        },
        "columns": [
            //{ "data": "Malzeme.Adi", "width": "10%" },
            {
                "data": "Id",
                "width": "20%",
                "render": function (data, type, row, meta) {
                    var malzeme = "";
                    if (row.Malzeme) {
                        malzeme = row.Malzeme.Adi;
                        if (row.Malzeme.MalzemeCinsi) {
                            malzeme = row.Malzeme.Adi + " (" + row.Malzeme.MalzemeCinsi.Adi+") ";
                            if (row.Malzeme.MalzemeCinsi.MalzemeGrubu) {
                                malzeme = row.Malzeme.Adi + " (" + row.Malzeme.MalzemeCinsi.Adi + " - " + row.Malzeme.MalzemeCinsi.MalzemeGrubu.Adi + ") ";

                            }
                        }
                    }
                    return malzeme;
                },
            },
            //{ "data": "Malzeme.MalzemeCinsi.MalzemeGrubu.Adi", "width": "10%" },
            //{ "data": "Malzeme.MalzemeCinsi.Adi", "width": "10%" },
            { "data": "KaynakYeri.Adi", "width": "10%" },
            { "data": "HedefYeri.Adi", "width": "10%" },
            { "data": "Adet", "width": "10%" },
            { "data": "GirisCikis", "width": "10%" },
            { "data": "IslemTipi", "width": "10%" },
            { "data": "IslemTarihi", "width": "10%" },
            { "data": "Aciklama", "width": "15%" },    
            {
                "data": "Id",
                "width": "10%",
                "render": function (data) {
                    return `
                        <div class="form-group" role="group">
                        <a href="/Admin/MalzemeHareket/Edit?id=${data}"
                        class="btn btn-primary "> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/MalzemeHareket/Delete/${data}')
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

