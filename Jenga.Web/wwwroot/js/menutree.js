
$(document).ready(function () {
    loadSliderMenu();
});
var data =  [
    {
        text: "Node 1",
        icon: "fa fa-folder",
        expanded: true,
        nodes: [
            {
                text: "Sub Node 1",
                icon: "fa fa-folder",
                nodes: [
                    {
                        id: "sub-node-1",
                        text: "Sub Child Node 1",
                        icon: "fa fa-folder",
                        class: "dropdown-menu",
                        href: "https://google.com"
                    },
                    {
                        text: "Sub Child Node 2",
                        icon: "fa fa-folder"
                    }
                ]
            },
            {
                text: "Sub Node 2",
                icon: "fa fa-folder"
            }
        ]
    },
    {
        text: "Node 2",
        icon: "fa fa-folder"
    },
    {
        text: "Node 3",
        icon: "fa fa-folder"
    },
    {
        text: "Node 4",
        icon: "fa fa-folder"
    },
    {
        text: "Node 5",
        icon: "fa fa-folder"
    }
];

function loadSliderMenu() {
    //$.getJSON('Admin/MenuTanim/GetMenuAll', function (data) {

    //    var text = `text: ${data.text}<br>
    //                id: ${data.time}<br>`


    //    $(".mySidebar").html(text);
    //});
    $(jQuery.parseJSON(JSON.stringify(data))).each(function () {

        var text = `id: ${this.id}<br>
                    text: ${this.text}<br>`


        $(".mySidebar").html(text);
    });
};
var isClosed = true;
function menuTreeFunction(x) {
    x.classList.toggle("change");
    if (isClosed) {
        openNav();
        isClosed = false;
    } else {
        closeNav();
        isClosed = true;
    }
    
}
function openNav() {
    document.getElementById("mySidebar").style.width = "250px";
    document.getElementById("main").style.marginLeft = "250px";
}

function closeNav() {
    document.getElementById("mySidebar").style.width = "0px";
    document.getElementById("main").style.marginLeft = "0px";
}