(function () {
    function initSelects() {
        if (!window.jQuery || !jQuery.fn.select2) {
            return;
        }

        jQuery('.jf-rc-select').each(function () {
            var $el = jQuery(this);
            if ($el.hasClass('select2-hidden-accessible')) {
                return;
            }

            var placeholder = $el.data('placeholder') || 'Search or select…';
            var searchPlaceholder = $el.data('search-placeholder') || 'Type to search…';

            $el.select2({
                width: '100%',
                placeholder: placeholder,
                allowClear: true,
                minimumResultsForSearch: 0,
                language: {
                    noResults: function () { return 'No matches found'; },
                    searching: function () { return 'Searching…'; }
                }
            });

            $el.on('select2:open', function () {
                setTimeout(function () {
                    var field = document.querySelector('.select2-container--open .select2-search__field');
                    if (field) {
                        field.setAttribute('placeholder', searchPlaceholder);
                    }
                }, 0);
            });
        });
    }

    window.jfRcInitSelects = initSelects;

    if (window.jQuery) {
        jQuery(initSelects);
    } else {
        document.addEventListener('DOMContentLoaded', initSelects);
    }
})();
