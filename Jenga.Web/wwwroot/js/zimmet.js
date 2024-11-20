$(document).ready(function () {
    var selectedMalzemeId = 0;
    $('#MalzemeDropdown').change(function () {
        selectedMalzemeId = $(this).val();
        $.ajax({
            //url: '@Url.Action("GetMalzemeYeri", "MalzemeYeriTanim")',
            url: '/Admin/MalzemeYeriTanim/GetMalzemeYeri',
            type: 'GET',
            data: {
                onlyExistingMalzeme: true,
                malzemeId: selectedMalzemeId,
            },
            success: function (data) {
                var malzemeYeriDropdown = $('#MalzemeYeriDropdown');
                malzemeYeriDropdown.empty(); // Clear the existing options
                malzemeYeriDropdown.append('<option value="">Select a MalzemeYeri</option>');
                $.each(data, function (index, item) {
                    malzemeYeriDropdown.append('<option value="' + item.Value + '">' + item.text + '</option>');
                    //malzemeYeriDropdown.append('<option value="' + item.id + '" data-adet="' + item.adetSum + '">' + item.adi + ' (' + item.adetSum + ')</option>');
                });
            }
        });

        // Refill Personel dropdown
        $.ajax({
            url: '/Admin/Personel/GetPersonel',
            type: 'GET',
            data: {
                onlyWorkingPersonel:false,
                malzemeId: selectedMalzemeId,
            },
            success: function (data) {
                var personelDropdown = $('#PersonelDropdown');
                personelDropdown.empty();
                personelDropdown.append('<option value="">Select a Personel</option>');
                $.each(data, function (index, item) {
                    //var adet = "";
                    //if (item.adetSum > 0)
                    //    adet = ' (' + item.adetSum + ')';
                    //personelDropdown.append('<option value="' + item.id + '">' + item.adi + adet + '</option>');
                    personelDropdown.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
            }
        });

    });
    $('#MalzemeYeriDropdown').change(function () {
        var selectedOption = $(this).find(':selected');
        var maxAdet = selectedOption.data('adet') || 0;
        $('#AdetInput').attr('max', maxAdet); // Set the max attribute on the Adet input
    });
});