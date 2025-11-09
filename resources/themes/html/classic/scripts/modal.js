/**
 * Setup the popup modal.
 *
 * This function initializes the popup modal, allowing it to display content
 * in an overlay.
 *
 * @returns {boolean} True if the popup was set up, false otherwise.
 */
function setupPopup() {
    const overlay = document.getElementById('modal-overlay');
    if (!overlay) return false;

    overlay.onclick = function (e) {
        if (e.target === overlay) {
            e.preventDefault();
            closePopup();
        }
    };

    const closeButton = document.getElementById('modal-close');
    if (closeButton) {
        closeButton.onclick = closePopup;
    }

    document.addEventListener('keydown', e => {
        if (e.key === 'Escape' && overlay.classList.contains('open')) {
            e.preventDefault();
            closePopup();
        }
    });

    function closePopup() {
        overlay.classList.remove('open');
        overlay.setAttribute('aria-hidden', 'true');
        setTimeout(() => {
            document.getElementById('modal-iframe').src = '';
        }, 300);

        if (overlay.onClose) {
            const callback = overlay.onClose;
            overlay.onClose = null;
            try {
                callback();
            } catch (e) {
                console.error('Error in modal onClose callback:', e);
            }
        }
    }

    return true;
}

/**
 * Displays content of a given URL in a popup modal or new window.
 *
 * The function assumes that the URL is a viewable document.
 *
 * @param {string} src The source URL to display in the popup.
 * @param {string} caption The caption or title for the popup.
 * @param {function} onClose A callback function to execute when the popup is closed.
 */
function openPopup(src, caption, onClose) {
    if (!src) return;
    const iframe = document.getElementById('modal-iframe');
    if (iframe) {
        iframe.src = src;
        const title = document.getElementById('modal-title');
        if (title) {
            title.textContent = caption || '';
        }
        const overlay = document.getElementById('modal-overlay');
        overlay.classList.add('open');
        overlay.setAttribute('aria-hidden', 'false');
        overlay.onClose = onClose ?? null;
    } else {
        const width = window.innerWidth * 0.6;
        const height = window.innerHeight * 0.7;
        const left = (window.innerWidth - width) / 2;
        const top = (window.innerHeight - height) / 2;
        window.open(src, '_blank', `left=${left},top=${top},width=${width},height=${height},resizable=yes,scrollbars=yes,status=no`);
    }
}
