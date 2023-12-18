$(document).ready(function () {
    $('#GovernorateId').on('change', function () {
        var governorateId = $(this).val();
        $('#AreaId').empty();
        $('#AreaId').append('<option></option>');
        if (governorateId != '') {
            $.ajax({
                url: '/Subscripers/GetAreas?governorateId=' + governorateId,
                success: function (areas) {
                    $.each(areas, function (i, area) {
                        $('#AreaId').append($('<option></option>').attr('value', area.value).text(area.text));
                    });
                },
                error: function () {
                    ShowMessageError();
                },
            });
        }
    });
});  