window.initRolTableWithExport = (data) => {

    new DataTable('#RolListTable', {
        layout: {
            topStart: {
                buttons: ['copy', 'csv', 'excel', 'pdf', 'print']
            }
        },
        data: data,
        columns: [
            { title: 'Rol Adı', data: 'adi' },
            { title: 'Açıklama', data: 'aciklama' }
        ]
    });

};
