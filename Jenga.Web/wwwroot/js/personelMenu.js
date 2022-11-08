
$(document).ready(function () {
    loadSidebarMenu();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Personel/GetAll"
        },
        "columns": [
            { "data": "personel.adi", "width": "15%" },
            { "data": "personel.soyadi", "width": "15%" },
            {
                "data": "id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="/Admin/PersonelMenu/Edit?id=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Menü Yetkilerini Görüntüle</a>
                        <a onClick=Delete('/Admin/DepoHareket/Delete/${data}')
                        class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Tüm Menü Yetkilerini Sil</a>
                        <a onClick=Delete('/Admin/DepoHareket/Delete/${data}')
                        class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Menü Yetkilerini Düzenle</a>
					</div>
                        `
                },
            },

        ],
    });
}
//Sidebar 
function loadSidebarMenu() {
    let url = '/Admin/MenuTanim/GetMenuAll';
    try {
        //API'den json al
        $.getJSON(url, function (data) {
            //collapse menuye dönüştür
            buildMenu($('#menu'), data);
          
        });
        
    } catch (e) {
        console.log('error');
    }
}


function buildMenu(parent, items) {
    $.each(items, function () {
        var li = $('<li></li>');
        var aTag;
        if (this.nodes && this.nodes.length > 0) {
            aTag = $('<input type="checkbox" id=' + this.id + ' /><label class=menu-label for=' + this.id + '>' + this.sira + '.'+this.text + '</label>');
        } else {
            aTag = $('<a class=menu-label href=' + this.url + '>' + this.sira + '.' + this.text + '</a>');
        }
        aTag.appendTo(li)
        //if (!this.isActive) {
        //    li.addClass("ui-state-disabled");
        //}
        li.text = this.text;
        li.appendTo(parent);
        if (this.nodes && this.nodes.length > 0) {
            var subUl = $('<ul></ul>');
            subUl.appendTo(li);
            buildMenu(subUl, this.nodes);
        }
    });
}

