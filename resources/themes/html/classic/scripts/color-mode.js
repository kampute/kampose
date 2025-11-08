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
            colorModeSelector.classList.remove('open');
        });
    });

    activateDropdown(colorModeSelector);
    return true;
}
