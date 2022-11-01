
$(document).ready(function () {
    let url = '/Admin/MenuTanim/GetMenuAll';

    try {

        $.getJSON(url, function (data) {
            buildMenu($('#menu'), data);
            $('#menu').menu();
        });
        function buildMenu(parent, items) {
            $.each(items, function () {
                var li = $('<li>' + this.text + '</li>');
                if (!this.isActive) {
                    li.addClass("ui-state-disabled");
                }
                li.appendTo(parent);
                if (this.nodes && this.nodes.length > 0) {
                    var ul = $('<ul></ul>');
                    ul.appendTo(li);
                    buildMenu(ul, this.nodes);
                }
            });
        }  
    } catch (e) {
        console.log('error');
    }

});
//Open - Close Sidebar with animation
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
var isClosed = true;
function openNav() {
    document.getElementById("mySidebar").style.width = "300px";
    document.getElementById("main").style.marginLeft = "300px";
}

function closeNav() {
    document.getElementById("mySidebar").style.width = "0px";
    document.getElementById("main").style.marginLeft = "0px";
}