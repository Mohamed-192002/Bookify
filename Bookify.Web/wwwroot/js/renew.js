$(document).ready(function () {
    $('.js-renew').on('click', function () {

        var key = $(this).data('key');

        bootbox.confirm({
            message: 'Are you sure that you need to renew this subscription?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-primary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: '/Subscripers/RenewSubscription?sKey=' + key,
                        success: function (row) {
                            $('#subscriptionTable').find('tbody').append(row);
                            $('#StatusBadge').text("Active Subscriper").removeClass().addClass("badge badge-light-success d-inline");
                            $('#RewardClass').removeClass().addClass("card bg-success hoverable h-md-100");
                            $('#RewardStutes').text("Active Subscriper")
                            ShowMessageSuccessfully();
                        },
                        error: function () {
                            ShowMessageError();
                        },
                    });
                }
            }
        });



    });
});