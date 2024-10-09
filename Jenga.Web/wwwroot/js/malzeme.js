var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Malzeme/GetAll",
        },
        "columns": [
            { "data": "adi", "width": "20%" },
            { "data": "malzemeCinsi.adi", "width": "15%" },
            { "data": "markaTanim.adi", "width": "10%" },
            { "data": "modelTanim.adi", "width": "10%" },
            { "data": "aciklama", "width": "25%" },
            {
                "data": "id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="/Admin/Malzeme/Edit?id=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/Malzeme/Delete/${data}')
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
// #region MalzemeOzellik 
function loadMalzemeOzellikDataTable(malzemeId) {
    dataTable = $('#tblDataMalzemeOzellik').DataTable({
        "ajax": {
            "url": "/Admin/MalzemeOzellik/GetAllByMalzemeId",
            data: {
                malzemeId: malzemeId,
            },
        },
        "columns": [
            { "data": "ozellik.adi", "width": "30%" },
            {
                "data": "ozellik",
                "width": "50%",
                "render": function (data) {
                    return data.miktar + " " + data.olcuBirimi + " " + data.aciklama;
                },
            },
           
            {
                "data": "id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="txt-end">
                        <a onClick=Delete('/Admin/MalzemeOzellik/DeleteRow/${data}')
                        class="btn btn-danger mx-2"> <i class="bi bi-dash-square"></i> Çıkar</a>
					</div>
                        `
                },
            },

        ],
        dom: 'frtip',
        
    });
}

// #endregion MalzemeOzellik
function Delete(url) {
    Swal.fire({
        title: 'Çıkarmak istediğinize emin misiniz?',
        text: "",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Çıkar',
        cancelButtonText: 'Vazgeç'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    $('#tblDataMalzemeOzellik').DataTable().ajax.reload();
                    toastr.success(data.message);
                },
                error: function (data) {
                    toastr.error(data.message);
                }
            })
        }
    })
}

// #region Modal 
function OpenModal() {
    var malzemeCinsiId = document.getElementById('MalzemeCinsiIdSlc').value;
    var malzemeId = document.getElementById('MalzemeIdInp').value;
    loadModalDataTable(malzemeCinsiId, malzemeId);
    $('#tblDataModal').DataTable().ajax.reload();
    // // Open the modal
    // var modal = new bootstrap.Modal(myModal);
    var myModalEl = document.querySelector('#tblDataModalDiv')
    var modal = bootstrap.Modal.getOrCreateInstance(myModalEl)
    // modal.show();
}
function loadModalDataTable(malzemeCinsiId, malzemeId) {

    dataTable = $('#tblDataModal').DataTable({
        "ajax": {
            "url": "/Admin/Ozellik/GetAllByMalzemeCinsiId",
            data: {
                malzemeCinsiId: malzemeCinsiId,
                malzemeId: malzemeId,
            },
        },
        "columns": [
            { "data": "adi", "width": "30%" },
            {
                "data": "id",
                "width": "70%",
                "render": function (data, type, row, meta) {
                    return row.miktar + " " + row.olcuBirimi + " " + row.aciklama;
                },
            },
        ],
        dom: 'frtip',
        retrieve: true,
        destroy: true,
    });

    dataTable.off('click').on('click', 'tbody tr', function (e) { //modal ikinci kez açıldığında onclick eventi iki kere çalışıyordu -SB- table row select düzgün çalışmıyordu
        e.currentTarget.classList.toggle('selected');
    });
}

document.querySelector('#saveSelectedBtn').addEventListener('click', function () {
    var selectedRowCount = dataTable.rows('.selected').data().length;
    var selectedIdArray = dataTable.rows('.selected').data();
    var malzemeId = document.getElementById('MalzemeIdInp').value;
    SaveSelectedRows(selectedIdArray, malzemeId);
});
function SaveSelectedRows(selectedIdArray, malzemeId) {
    var selectedIds = [];

    selectedIdArray.each(function (item) {
        selectedIds.push(item.id);
    });

    $.ajax({
        url: '/Admin/MalzemeOzellik/SaveSelectedRows',  // Make sure the URL points to your controller action
        type: 'POST',
        data:
            JSON.stringify({
                selectedIds: selectedIds,
                malzemeId: malzemeId,
            }),
        // Send array as JSON
        contentType: 'application/json; charset=utf-8',  // Specify JSON format
        success: function (response) {
            console.log("Data saved successfully", response);
            $('#tblDataModalDiv').modal('hide');
            $('#tblDataMalzemeOzellik').DataTable().ajax.reload();
        },
        error: function (error) {
            console.error("Error saving data: ", error);
        }
    });
}
// #endregion Modal  

