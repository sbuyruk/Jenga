var dataTable;


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
                        class="btn btn-success "> Kurum/Görev Seç</a>
					    </div>
                        `;
                    } else {
                        var kurumGorevId = row.MTSKurumGorevs[0].Id;
                        button = `
                        <div class="form-group" role="group">
                        <a  href="/Admin/MTSKurumGorev/Edit?id=${kurumGorevId}"
                        class="btn btn-danger "> Görev Bitir</a>
					    </div>
                        `;
                    }
                    return button
                },
            },
            {
                "data": "Id",
                "width": "15%",
                "render": function (data) {//style="pointer-events: none"
                    return `
                        <div class="form-group" role="group" >
                        <a href="/Admin/AramaGorusme/Create?katilimciId=${data}&KatilimciTipi=2"
                        class="btn btn-outline-success"> Yeni Arama/Görüşme</a>
					</div>
                        `
                },
            },
            {
                "data": "Id",
                "width": "10%",
                "render": function (data) {
                    return `
                        <div class="form-group" role="group" style="pointer-events: none">
                        <a href="/Admin/Kisi/Edit?id=${data}"
                        class="btn btn-secondary "></i> Kişi Kartı</a>
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
                        class="btn btn-danger " > <i class="bi bi-trash-fill"></i> Sil</a>
					</div>
                        `
                },
            },
        ],    
        dom: 'Bfrtip',
        "columnDefs": [
            { "className": "dt-center", "targets": [6, 7,8,9] }
        ],
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
function loadDataTableKisiBul(text) {
    dataTable = $('#tblDataKisibul').DataTable({
        "ajax": {
            "url": "/Admin/Kisi/GetKisiListByFilter",
             "data": {
                Text: text,
            },
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
            { data: "Kurumu" },
            { data: "Gorevi" },
            { data: "Unvani" },
            { data: "Telefon1" },
            { data: "TCKimlikNo" },
            {
                "data": "Id",
                "width": "15%",
                "render": function (data, type, row, meta) {
                    var button = "";
                    if (row.KatilimciTipi == 2) {
                        button = ` 
                        <div class="form-group" role="group" >
                            <a href="/Admin/Kisi/Edit?id=${row.Id}" class="btn btn-primary "></i> Kişi Bilgileri</a>
					    </div>
                        `;
                    } else {

                        if (row.TCKimlikNo.length < 11 || row.TCKimlikNo < 1) {
                            button = `TC Kimlik No Eksik`;
                        } else {

                            button = `
                                <div class="form-group" role="group">
                                    <button type="button" class="btn btn-danger form-control" id="MTSyeTasiBtn" onclick="OpenModalOnay(${row.Id},${row.KatilimciTipi})"> MTS ye Taşı</button>
					            </div>
                            `;
                        }
                    }
                    return button
                },
            },
            
        ],
        destroy: true,
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