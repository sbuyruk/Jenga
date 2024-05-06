var dataTable;



function loadDataTable() {
    //var baslangicTarihi;
    //if (bastar == null) {
    //    let d = new Date();
    //    d.setMonth(d.getMonth() - 3)
    //    baslangicTarihi = d.toLocaleDateString("tr-TR");
    //} else {
    //    baslangicTarihi= bastar ;
    //}
    $.fn.dataTable.moment('DD.MM.YYYY HH:mm'); // Date format

    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/AramaGorusme/GetAllAramaGorusmeList",
            //"data": {
            //    BaslangicTarihi: baslangicTarihi,
            //},
        },
        "initComplete": function (settings, json) {
            console.log(json);
            // call your function here
        },
        "columns": [
            { "data": "aramaGorusme.Id", "width": "4%" },
            {
                "data": "aramaGorusme.Kisi",
                "width": "15%",
                "render": function (data) {
                    var retval = "";
                    if (data) {
                        retval = (data.Adi == null ? '' : data.Adi) + ' ' + (data.Soyadi == null ? '' : data.Soyadi)  ;

                    }
                    return retval;
                },
            },
            { "data": "aramaGorusme.Tarih", "width": "10%" },
            { "data": "aramaGorusme.Konu", "width": "20%" },
            {
                "data": "aramaGorusme.Kisi",
                "width": "20%",
                "render": function (data) {
                    var retval = "";
                    if (data) {
                        var kurumu = (data.MTSKurumGorevs == null) || (data.MTSKurumGorevs.length == 0) ? "" : data.MTSKurumGorevs[0].MTSKurumTanim.Adi;
                        var gorevi = (data.MTSKurumGorevs == null) || (data.MTSKurumGorevs.length == 0) ? "" : data.MTSKurumGorevs[0].MTSGorevTanim.Adi;
                        retval = (data.MTSKurumGorevs == null ? '' : kurumu) + '  ' + (gorevi == null ? '' : gorevi).trim();

                    }
                    return retval;
                },
            },
            {
                "data": "aramaGorusme.FaaliyetId",
                "width": "15%",
                "render": function (data, type, row, meta) {
                    if (data ==0) {
                        return''
                    }
                    else {
                        var faaliyet = row.aramaGorusme.Faaliyet;
                        var acik = faaliyet == null ? '' : (row.aramaGorusme.Faaliyet.AcikTarih ? '(Açık)' : '');
                        var retval = `
                        <div class="btn-group" role="group">
                            <a href="http://tskgv-portal/Sayfalar/FaaliyetGirisi.aspx?FaaliyetId=${data}&"
                            class="btn btn-warning mx-2"> <i class="bi bi-pencil-square"></i> Faaliyet ` + acik +`</a>
					    </div>
                        `;

                        return retval;
                    }
                },
            },
            {
                "data": "aramaGorusme.Id",
                "width": "9%",
                "render": function (data) {
                    var button = `
                        <div class="form-group" role="group">
                        <a  href="/Admin/AramaGorusme/Edit?id=${data}"
                        class="btn btn-primary "> <i class="bi bi-pencil-square"></i> Düzenle </a>
                        `;
                    return button;
                },
            },
            {
                "data": "aramaGorusme.Id",
                "width": "7%",
                "render": function (data) {
                    var button = `
                        <div class="form-group" role="group">
                        <a onClick=Delete('/Admin/AramaGorusme/Delete/${data}')
                        class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i>Sil</a>
					    </div>
                        `;
                    return button;
                },
            },
        ],
        dom: 'Bfrtip',
        order: [
            [2, 'desc']  // Sort by the third column (date time) in descending order
        ],
        destroy: true,
        "columnDefs": [
            { "className": "dt-center", "targets": [5,6,7] }
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
function loadModalDataTableKisi(actionType) {
    if (jQuery.fn.DataTable.isDataTable('#tblDataModalKisi')) {
        jQuery('#tblDataModalKisi').DataTable().destroy();
    }
    jQuery('#tblDataModalKisi tbody').empty();
    dataTable = $('#tblDataModalKisi').DataTable({
        "ajax": {
            "url": "/Admin/Kisi/GetAll",// GetAllKatilimci",
            type: 'GET',

        },
        "columns": [
            {
                "data": "Adi",
                "width": "30%",
                "render": function (data, type, row, meta) {
                    return `
                        <div class="form-group" >                        
                            <p><a class="link-opacity-50" href="/Admin/Kisi/Edit?id=${row.Id}">${row.Adi} ${row.Soyadi}</a></p>
					    </div>
                        `
                },
            },
            {
                "data": "Id",
                "width": "70%",
                "render": function (data, type, row, meta) {
                    var kurum = "";
                    var gorev = "";
                    var kurumGorev = "";

                    if (row.MTSKurumGorevs != null && row.MTSKurumGorevs[0] != null) {
                        if (row.MTSKurumGorevs[0].MTSKurumTanim != null && row.MTSKurumGorevs[0].MTSKurumTanim.Adi != null) {
                            kurum = row.MTSKurumGorevs[0].MTSKurumTanim.Adi;
                        }
                        if (row.MTSKurumGorevs[0].MTSGorevTanim != null && row.MTSKurumGorevs[0].MTSGorevTanim.Adi != null) {
                            gorev = row.MTSKurumGorevs[0].MTSGorevTanim.Adi;
                        }
                    }

                    kurumGorev = kurum + " - " + gorev;
                    return `
                        <div class="form-group" >
                            <p class="">${kurumGorev}</p>
                            
					    </div>
                        `
                },
            },
            {
                "data": "arayanId",
                "width": "60%",
                "render": function (data, type, row, meta) {
                        var adiSoyadi = "";
                        var kurum = "";
                        var gorev = "";
                        var kurumGorev = "";

                        if (row.Adi != null && row.Soyadi != null) {
                            adiSoyadi = row.Adi + " " + row.Soyadi;
                        }
                        if (row.MTSKurumGorevs != null && row.MTSKurumGorevs[0] != null) {
                            if (row.MTSKurumGorevs[0].MTSKurumTanim != null && row.MTSKurumGorevs[0].MTSKurumTanim.Adi != null) {
                                kurum = row.MTSKurumGorevs[0].MTSKurumTanim.Adi;
                            }
                            if (row.MTSKurumGorevs[0].MTSGorevTanim != null && row.MTSKurumGorevs[0].MTSGorevTanim.Adi != null) {
                                gorev = row.MTSKurumGorevs[0].MTSGorevTanim.Adi;
                            }
                        }

                        kurumGorev = kurum + " / " + gorev;
                    if (actionType == "Edit") {

                        var retval =
                            `
                        <div class="form-group" >                        
                            <p><a class="btn btn-primary" href='javascript:;' onclick='ReplaceKisi(${row.Id},"${adiSoyadi}","${kurumGorev}");'>DEĞİŞTİR</a></p>
					    </div>
                        `;

                    } else {
                        var retval =
                            `
                        <div class="form-group" >                        
                            
                            <p><a class="btn btn-success" href="/Admin/AramaGorusme/Create?kisiId=`+ row.Id +`">SEÇ</a></p>
					    </div>
                        `;
                    }
                return retval;
                },
            },
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
