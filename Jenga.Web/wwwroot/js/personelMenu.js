
$(document).ready(function () {
    loadMenuTree();
    loadDataTable();
    
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/PersonelMenu/GetAll"
        },
        "columns": [
            { "data": "adi", "width": "20%" },
            { "data": "soyadi", "width": "20%" },
            {
                "data": "id",
                "width": "20%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                        <a href="/Admin/PersonelMenu/Edit?id=${data}"
                        class="btn btn-secondary mx-2"> <i class="bi bi-binoculars "></i>Görüntüle</a>
                        <a onClick=Delete('/Admin/DepoHareket/Edit/${data}')
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Düzenle</a>
                        <a onClick=Delete('/Admin/DepoHareket/Delete/${data}')
                        class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Sil</a>
					</div>
                        `
                },
            },

        ],
    });
}
//Sidebar 
function loadMenuTree() {
    let url = '/Admin/PersonelMenu/GetMenuAll';
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
function SetChilderenCheckedValue() {
    $("input[class=menuEditInput]").click(function () {
        $(this).siblings("ul").find("input[class=menuEditInput]").prop('checked', $(this).prop('checked'));
    })
}
//1. Check durumu değiştirildiğinde
//  a. Bu nodun alt soyunun tamamının checked durumu değişir
//  b. Bu nodun üst soyunun ise:
//      (1). Eğer tek işaretli nod bu nodun atasıysa değişir
//      (2). Ata nodlardan biri işaretlenmiş kardeş noda sahipse bundan üstteki atalar değişmez
function SetParentsCheckedValue(item, isChecked) {
    if (item.is("li")){
        var inputt = item.find("input[type=checkbox]")[0];
        inputt.checked = isChecked;
        var siblingss = item.siblings().find("input[class=menuEditInput]").prop('id');
        if (typeof siblingss === "undefined") {
            SetParentsCheckedValue(item.parent(), isChecked)
        }
        else if (((item.siblings().children('input[class=menuEditInput]').prop('checked') != isChecked) && isChecked)
            || (item.siblings().children('input[class=menuEditInput]').prop('checked') == isChecked) && !isChecked)
        {
            var checkedVarMi = false;
            item.siblings().children('input[class=menuEditInput]').each(function () {
                if ($(this).prop('checked')) {
                    checkedVarMi = true;
                    return false;
                }


            });
            if (!checkedVarMi)
                SetParentsCheckedValue(item.parent(), isChecked)
        }
        else return;
    }else if (item.is("div")) {
        return;
    }
    else {
        SetParentsCheckedValue(item.parent(), isChecked)
    }
        
    
}
function loadFunctions() {
    ToggleCollapsState();

    SetChilderenCheckedValue();

    $("input[class=menuEditInput]").click(function () {
        SetParentsCheckedValue($(this), $(this).prop('checked'));
    })

    //$("input[class=menuEditInput]").change(function () {
    //    var sp = $(this).attr("id");
    //    if (sp.substring(0, 4) === "c_io") {
    //        var ff = $(this).parents("ul[id^=bf_l]").attr("id");
    //        if ($('#' + ff + ' > li input[class=menuEditInput]:checked').length == $('#' + ff + ' > li input[class=menuEditInput]').length) {
    //            $('#' + ff).siblings("input[class=menuEditInput]").prop('checked', true);
    //            check_fst_lvl(ff);
    //        }
    //        else {
    //            $('#' + ff).siblings("input[class=menuEditInput]").prop('checked', false);
    //            check_fst_lvl(ff);
    //        }
    //    }

    //    if (sp.substring(0, 4) === "c_bf") {
    //        var ss = $(this).parents("ul[id^=bs_l]").attr("id");
    //        if ($('#' + ss + ' > li input[class=menuEditInput]:checked').length == $('#' + ss + ' > li input[class=menuEditInput]').length) {
    //            $('#' + ss).siblings("input[class=menuEditInput]").prop('checked', true);
    //            check_fst_lvl(ss);
    //        }
    //        else {
    //            $('#' + ss).siblings("input[class=menuEditInput]").prop('checked', false);
    //            check_fst_lvl(ss);
    //        }
    //    }
    //});

}

function check_fst_lvl(dd) {
    //var ss = $('#' + dd).parents("ul[id^=bs_l]").attr("id");
    var ss = $('#' + dd).parent().closest("ul").attr("id");
    if ($('#' + ss + ' > li input[class=menuEditInput]:checked').length == $('#' + ss + ' > li input[class=menuEditInput]').length) {
        //$('#' + ss).siblings("input[id^=c_bs]").prop('checked', true);
        $('#' + ss).siblings("input[class=menuEditInput]").prop('checked', true);
    }
    else {
        //$('#' + ss).siblings("input[id^=c_bs]").prop('checked', false);
        $('#' + ss).siblings("input[class=menuEditInput]").prop('checked', false);
    }

}
