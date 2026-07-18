/**
 * Add copy controls to code blocks.
 *
 * The controls are added with JavaScript so code blocks remain usable when
 * scripting is unavailable and templates do not need special copy markup.
 *
 * @returns {boolean} True when at least one code block was enhanced.
 */
function setupCodeCopy() {
    const codeBlocks = document.querySelectorAll('pre > code');
    if (codeBlocks.length === 0) return false;

    codeBlocks.forEach(code => {
        const pre = code.parentElement;
        if (!pre || pre.parentElement?.classList.contains('code-copy-container')) return;

        const container = document.createElement('div');
        container.className = 'code-copy-container';
        pre.parentNode.insertBefore(container, pre);
        container.appendChild(pre);

        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'code-copy-button';
        button.setAttribute('aria-label', 'Copy code');
        button.title = 'Copy code';

        const icon = document.createElement('span');
        icon.className = 'icon icon-copy';
        icon.setAttribute('aria-hidden', 'true');
        button.appendChild(icon);
        container.appendChild(button);

        button.addEventListener('click', async () => {
            clearTimeout(button.copyResetTimer);

            const copied = await copyTextToClipboard(code.textContent ?? '');
            const message = copied ? 'Copied' : 'Unable to copy code';

            button.classList.toggle('copied', copied);
            button.classList.toggle('copy-failed', !copied);
            button.setAttribute('aria-label', message);
            button.title = message;
            icon.classList.toggle('icon-copy', !copied);
            icon.classList.toggle('icon-check', copied);

            announceCopyStatus(message);

            button.copyResetTimer = setTimeout(() => {
                button.classList.remove('copied', 'copy-failed');
                button.setAttribute('aria-label', 'Copy code');
                button.title = 'Copy code';
                icon.classList.remove('icon-check');
                icon.classList.add('icon-copy');
            }, 2000);
        });
    });

    return true;
}
