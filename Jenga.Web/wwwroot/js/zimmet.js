$(document).ready(function () {
    $('#MalzemeDropdown').change(function () {
        var malzemeId = $(this).val();
        $.ajax({
            url: '@Url.Action("GetMalzemeYeri", "MalzemeYeriTanim")',
            type: 'GET',
            data: { malzemeId: malzemeId },
            success: function (data) {
                var malzemeYeriDropdown = $('#MalzemeYeriDropdown');
                malzemeYeriDropdown.empty(); // Clear the existing options
                malzemeYeriDropdown.append('<option value="">Select a MalzemeYeri</option>');
                $.each(data, function (index, item) {
                    malzemeYeriDropdown.append('<option value="' + item.id + '" data-adet="' + item.adetSum + '">' + item.adi + ' (' + item.adetSum + ')</option>');
                });
            }
        });
        // Refill Personel dropdown
        $.ajax({
            url: '@Url.Action("GetPersonelByMalzeme", "Zimmet")',
            type: 'GET',
            data: { malzemeId: malzemeId },
            success: function (data) {
                var personelDropdown = $('#PersonelDropdown');
                personelDropdown.empty();
                personelDropdown.append('<option value="">Select a Personel</option>');
                $.each(data, function (index, item) {
                    var adet = "";
                    if (item.adetSum > 0)
                        adet = ' (' + item.adetSum + ')';
                    personelDropdown.append('<option value="' + item.id + '">' + item.adi + adet + '</option>');
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