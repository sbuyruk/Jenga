
$(document).ready(function () {
    loadMenuTree();
    loadDataTable();
    
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/PersonelMenu/GetPersonelAll"
        },
        "columns": [
            { "data": "personel.personel.adi", "width": "20%" },
            { "data": "personel.personel.soyadi", "width": "20%" },
            { "data": "menuTanim", "width": "20%" },
            {
                "data": "personel.id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="form-group" role="group">
                        <a href="/Admin/PersonelMenu/Edit?id=${data}"
                        class="btn btn-secondary"> <i class="bi bi-binoculars "></i>Görüntüle</a>
                        <a href="/Admin/PersonelMenu/Edit?id=${data}"
                        class="btn btn-primary"> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/PersonelMenu/Delete/${data}')
                        class="btn btn-danger"> <i class="bi bi-trash-fill"></i> Sil</a>
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
// URL'de yazan API den datayı al
function loadMenuTree() {
    let url = '/Admin/MenuTanim/GetMenuAll';
    try {
        //API'den json al
        $.getJSON(url, function (menuTanimdata) {
            //collapse menuye dönüştür
            buildMenuTree($('#bs_main'), menuTanimdata);
            loadFunctions();
        });
        
    } catch (e) {
        console.log('error');
    }
}
//Veritabanından (MenuTanim_Table) okuduğu listeyi recursive olarak tree ye yerleştirsin
// TODO veritabanından okumayı personel-menu ilişkisinin tanımlandığı tablodan getirmeli
function buildMenuTree(parent, items) {
    $.each(items, function () {
        var inputTag;
        if (this.nodes && this.nodes.length > 0) {
            inputTag = $('<input class=menuEditInput type="checkbox" id=hasChildren' + this.id + ' />');
        } else {
            inputTag = $('<input class=menuEditInput type="checkbox" id=noChildren' + this.id + ' />');
        }

        var li = $('<li class=menuEditLi id=li' + this.id + '></li>');
        var spanTag = $('<span class="plus">&nbsp;&nbsp;&nbsp;</span>');
        //var inputTag = $('<input class=menuEditInput type="checkbox" id=chk' + this.id + ' />');
        var labelTag = $('<span>' + this.text + ' </span>');


        
        inputTag.appendTo(li);
        labelTag.appendTo(li);

        li.text = this.text;
        li.appendTo(parent);
        if (this.nodes && this.nodes.length > 0) {
            spanTag.appendTo(li);
            var subUl = $('<ul  class=menuEditUl id=ul' + this.id + ' style="display: none"></ul>');
            subUl.appendTo(li);
            buildMenuTree(subUl, this.nodes);
        } 
       
    });
}
function ToggleCollapsState() {
    $(".plus").click(function () {
        $(this).toggleClass("minus").siblings("ul").toggle();
    })
}
/*
 Bir item checked edildiğinde varsa tüm altsoyları de checked edilsin
 */
function SetChilderenCheckedValue() {
    $("input[class=menuEditInput]").click(function () {
        $(this).siblings("ul").find("input[class=menuEditInput]").prop('checked', $(this).prop('checked'));
    })
}
/*  ÖNEMLİ
//  Check durumu değiştirildiğinde
//  a. Bu nodun alt soyunun tamamının checked durumu değişir
//  b. Bu nodun üst soyunun ise:
//      (1). Eğer tek işaretli nod bu nodun atasıysa değişir
//      (2). Ata nodlardan biri işaretlenmiş kardeş noda sahipse bundan üstteki atalar değişmez
*/
function SetParentsCheckedValue(item, isChecked) {
    if (item.is("li")){ //li ise içinde input vardır
        var inputt = item.find("input[type=checkbox]")[0]; //ilk input benim parent'imdir
        inputt.checked = isChecked; //parent'imin inputunu benim checked değerime eşitle
        var siblingss = item.siblings().find("input[class=menuEditInput]").prop('id'); //parent'imin kardeşleri var mı? yoksa undefined döner
        if (typeof siblingss === "undefined") {//undefined döndü ise, kardeşi yok demektir o zaman perintimin parentine git
            SetParentsCheckedValue(item.parent(), isChecked)
        }
        else if (((item.siblings().children('input[class=menuEditInput]').prop('checked') != isChecked) && isChecked)// parent'imin kardeşleri varsa o kardeşlerin checked durumu ...
            || (item.siblings().children('input[class=menuEditInput]').prop('checked') == isChecked) && !isChecked) // .. getirdiğimiz checked durumundan farklı mı
        {
            var checkedVarMi = false;
            item.siblings().children('input[class=menuEditInput]').each(function () { //parentimin kardeşlerinin child'larına bakıyoruz, çünkü input li'nin bir altındadır
                if ($(this).prop('checked')) {
                    checkedVarMi = true;
                    return false;
                }
            });
            if (!checkedVarMi) //parentimn child'larından hiç biri checked değilde üst soya işleme devam et
                SetParentsCheckedValue(item.parent(), isChecked)
        }
        else return;
    }else if (item.is("div")) {//burası çıkış noktası en üstteki div'e ulaşınca dur!.
        return;
    }
    else { // item li değilse bişey yapmadan üst soya devam et 
        SetParentsCheckedValue(item.parent(), isChecked)
    }
}
function loadFunctions() {
    ToggleCollapsState();

    SetChilderenCheckedValue();

    $("input[class=menuEditInput]").click(function () {
        SetParentsCheckedValue($(this), $(this).prop('checked'));
    })
    var array = [];
    //$("input:checkbox[name=type]:checked").each(function () {
    $("input[class=menuEditInput]:checked").each(function () {
        array.push($(this).val());
    });
    $('#GFG_DOWN').text(array);
}

function FindCheckedMenus() {
    var array = [];
    //$("input:checkbox[name=type]:checked").each(function () {
    $("input[class=menuEditInput]:checked").each(function () {
        array.push($(this).val());
    });
    $('#GFG_DOWN').text(array);
/*    alert("array=" + array);*/
    document.getElementById("SelectedMenuTanimListLbl").value = "1,2,3";
    $('#SelectedMenuTanimListLbl').val(array);
    //alert("SelectedMenuTanimListLbl=" + $('#SelectedMenuTanimListLbl').val());

}

