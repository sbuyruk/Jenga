
$(document).ready(function () {
    loadSidebarMenu();
});
//Sidebar Menu
function loadSidebarMenu() {
    let url = '/Admin/MenuTanim/GetMenuAll';
    try {

        $.getJSON(url, function (data) {
            buildMenu($('#menu'), data);
          
        });
        
    } catch (e) {
        console.log('error');
    }
}
$(document).on('click', '.list > li ', function () {
    $(this).next('ul').toggle();
});

function buildMenu(parent, items) {
    $.each(items, function () {
        var li = $('<li></li>');
       
        var aTag;
        if (this.nodes && this.nodes.length > 0) {
            aTag = $('<input type="checkbox" id=' + this.id + ' /><label for=' + this.id + '>' + this.text + '</label>');
            //$(this).prev("input").attr("id", "newId");
        } else {
            aTag = $('<label>' + this.text + '</label>');
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

//Open - Close Sidebar with animation
var isClosed = true;
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