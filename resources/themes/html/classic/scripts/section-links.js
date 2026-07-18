/**
 * Add copy controls to article section headings that have generated IDs.
 *
 * @returns {boolean} True when at least one heading was enhanced.
 */
function setupSectionLinks() {
    const article = document.getElementById('article');
    if (!article) return false;

    const headings = article.querySelectorAll('h2[id], h3[id], h4[id], h5[id], h6[id]');
    if (headings.length === 0) return false;

    headings.forEach(heading => {
        if (heading.querySelector(':scope > .section-link-button')) return;

        const headingText = heading.textContent.trim();
        const defaultLabel = headingText ? `Copy link to ${headingText}` : 'Copy link';

        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'section-link-button';
        button.setAttribute('aria-label', defaultLabel);
        button.title = 'Copy link';

        const icon = document.createElement('span');
        icon.className = 'icon icon-link';
        icon.setAttribute('aria-hidden', 'true');
        button.appendChild(icon);
        heading.appendChild(button);

        button.addEventListener('click', async () => {
            clearTimeout(button.copyResetTimer);

            // Treat generated IDs as opaque canonical fragments. In particular,
            // do not assign through URL.hash, which could re-encode API hashes.
            const pageUrl = window.location.href.split('#', 1)[0];
            const copied = await copyTextToClipboard(`${pageUrl}#${heading.id}`);
            const message = copied ? 'Link copied' : 'Unable to copy link';

            button.classList.toggle('copied', copied);
            button.classList.toggle('copy-failed', !copied);
            button.setAttribute('aria-label', message);
            button.title = message;
            icon.classList.toggle('icon-link', !copied);
            icon.classList.toggle('icon-check', copied);
            announceCopyStatus(message);

            button.copyResetTimer = setTimeout(() => {
                button.classList.remove('copied', 'copy-failed');
                button.setAttribute('aria-label', defaultLabel);
                button.title = 'Copy link';
                icon.classList.remove('icon-check');
                icon.classList.add('icon-link');
            }, 2000);
        });
    });

    return true;
}
