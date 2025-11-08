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
        } else if (!url.pathname.endsWith('.html') && isPopupFileType(url.pathname, popupExtensions)) {
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
        const lastDot = path.lastIndexOf('.');
        const lastSegment = path.lastIndexOf('/');
        const ext = lastDot > lastSegment ? path.substring(lastDot + 1) : '';
        return extensions.includes(ext.toLowerCase());
    }

    function openLinkInPopup(href) {
        const title = href.split('/').pop();
        fetch(href)
            .then(response => response.blob())
            .then(blob => {
                const blobType = blob.type || 'application/octet-stream';
                switch (true) {
                    case blobType.startsWith('text/'):
                    case blobType.startsWith('image/'):
                    case blobType === 'application/pdf':
                        break;
                    case blobType === 'application/octet-stream' && !href.split('/').pop().includes('.'):
                        blob = new Blob([blob], { type: 'text/plain' });
                        break;
                    default:
                        window.location.href = href;
                        return;
                }

                const objectUrl = URL.createObjectURL(blob)
                openPopup(objectUrl, title);
            })
            .catch(() => openPopup(href, title));
    }
}
