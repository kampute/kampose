(function () {
    'use strict';

    applyColorMode(retrieveFromLocalStorage('color-mode'));

    document.addEventListener('DOMContentLoaded', () => {
        requestAnimationFrame(() => {
            setupColorModeSelector();
            setupMenuBar();
            setupSiteNavigation();
            setupArticleNavigation();
            setupBreadcrumbEllipsis();
            setupMobileNavigation();
        });

        setupLinks();
        setupPopup();
    });
})();
