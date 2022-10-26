
$(document).ready(function () {
    loadSliderMenu();
});
var data = [
    {
        name: 'node1',
        children: [
            { name: 'child1' },
            { name: 'child2' }
        ]
    },
    {
        name: 'node2',
        children: [
            { name: 'child3' }
        ]
    }
];
function loadSliderMenu() {
    $('#tree1').tree({
        data: data
    });
};

