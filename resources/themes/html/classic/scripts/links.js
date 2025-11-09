/**
 * Setup link behaviors.
 *
 * This function initializes link behaviors, such as marking external links and
 * setting up popups for link to text and image files.
 */
function setupLinks() {
    const origin = window.location.origin;
    const links = document.querySelectorAll('a');
    const popupAssetNames = (window.kampose.config.popupAssetNames || []).map(name => name.toLowerCase());

    links.forEach(link => {
        if (!link.href) {
            return;
        }

        const url = new URL(link.href);

        if (origin !== url.origin) {
            link.classList.add('external-link');
        } else if (isPopupFileType(url.pathname, popupAssetNames)) {
            link.classList.add('popup-link');
        }
    });

    document.querySelectorAll('.popup-link').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            openLinkInPopup(link.href);
        });
    });

    function isPopupFileType(path, patterns) {
        if (patterns.length === 0 || path.endsWith('/')) {
            return false;
        }

        const filename = path.substring(path.lastIndexOf('/') + 1).toLowerCase();
        for (const pattern of patterns) {
            if (pattern.startsWith('*')) {
                const ext = pattern.slice(1);
                if (filename.endsWith(ext)) {
                    return true;
                }
            } else if (filename === pattern) {
                return true;
            }
        }

        return false;
    }

    function openLinkInPopup(href) {
        const url = new URL(href);
        const title = url.pathname.split('/').pop();

        fetch(href)
            .then(response => response.blob())
            .then(async blob => {
                const mimeType = blob.type || '';

                let isDisplayable = mimeType.startsWith('image/') || [
                    'text/plain',
                    'application/pdf',
                    'application/svg+xml'
                ].includes(mimeType);

                // Attempt to display small files as text (max. 16 KiB)
                if (!isDisplayable && blob.size <= 16384) {
                    try {
                        const text = await blob.text();
                        // Heuristic: check for null bytes to avoid obvious binary files.
                        // This may not catch all binary files, and some text files could be misclassified.
                        if (!text.includes('\0')) {
                            blob = new Blob([text], { type: 'text/plain' });
                            isDisplayable = true;
                        }
                    } catch {
                        // Ignore conversion errors
                    }
                }

                if (isDisplayable) {
                    const blobUrl = URL.createObjectURL(blob);
                    openPopup(blobUrl, title, () => URL.revokeObjectURL(blobUrl));
                } else {
                    window.location.href = href;
                }
            })
            .catch(() => openPopup(href, title));
    }
}
