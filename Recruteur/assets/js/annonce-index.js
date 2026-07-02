(function () {
    'use strict';

    function init() {
        var container = document.getElementById('offer-list-container');
        if (!container) return;

        var searchInput = document.getElementById('offer-search');
        var sectorSelect = document.getElementById('offer-sector');
        var citySelect = document.getElementById('offer-city');
        var filterTabs = document.querySelectorAll('.jf-rc-filter-tab');
        var summaryEl = document.getElementById('offer-list-summary');
        var baseUrl = container.getAttribute('data-base-url');
        var debounceTimer = null;
        var currentStatus = getActiveStatus();
        var restoringHistory = false;

        function getActiveStatus() {
            var active = document.querySelector('.jf-rc-filter-tab.is-active');
            return active ? active.getAttribute('data-status') : 'all';
        }

        function getSelectValue(select) {
            if (!select) return '';
            if (window.jQuery && jQuery.fn.select2 && jQuery(select).hasClass('select2-hidden-accessible')) {
                var val = jQuery(select).val();
                return val ? String(val) : '';
            }
            return select.value || '';
        }

        function buildUrl(page) {
            var params = new URLSearchParams();
            if (page && page > 1) params.set('Page', page);
            if (searchInput && searchInput.value.trim()) params.set('search', searchInput.value.trim());
            if (currentStatus && currentStatus !== 'all') params.set('status', currentStatus);

            var sector = getSelectValue(sectorSelect);
            var city = getSelectValue(citySelect);
            if (sector) params.set('sector', sector);
            if (city) params.set('city', city);

            var qs = params.toString();
            return qs ? baseUrl + '?' + qs : baseUrl;
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

        function updateSummary(results) {
            if (!summaryEl || !results) return;
            var total = results.getAttribute('data-total');
            var onPage = results.getAttribute('data-on-page');
            var page = results.getAttribute('data-page');
            var pageCount = results.getAttribute('data-page-count');

            if (total === '0') {
                summaryEl.textContent = 'No offers match your filters.';
                return;
            }

            if (pageCount && parseInt(pageCount, 10) > 1) {
                summaryEl.textContent = 'Showing ' + onPage + ' of ' + total + ' offers (page ' + page + ' of ' + pageCount + ')';
            } else {
                summaryEl.textContent = total + ' offer' + (total === '1' ? '' : 's') + ' in your account';
            }
        }

        function loadOffers(page, pushState) {
            var url = buildUrl(page);
            container.classList.add('is-loading');

            fetch(url, {
                method: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                cache: 'no-store'
            })
                .then(function (response) {
                    if (!response.ok) throw new Error('Request failed');
                    return response.text();
                })
                .then(function (html) {
                    container.innerHTML = html;
                    updateSummary(container.querySelector('#offer-list-results'));

                    if (pushState !== false && window.history && window.history.pushState) {
                        window.history.pushState({ offersPage: true }, '', url);
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
            loadOffers(1, true);
        }

        function bindFilterDropdowns() {
            if (!window.jQuery) {
                setTimeout(bindFilterDropdowns, 50);
                return;
            }

            if (window.jfRcInitSelects) {
                window.jfRcInitSelects();
            }

            var $sector = jQuery('#offer-sector');
            var $city = jQuery('#offer-city');

            $sector.off('.jfOffers')
                .on('select2:select.jfOffers select2:clear.jfOffers change.jfOffers', onFilterChange);
            $city.off('.jfOffers')
                .on('select2:select.jfOffers select2:clear.jfOffers change.jfOffers', onFilterChange);
        }

        function applyUrlParams(params) {
            restoringHistory = true;
            if (searchInput) searchInput.value = params.get('search') || '';
            currentStatus = params.get('status') || 'all';
            filterTabs.forEach(function (t) {
                t.classList.toggle('is-active', t.getAttribute('data-status') === currentStatus);
            });
            setSelectValue(sectorSelect, params.get('sector'));
            setSelectValue(citySelect, params.get('city'));
            restoringHistory = false;
        }

        container.addEventListener('click', function (e) {
            var link = e.target.closest('.jf-rc-pagination a');
            if (!link || !container.contains(link)) return;
            e.preventDefault();
            var href = link.getAttribute('href');
            if (!href) return;

            container.classList.add('is-loading');
            fetch(href, {
                method: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                cache: 'no-store'
            })
                .then(function (response) {
                    if (!response.ok) throw new Error('Request failed');
                    return response.text();
                })
                .then(function (html) {
                    container.innerHTML = html;
                    updateSummary(container.querySelector('#offer-list-results'));
                    if (window.history && window.history.pushState) {
                        window.history.pushState({ offersPage: true }, '', href);
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
                    loadOffers(1, true);
                }, 350);
            });
        }

        filterTabs.forEach(function (tab) {
            tab.addEventListener('click', function () {
                filterTabs.forEach(function (t) { t.classList.remove('is-active'); });
                tab.classList.add('is-active');
                currentStatus = tab.getAttribute('data-status');
                loadOffers(1, true);
            });
        });

        window.addEventListener('popstate', function () {
            if (window.location.pathname.toLowerCase().indexOf('/annonce') !== -1) {
                var params = new URLSearchParams(window.location.search);
                applyUrlParams(params);

                fetch(window.location.href, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    cache: 'no-store'
                })
                    .then(function (r) { return r.text(); })
                    .then(function (html) {
                        container.innerHTML = html;
                        updateSummary(container.querySelector('#offer-list-results'));
                    });
            }
        });

        bindFilterDropdowns();
        updateSummary(container.querySelector('#offer-list-results'));
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
