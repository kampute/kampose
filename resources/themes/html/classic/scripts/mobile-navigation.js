/**
 * Setup mobile navigation controls.
 *
 * This function wires the mobile header buttons to the off-canvas site navigation
 * and compact main menu panel.
 *
 * @returns {boolean} True if mobile navigation was set up, false otherwise.
 */
function setupMobileNavigation() {
    const mobileQuery = window.matchMedia('(max-width: 1024px)');
    const navButton = document.getElementById('mobile-nav-toggle');
    const menuButton = document.getElementById('mobile-menu-toggle');
    const navPanel = document.getElementById('site-navigation-panel');
    const menuPanel = document.getElementById('menubar');
    const backdrop = document.getElementById('mobile-backdrop');

    if (!backdrop || (!navButton && (!menuButton || !menuPanel))) return false;

    let activePanel = null;

    const isMobile = () => mobileQuery.matches;

    const setBackdrop = (visible) => {
        backdrop.hidden = !visible;
        document.body.classList.toggle('mobile-panel-open', visible);
    };

    const setButtonState = (button, isOpen) => {
        if (!button) return;
        button.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
    };

    const setPanelState = (panelName, isOpen) => {
        document.body.classList.toggle(`${panelName}-open`, isOpen);
        if (panelName === 'mobile-nav') setButtonState(navButton, isOpen);
        if (panelName === 'mobile-menu') setButtonState(menuButton, isOpen);
    };

    const setMenuExpansion = (isExpanded) => {
        if (!menuPanel) return;

        menuPanel.querySelectorAll('.has-submenu').forEach(item => {
            item.classList.remove('open');
            delete item.dataset.openedByClick;
            item.setAttribute('aria-expanded', isExpanded ? 'true' : 'false');
        });
        menuPanel.querySelectorAll('.submenu-toggle').forEach(button => {
            button.setAttribute('aria-expanded', isExpanded ? 'true' : 'false');
        });
    };

    const focusPanel = (panelName) => {
        if (panelName === 'mobile-nav') {
            const searchInput = document.getElementById('nav-search');
            (searchInput || navPanel)?.focus();
            return;
        }

        const menuLink = menuPanel?.querySelector('.menu-item > a');
        (menuLink || menuPanel).focus();
    };

    const closePanels = () => {
        setPanelState('mobile-nav', false);
        setPanelState('mobile-menu', false);
        setMenuExpansion(false);
        setBackdrop(false);
        activePanel = null;
    };

    const openPanel = (panelName) => {
        if (!isMobile()) return;

        setPanelState('mobile-nav', panelName === 'mobile-nav');
        setPanelState('mobile-menu', panelName === 'mobile-menu');
        setMenuExpansion(false);
        setBackdrop(true);
        activePanel = panelName;
        window.setTimeout(() => focusPanel(panelName), 0);
    };

    const togglePanel = (panelName) => {
        if (activePanel === panelName) {
            closePanels();
        } else {
            openPanel(panelName);
        }
    };

    navButton?.addEventListener('click', () => togglePanel('mobile-nav'));
    menuButton?.addEventListener('click', () => togglePanel('mobile-menu'));
    backdrop.addEventListener('click', closePanels);

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && activePanel) closePanels();
    });

    menuPanel?.addEventListener('click', (event) => {
        if (!isMobile()) return;
        if (!(event.target instanceof Element)) return;

        if (event.target.closest('a[href]')) closePanels();
    });

    navPanel?.addEventListener('click', (event) => {
        if (!(event.target instanceof Element)) return;
        if (isMobile() && event.target.closest('a[href]')) closePanels();
    });

    mobileQuery.addEventListener('change', () => closePanels());
    return true;
}
