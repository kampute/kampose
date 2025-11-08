/**
 * Setup link behaviors.
 *
 * This function initializes link behaviors, such as marking external links and
 * setting up popups for link to text and image files.
 */
function setupLinks() {
    const origin = window.location.origin;
    const links = document.querySelectorAll('a');
    const popupExtensions = window.kampose.config.popupFileExtensions || [];

    links.forEach(link => {
        if (!link.href) {
            return;
        }

        const url = new URL(link.href);

        if (origin !== url.origin) {
            link.classList.add('external-link');
        } else if (isPopupFileType(url.pathname, popupExtensions)) {
            link.classList.add('popup-link');
        }
    });

    document.querySelectorAll('.popup-link').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            openLinkInPopup(link.href);
        });
    });

    function isPopupFileType(path, extensions) {
        if (path.endsWith('.html') || path.endsWith('/')) {
            return false;
        }

        if (path.endsWith('/LICENSE') || path.endsWith('/DISCLAIMER')) {
            return true;
        }

        if (extensions.length === 0) {
            return false;
        }

        const lastDot = path.lastIndexOf('.');
        const lastSegment = path.lastIndexOf('/');
        const ext = lastDot > lastSegment ? path.substring(lastDot + 1) : '';
        return ext && extensions.includes(ext.toLowerCase());
    }

    function openLinkInPopup(href) {
        const url = new URL(href);
        const title = url.pathname.split('/').pop();

        fetch(href)
            .then(response => response.blob())
            .then(async blob => {
                const mimeType = blob.type || '';

                // Fallback to normal navigation for HTML files
                if (mimeType === 'text/html') {
                    window.location.href = href;
                    return;
                }

                // Convert unknown types to plain text for safer display
                if (!mimeType.startsWith('image/') && mimeType !== 'application/pdf' && mimeType !== 'application/svg+xml') {
                    blob = new Blob([blob], { type: 'text/plain' });
                }

                const blobUrl = URL.createObjectURL(blob);
                openPopup(blobUrl, title);
            })
            .catch(() => openPopup(href, title));
    }
}
