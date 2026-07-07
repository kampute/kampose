/**
 * Setup the color mode selector.
 *
 * This function initializes the color mode selector, allowing users to switch between
 * light, dark, and system color modes. It updates the icon based on the selected mode.
 * It also stores the selected mode in local storage for persistence across sessions.
 *
 * @returns {boolean} True if the color mode selector was set up, false otherwise.
 */
function setupColorModeSelector() {
    const colorModeSelector = document.getElementById('color-mode');
    if (!colorModeSelector) return false;

    const trigger = colorModeSelector.querySelector('.color-mode-trigger');
    const hoverQuery = window.matchMedia('(hover: hover) and (pointer: fine)');
    let closeTimeout;
    let clickOpened = false;

    const setExpanded = (isExpanded) => {
        window.clearTimeout(closeTimeout);
        colorModeSelector.classList.toggle('open', isExpanded);
        trigger?.setAttribute('aria-expanded', isExpanded ? 'true' : 'false');
    };

    const closeWithDelay = () => {
        window.clearTimeout(closeTimeout);
        closeTimeout = window.setTimeout(() => setExpanded(false), 150);
    };

    const setColorModeIcon = (selectedMode) => {
        const icon = colorModeSelector.querySelector('.selected-icon');
        if (!icon) return;

        const modes = ['light', 'dark', 'system'];
        selectedMode = modes.includes(selectedMode) ? selectedMode : 'system';
        modes.forEach(mode => icon.classList.toggle(`icon-${mode}-color`, mode === selectedMode));
    }

    setColorModeIcon(retrieveFromLocalStorage('color-mode'));

    colorModeSelector.querySelectorAll('.menu-item').forEach(item => {
        item.addEventListener('click', (e) => {
            e.preventDefault();
            const mode = item.dataset.mode;
            applyColorMode(mode);
            setColorModeIcon(mode);
            storeInLocalStorage('color-mode', ['light', 'dark'].includes(mode) ? mode : null);
            clickOpened = false;
            setExpanded(false);
            trigger?.blur();
        });
    });

    trigger?.setAttribute('aria-expanded', 'false');
    trigger?.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        clickOpened = !colorModeSelector.classList.contains('open');
        setExpanded(clickOpened);
    });

    document.addEventListener('click', (e) => {
        if (e.target instanceof Node && !colorModeSelector.contains(e.target)) {
            clickOpened = false;
            setExpanded(false);
        }
    });

    colorModeSelector.addEventListener('mouseenter', () => {
        if (hoverQuery.matches && !clickOpened) setExpanded(true);
    });

    colorModeSelector.addEventListener('mouseleave', () => {
        if (hoverQuery.matches && !clickOpened) closeWithDelay();
    });

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            clickOpened = false;
            setExpanded(false);
        }
    });

    return true;
}
