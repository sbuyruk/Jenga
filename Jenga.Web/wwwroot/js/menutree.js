
$(document).ready(function () {
    loadSidebarMenu();
});
//Sidebar 
function loadSidebarMenu() {
    //let url = '/Admin/MenuTanim/GetMenuAll';
    let url = '/Admin/MenuTanim/GetMenuByPersonId';
   
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
        var li = $('<li id=liSB' + this.id + '></li>');
        var aTag;
        if (this.nodes && this.nodes.length > 0) {
            aTag = $('<input class=menuInput type="checkbox" id=' + this.id + ' /><label class=menu-label for=' + this.id + '>' + this.sira + '.'+this.text + '</label>');
        } else {
            aTag = $('<a class=menu-label href=' + this.url + '> <i class="bi bi-circle-fill" style="font-size:14px;" ></i>&nbsp;&nbsp;&nbsp;' + this.sira + '.' + this.text + '</a>');
        }
        aTag.appendTo(li)
        //if (!this.isActive) {
        //    li.addClass("ui-state-disabled");
        //}
        li.text = this.text;
        li.appendTo(parent);
        if (this.nodes && this.nodes.length > 0) {
            var subUl = $('<ul class=menuUl></ul>');
            subUl.appendTo(li);
            buildMenu(subUl, this.nodes);
        }
    });
}

//Open - Close Sidebar with animation
var isClosed = false;
function openCloseSideBar(menuIconsDiv) {
    menuIconsDiv.classList.toggle("change");
    if (isClosed) {
        openNav();
        isClosed = false;
    } else {
        closeNav();
        isClosed = true;
    }
    
}
function openNav() {
    document.getElementById("mySidebar").style.width = "300px";
    document.getElementById("main").style.marginLeft = "300px";
}
function closeNav() {
    document.getElementById("mySidebar").style.width = "0px";
    document.getElementById("main").style.marginLeft = "0px";
}