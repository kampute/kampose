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
        if (!extensions.length || path.endsWith('.html') || path.endsWith('/')) {
            return false;
        }
        const lastDot = path.lastIndexOf('.');
        const lastSegment = path.lastIndexOf('/');
        const ext = lastDot > lastSegment ? path.substring(lastDot + 1) : '';
        return extensions.includes(ext.toLowerCase());
    }

    function openLinkInPopup(href) {
        const url = new URL(href);
        const title = url.pathname.split('/').pop() || url.hostname;

        fetch(href)
            .then(response => response.blob())
            .then(async blob => {
                const mimeType = blob.type || '';

                // Directly open images and PDFs in popup
                if (mimeType.startsWith('image/') || mimeType === 'application/pdf') {
                    openPopup(href, title);
                    return;
                }

                // Try to get text data URL for renderable text content
                const textDataUrl = await getTextDataUrl(blob);
                if (textDataUrl) {
                    openPopup(textDataUrl, title);
                    return;
                }

                // Unsupported content: redirect instead of popup
                window.location.href = href;
            })
            .catch(() => openPopup(href, title));
    }

    async function getTextDataUrl(blob) {
        const mimeType = blob.type || '';

        // Exclude HTML content
        if (mimeType === 'text/html') {
            return null;
        }

        // Known text types that are safe to render
        const isKnownTextType = mimeType.startsWith('text/') || [
            'application/json',
            'application/xml',
            'application/javascript',
            'application/typescript',
            'application/x-yaml',
            'application/yaml',
            'application/toml',
            'application/x-sh',
            'application/x-python',
            'application/sql',
            'application/graphql',
            'application/x-httpd-php'
        ].includes(mimeType);

        if (isKnownTextType) {
            try {
                const text = await blob.text();
                return 'data:text/plain;base64,' + btoa(text);
            } catch {
                return null;
            }
        }

        // Unknown or binary MIME type - check content heuristically
        if (blob.size < 1024) {
            try {
                const text = await blob.text();
                // If it contains null bytes, it's likely binary
                if (!text.includes('\0')) {
                    return 'data:text/plain;base64,' + btoa(text);
                }
            } catch {
                return null;
            }
        }

        return null;
    }
}
