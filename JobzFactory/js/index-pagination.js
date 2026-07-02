(function () {
    'use strict';

    function init() {
        var container = document.getElementById('job-list-container');
        if (!container) return;

        var searchInput = document.getElementById('job-search');
        var sectorSelect = document.getElementById('job-sector');
        var citySelect = document.getElementById('job-city');
        var baseUrl = container.getAttribute('data-base-url') || '/';
        var debounceTimer = null;
        var restoringHistory = false;

        function getSelectValue(select) {
            if (!select) return '';
            if (window.jQuery && jQuery.fn.select2 && jQuery(select).hasClass('select2-hidden-accessible')) {
                var val = jQuery(select).val();
                return val ? String(val) : '';
            }
            return select.value || '';
        }

        function setSelectValue(select, value) {
            if (!select) return;
            var nextValue = value || '';

            if (window.jQuery && jQuery.fn.select2) {
                jQuery(select).val(nextValue || null).trigger('change.select2');
                return;
            }

            select.value = nextValue;
        }

        function buildUrl(page) {
            var params = new URLSearchParams();
            if (page && page > 1) params.set('page', page);
            if (searchInput && searchInput.value.trim()) params.set('search', searchInput.value.trim());

            var sector = getSelectValue(sectorSelect);
            var city = getSelectValue(citySelect);
            if (sector) params.set('sector', sector);
            if (city) params.set('city', city);

            var qs = params.toString();
            return qs ? baseUrl + '?' + qs : baseUrl;
        }

        function updateStats(results) {
            if (!results) return;

            var total = results.getAttribute('data-total');
            var onPage = results.getAttribute('data-on-page');
            var pageCount = results.getAttribute('data-page-count');
            var page = results.getAttribute('data-page');

            var totalEls = [
                document.getElementById('hero-total-count'),
                document.getElementById('hero-total-stat'),
                document.getElementById('jobs-total-count')
            ];
            totalEls.forEach(function (el) {
                if (el && total !== null) el.textContent = total;
            });

            var onPageEl = document.getElementById('hero-on-page-count');
            var pageCountEl = document.getElementById('hero-page-count');
            var summaryEl = document.getElementById('jobs-list-summary');
            var paginationInfo = document.getElementById('jf-pagination-info');

            if (onPageEl && onPage !== null) onPageEl.textContent = onPage;
            if (pageCountEl && pageCount !== null) pageCountEl.textContent = pageCount;

            if (summaryEl) {
                if (total === '0') {
                    summaryEl.textContent = 'No jobs match your filters.';
                } else if (pageCount && parseInt(pageCount, 10) > 1) {
                    summaryEl.textContent = 'Showing ' + onPage + ' of ' + total + ' jobs (page ' + page + ' of ' + pageCount + ')';
                } else {
                    summaryEl.textContent = total + ' job' + (total === '1' ? '' : 's') + ' matching your criteria';
                }
            }

            if (paginationInfo && total !== '0' && pageCount && parseInt(pageCount, 10) > 1) {
                paginationInfo.textContent = 'Showing ' + onPage + ' of ' + total + ' — Page ' + page + ' of ' + pageCount;
            }
        }

        function loadJobs(page, pushState) {
            var url = buildUrl(page);
            container.classList.add('is-loading');

            fetch(url, {
                method: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                cache: 'no-store',
                credentials: 'same-origin'
            })
                .then(function (response) {
                    if (!response.ok) throw new Error('Request failed');
                    return response.text();
                })
                .then(function (html) {
                    container.innerHTML = html;
                    updateStats(container.querySelector('#job-list-results'));

                    if (pushState !== false && window.history && window.history.pushState) {
                        window.history.pushState({ jobPage: true }, '', url);
                    }
                })
                .catch(function () {
                    window.location.href = url;
                })
                .finally(function () {
                    container.classList.remove('is-loading');
                });
        }

        function onFilterChange() {
            if (restoringHistory) return;
            loadJobs(1, true);
        }

        function initSelect2(select) {
            if (!select || !window.jQuery || !jQuery.fn.select2) return;

            var $el = jQuery(select);
            if ($el.hasClass('select2-hidden-accessible')) return;

            $el.select2({
                width: '100%',
                allowClear: true,
                placeholder: select.getAttribute('data-placeholder') || 'All',
                minimumResultsForSearch: 0,
                language: {
                    noResults: function () { return 'No matches'; },
                    searching: function () { return 'Searching…'; }
                }
            });

            $el.on('select2:open', function () {
                window.setTimeout(function () {
                    var field = document.querySelector('.select2-container--open .select2-search__field');
                    if (field) {
                        field.placeholder = select.getAttribute('data-search-placeholder') || 'Search…';
                        field.focus();
                    }
                }, 0);
            });
        }

        function bindFilterDropdowns() {
            if (!window.jQuery || !jQuery.fn.select2) {
                window.setTimeout(bindFilterDropdowns, 50);
                return;
            }

            initSelect2(sectorSelect);
            initSelect2(citySelect);

            var $sector = jQuery('#job-sector');
            var $city = jQuery('#job-city');

            $sector.off('.jfJobs')
                .on('select2:select.jfJobs select2:clear.jfJobs change.jfJobs', onFilterChange);
            $city.off('.jfJobs')
                .on('select2:select.jfJobs select2:clear.jfJobs change.jfJobs', onFilterChange);
        }

        function applyUrlParams(params) {
            restoringHistory = true;
            if (searchInput) searchInput.value = params.get('search') || '';
            setSelectValue(sectorSelect, params.get('sector'));
            setSelectValue(citySelect, params.get('city'));
            restoringHistory = false;
        }

        container.addEventListener('click', function (e) {
            var link = e.target.closest('.jf-pagination a');
            if (!link || !container.contains(link)) return;

            e.preventDefault();
            var href = link.getAttribute('href');
            if (!href) return;

            container.classList.add('is-loading');
            fetch(href, {
                method: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                cache: 'no-store',
                credentials: 'same-origin'
            })
                .then(function (response) {
                    if (!response.ok) throw new Error('Request failed');
                    return response.text();
                })
                .then(function (html) {
                    container.innerHTML = html;
                    updateStats(container.querySelector('#job-list-results'));
                    if (window.history && window.history.pushState) {
                        window.history.pushState({ jobPage: true }, '', href);
                    }
                })
                .catch(function () {
                    window.location.href = href;
                })
                .finally(function () {
                    container.classList.remove('is-loading');
                });
        });

        if (searchInput) {
            searchInput.addEventListener('input', function () {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(function () {
                    loadJobs(1, true);
                }, 350);
            });
        }

        window.addEventListener('popstate', function () {
            var path = window.location.pathname.toLowerCase();
            if (path.indexOf('default') !== -1 || path === '/' || path.endsWith('/')) {
                applyUrlParams(new URLSearchParams(window.location.search));

                container.classList.add('is-loading');
                fetch(window.location.href, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    cache: 'no-store',
                    credentials: 'same-origin'
                })
                    .then(function (response) {
                        if (!response.ok) throw new Error('Request failed');
                        return response.text();
                    })
                    .then(function (html) {
                        container.innerHTML = html;
                        updateStats(container.querySelector('#job-list-results'));
                    })
                    .catch(function () {
                        window.location.reload();
                    })
                    .finally(function () {
                        container.classList.remove('is-loading');
                    });
            }
        });

        bindFilterDropdowns();
        updateStats(container.querySelector('#job-list-results'));
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
