/**
 * Setup the menu bar with custom items and topics.
 *
 * This function builds the menu bar based on the the configured menu items.
 * If an element in the menu items or the sub-items array is asterisk (*)
 * instead of an object, it will be replaced with the documentation topics
 * from the sitemap.
 *
 * @returns {boolean} True if the menu bar was set up, false otherwise.
 */
function setupMenuBar() {
    const menuBar = document.getElementById('menubar');
    if (!menuBar) return false;

    const items = Array.isArray(window.kampose.config.menuItems)
        ? window.kampose.config.menuItems
        : [];

    if (items.length === 0) return false;

    const baseUrl = menuBar.getAttribute('data-base-url') || '';
    const menuItems = buildMenuItems(items, baseUrl, 0);

    if (menuItems.children.length === 0) return false;

    const existingMenuItems = menuBar.querySelector('.menu');
    if (existingMenuItems) {
        existingMenuItems.prepend(...menuItems.children);
    } else {
        menuBar.appendChild(menuItems);
    }

    setupSubmenuToggles(menuBar);
    return true;

    function buildMenuItems(items, baseUrl, depth) {
        const menuList = document.createElement('ul');
        menuList.className = 'menu';
        menuList.setAttribute('role', 'menu');
        menuList.setAttribute('tabindex', '-1');

        items.forEach(item => {
            if (!item) return;

            if (typeof item === 'object') {
                const menuItem = buildMenuItem(item, baseUrl, depth);
                menuList.appendChild(menuItem);
            } else if (item === '*') {
                const topics = getTopicsFromSitemap();
                topics.forEach(topic => {
                    const topicItem = buildMenuItem(topic, baseUrl, depth);
                    menuList.appendChild(topicItem);
                });
            } else if (item === '-') {
                const divider = buildMenuDivider();
                menuList.appendChild(divider);
            } else if (typeof item === 'string') {
                const topic = getTopicFromSitemapBtSlug(item);
                if (topic) {
                    const topicItem = buildMenuItem(topic, baseUrl, depth);
                    menuList.appendChild(topicItem);
                }
            }
        });

        return menuList;
    }

    function buildMenuItem(item, baseUrl, depth) {
        const menuItem = document.createElement('li');
        menuItem.setAttribute('role', 'menuitem');
        menuItem.setAttribute('aria-label', item.title);
        menuItem.setAttribute('tabindex', '-1');
        menuItem.dataset.menuDepth = depth;
        menuItem.style.setProperty('--menu-depth', depth);
        menuItem.className = 'menu-item';

        const link = document.createElement('a');
        link.href = item.url ? (isRelativeUrl(item.url) ? baseUrl + item.url : item.url) : null;
        link.textContent = item.title;

        menuItem.appendChild(link);

        if (item.items && Array.isArray(item.items) && item.items.length > 0) {
            const submenu = buildMenuItems(item.items, baseUrl, depth + 1);
            const toggle = document.createElement('button');
            toggle.type = 'button';
            toggle.className = 'submenu-toggle';
            toggle.setAttribute('aria-label', `Toggle ${item.title} submenu`);
            toggle.setAttribute('aria-expanded', 'false');
            const toggleIcon = document.createElement('span');
            toggleIcon.className = 'icon icon-chevron';
            toggleIcon.setAttribute('aria-hidden', 'true');
            toggle.appendChild(toggleIcon);

            menuItem.setAttribute('aria-haspopup', 'true');
            menuItem.setAttribute('aria-expanded', 'false');
            menuItem.classList.add('has-submenu');
            menuItem.appendChild(toggle);
            menuItem.appendChild(submenu);
            activateDropdown(menuItem);
        }

        return menuItem;
    }

    function buildMenuDivider() {
        const divider = document.createElement('li');
        divider.className = 'divider';
        divider.setAttribute('role', 'separator');
        divider.setAttribute('tabindex', '-1');
        return divider;
    }

    function setupSubmenuToggles(menuBar) {
        const closeItem = (item) => {
            item.classList.remove('open');
            delete item.dataset.openedByClick;
            item.setAttribute('aria-expanded', 'false');
            item.querySelector(':scope > .submenu-toggle')?.setAttribute('aria-expanded', 'false');
            item.querySelectorAll('.has-submenu.open').forEach(closeItem);
        };

        const closeOtherItems = (activeItem) => {
            menuBar.querySelectorAll('.has-submenu.open').forEach(item => {
                if (item !== activeItem && !item.contains(activeItem)) closeItem(item);
            });
        };

        menuBar.addEventListener('click', (event) => {
            if (!(event.target instanceof Element)) return;

            const toggle = event.target.closest('.submenu-toggle');
            if (!toggle || !menuBar.contains(toggle)) return;

            event.preventDefault();
            event.stopPropagation();

            const item = toggle.closest('.has-submenu');
            if (!item) return;

            const isExpanded = !item.classList.contains('open');
            closeOtherItems(item);
            item.classList.toggle('open', isExpanded);
            item.dataset.openedByClick = isExpanded ? 'true' : '';
            if (!isExpanded) delete item.dataset.openedByClick;
            item.setAttribute('aria-expanded', isExpanded ? 'true' : 'false');
            toggle.setAttribute('aria-expanded', isExpanded ? 'true' : 'false');
        });

        document.addEventListener('click', (event) => {
            if (event.target instanceof Node && !menuBar.contains(event.target)) {
                menuBar.querySelectorAll('.has-submenu.open').forEach(closeItem);
            }
        });

        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape') {
                menuBar.querySelectorAll('.has-submenu.open').forEach(closeItem);
            }
        });
    }
}
