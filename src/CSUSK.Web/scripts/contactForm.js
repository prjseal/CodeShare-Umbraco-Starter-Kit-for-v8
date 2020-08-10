var contactForm = contactForm || {
    init: function () {
        this.listeners();
    },
    listeners: function () {
        $(document).on('click', '.contact-submit', function (e) {
            e.preventDefault();
            var form = $('#contact-form');
            form.submit();
        })
    },
    showResult: function () {
        $("#form-outer").hide('slow');
        $("#form-result").show('slow');
    }
};

contactForm.init();

function showSendButton() {
    $('.contact-clean form .btn-primary').prop('disabled', false);
}

function hideSendButton() {
    $('.contact-clean form .btn-primary').prop('disabled', true);
}