$(document).ready(function () {
    $('#SelectedIlIdDropDown').on('change', function () {
        var selectedIlId = $(this).val();

        // İlçeleri dinamik olarak yükleme
        if (selectedIlId) {
            $.get('/Admin/Lokasyon/GetIlceByIl', { ilId: selectedIlId }, function (data) {
                var ilceDropdown = $('#SelectedIlceIdDropDown');
                ilceDropdown.empty(); // Önce temizle
                ilceDropdown.append('<option value="" disabled selected>-- İlçe Seçiniz --</option>');

                // Gelen ilçeleri dropdown'a ekle
                $.each(data, function (index, ilce) {
                    ilceDropdown.append('<option value="' + ilce.value + '">' + ilce.text + '</option>');
                });
            });
        }
    });
});