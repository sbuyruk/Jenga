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

function initLocationTableWithExport() {
    $('#LocationListTable').DataTable({
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
        dom: 'flrtip'
    });
};

window.initCustomDataTable1 = () => {
    if ($.fn.DataTable.isDataTable('#CustomDataTable1')) {
        $('#CustomDataTable1').DataTable().destroy();
    }
    new DataTable('#CustomDataTable1', {
        pageLength: 5,
        lengthChange: false,
        dom: 'frtip'
    });
};
