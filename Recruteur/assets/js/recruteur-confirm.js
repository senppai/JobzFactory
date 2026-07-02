(function () {
    var modal = document.getElementById('jf-rc-confirm-modal');
    if (!modal) return;

    var backdrop = modal.querySelector('.jf-rc-confirm__backdrop');
    var iconEl = modal.querySelector('.jf-rc-confirm__icon');
    var titleEl = modal.querySelector('.jf-rc-confirm__title');
    var messageEl = modal.querySelector('.jf-rc-confirm__message');
    var cancelBtn = modal.querySelector('.jf-rc-confirm__cancel');
    var confirmBtn = modal.querySelector('.jf-rc-confirm__confirm');
    var pendingAction = null;

    var icons = {
        danger: '!',
        warning: '!',
        primary: '✓'
    };

    function closeModal() {
        modal.classList.remove('is-open');
        modal.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('jf-rc-modal-open');
        pendingAction = null;
    }

    function openModal(options) {
        var variant = options.variant || 'primary';
        titleEl.textContent = options.title || 'Are you sure?';
        messageEl.textContent = options.message || 'Please confirm this action.';
        confirmBtn.textContent = options.confirmText || 'Confirm';
        iconEl.textContent = icons[variant] || icons.primary;
        iconEl.className = 'jf-rc-confirm__icon jf-rc-confirm__icon--' + variant;
        confirmBtn.className = 'jf-rc-btn jf-rc-confirm__confirm jf-rc-confirm__confirm--' + variant;

        modal.classList.add('is-open');
        modal.setAttribute('aria-hidden', 'false');
        document.body.classList.add('jf-rc-modal-open');
        confirmBtn.focus();
    }

    document.addEventListener('click', function (e) {
        var el = e.target.closest('[data-confirm-title]');
        if (!el) return;

        e.preventDefault();

        pendingAction = function () {
            if (el.tagName === 'A' && el.href) {
                window.location.href = el.href;
                return;
            }

            var form = el.closest('form');
            if (form) {
                form.submit();
            }
        };

        openModal({
            title: el.getAttribute('data-confirm-title'),
            message: el.getAttribute('data-confirm-message'),
            confirmText: el.getAttribute('data-confirm-btn'),
            variant: el.getAttribute('data-confirm-variant')
        });
    });

    confirmBtn.addEventListener('click', function () {
        if (typeof pendingAction === 'function') {
            pendingAction();
        }
        closeModal();
    });

    cancelBtn.addEventListener('click', closeModal);
    backdrop.addEventListener('click', closeModal);

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && modal.classList.contains('is-open')) {
            closeModal();
        }
    });

})();
