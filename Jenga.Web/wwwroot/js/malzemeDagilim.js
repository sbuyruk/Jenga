var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/MalzemeDagilim/GetAll"
        },
        "columns": [
            { "data": "malzeme.adi", "width": "15%" },
            { "data": "malzeme.malzemeCinsi.malzemeGrubu.adi", "width": "15%" },
            { "data": "malzeme.malzemeCinsi.adi", "width": "15%" },
            { "data": "malzemeYeriTanim.adi", "width": "15%" },
            { "data": "adet", "width": "15%" },
            { "data": "aciklama", "width": "15%" },        
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

