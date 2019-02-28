$('li.dropdown :first-child').not('.textonly').on('click', function () {
    var $el = $(this).parent();
    if ($el.hasClass('open')) {
        var $a = $el.children('a.dropdown-toggle');
        if ($a.length && $a.attr('href')) {
            location.href = $a.attr('href');
        }
    }
});