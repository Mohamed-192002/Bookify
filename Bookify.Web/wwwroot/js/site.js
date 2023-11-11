var table;
var datatable;
var UpdatedRow;
function OnModelBegin() {
    $('body :submit').attr('disabled', 'disabled');
}
function ShowMessageSuccessfully(message = 'Saved succssfully!') {
    Swal.fire({
        title: "success",
        text: message,
        icon: "success",
        buttonsStyling: false,
        confirmButtonText: "Ok",
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}
function ShowMessageError(message = 'Something went wrong!') {
    Swal.fire({
        icon: 'error',
        title: 'Oops...',
        text: message.responseText != undefined ? message.responseText : message
    });
}
function OnModelComplete() {
    $('body:submit').removeAttr('disabled');
}

function OnModelSuccess(item) {
    ShowMessageSuccessfully();
    $('#Modal').modal('hide');
    if (UpdatedRow !== undefined) {
        datatable.row(UpdatedRow).remove().draw();
        UpdatedRow = undefined;
    }
    var NewRow = $(item);
    datatable.row.add(NewRow).draw();

    KTMenu.init();
    KTMenu.initHandlers();
    KTMenu.initGlobalHandlers();
}

// Data Table
var KTDatatables = function () {
    // Private functions
    var initDatatable = function () {
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'pageLength': 10,
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = 'Customer Orders Report';
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ':not(:last-child)',
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ':not(:last-child)',
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ':not(:last-child)',
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: ':not(:last-child)',
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-dataTable');
            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();

$(document).ready(function () {

    // Call Data Table
    KTUtil.onDOMContentLoaded(function () {
        KTDatatables.init();
    });

    //  bootbox
    var msg = $('#Message').text();
    if (msg !== '') {
        ShowMessageSuccessfully(msg)
    }

    // Model
    $('body').delegate('.js-render-modal', 'click', function () {
        var modal = $('#Modal');
        var btn = $(this);
        modal.find('.modal-title').text(btn.data('title'));

        if (btn.data('update') !== undefined) {
            UpdatedRow = btn.parents('tr');
        }
        $.get({
            url: btn.data('url'),
            success: function (form) {
                modal.find('.modal-body').html(form);
                $.validator.unobtrusive.parse(modal);

                $('.js-data-ajax').select2();
                $('.js-data-ajax').on('select2:select', function (e) {
                    $(form).validate().element('#' + $(this).attr('id'));
                });
            },
            error: function () {
                ShowMessageError()
            },
        });
        modal.modal('show');
    });

    // Handel Toggel Status
    $('body').delegate('.js-toggle-status', "click", function () {
        var btn = $(this);
        bootbox.confirm({
            message: 'Are you sure that you need to toggle this item status?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-primary '
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: btn.data('url'),
                        success: function (LastUpdatedOn) {
                            var status = btn.parents('tr').find('.js-status');
                            var newText = status.text() == "Deleted" ? "Available" : "Deleted";
                            status.text(newText);
                            status.toggleClass("badge-light-success badge-light-danger");
                            btn.parents('tr').find('.js-update').text(LastUpdatedOn);
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
})