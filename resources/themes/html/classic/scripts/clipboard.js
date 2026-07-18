/**
 * Copy text using the modern Clipboard API, with a fallback for local files
 * and browsers where clipboard access is unavailable or denied.
 *
 * @param {string} text The text to copy.
 * @returns {Promise<boolean>} Whether the text was copied.
 */
async function copyTextToClipboard(text) {
    if (window.isSecureContext && navigator.clipboard?.writeText) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch {
            // Try the fallback below when clipboard permission is denied.
        }
    }

    const activeElement = document.activeElement;
    const textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.setAttribute('readonly', '');
    textarea.setAttribute('aria-hidden', 'true');
    textarea.style.position = 'fixed';
    textarea.style.opacity = '0';
    textarea.style.pointerEvents = 'none';
    document.body.appendChild(textarea);

    textarea.select();
    textarea.setSelectionRange(0, textarea.value.length);

    let copied = false;
    try {
        copied = document.execCommand('copy');
    } catch {
        copied = false;
    }

    textarea.remove();
    activeElement?.focus?.();
    return copied;
}

/**
 * Announce the result of a copy action to assistive technologies.
 *
 * @param {string} message The message to announce.
 */
function announceCopyStatus(message) {
    let status = document.getElementById('copy-status');
    if (!status) {
        status = document.createElement('span');
        status.id = 'copy-status';
        status.className = 'visually-hidden copy-status';
        status.setAttribute('role', 'status');
        status.setAttribute('aria-live', 'polite');
        status.setAttribute('aria-atomic', 'true');
        document.body.appendChild(status);
    }

    // Clear first so repeated copies announce the same message.
    status.textContent = '';
    requestAnimationFrame(() => {
        status.textContent = message;
    });
}
