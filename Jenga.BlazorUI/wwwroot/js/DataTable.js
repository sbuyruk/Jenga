window.initRolTableWithExport = (data) => {
    new DataTable('#RolListTable', {
        layout: {
            topStart: {
                buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
            }
        },
    });
};
window.initDataTable = (data) => {
    new DataTable('#DataTable', {
        layout: {
            topStart: {
                buttons: ['copy', 'csv', 'excel', 'pdf', 'print', 'pageLength']
            }
        },
    });
};
// Material Tables
function initMaterialTableWithExport() {
    $('#MaterialListTable').DataTable({
        dom: 'Bfrtip',
        buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
    });
}
function initMaterialCategoryTableWithExport() {
    $('#MaterialCategoryListTable').DataTable({
        dom: 'Bfrtip',
        buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
    });
}
function initMaterialModelTableWithExport() {
    $('#MaterialModelListTable').DataTable({
        dom: 'Bfrtip',
        buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
    });
}
function initMaterialBrandTableWithExport() {
    $('#MaterialBrandListTable').DataTable({
        dom: 'Bfrtip',
        buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
    });
}
function initLocationTableWithExport() {
    $('#LocationListTable').DataTable({
        dom: 'Bfrtip',
        buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
    });
}
function initMaterialEntryTableWithExport() {
    $('#MaterialEntryListTable').DataTable({
        dom: 'Bfrtip',
        buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
    });
}
// Custom Tables
window.initCustomDataTable = (data) => {
    new DataTable('#CustomDataTable', {
        destroy: true,
        pageLength: 5,
        lengthChange: false,
        dom: 'flrtip' // 'l' (length), 'r' (processing), 't' (table), 'i' (info), 'p' (pagination), 'f' (filter/search)
        // Eğer arama kutusu görünmüyorsa 'f' ekleyin:
        // dom: 'frtip'
    });
};
window.initCustomDataTable1 = () => {
    // Eğer tablo daha önce başlatıldıysa yok et
    if ($.fn.DataTable.isDataTable('#CustomDataTable1')) {
        $('#CustomDataTable1').DataTable().destroy();
    }
    new DataTable('#CustomDataTable1', {
        pageLength: 5,
        lengthChange: false,
        dom: 'frtip'
    });
};
