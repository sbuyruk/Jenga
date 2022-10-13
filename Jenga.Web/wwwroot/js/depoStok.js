var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/DepoStok/GetAll"
        },
        "columns": [
            { "data": "depoTanim.adi", "width": "15%" },
            { "data": "aniObjesiTanim.adi", "width": "15%" },
            { "data": "sonAdet", "width": "15%" },
            { "data": "sonIslemYapan", "width": "15%" },
            { "data": "sonIslemTarihi", "width": "15%" },
            { "data": "aciklama", "width": "15%" },        
        ],        
    });
}

