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
    const menuItems = buildMenuItems(items, baseUrl);

    const existingMenuItems = menuBar.querySelector('.menu');
    if (existingMenuItems) {
        existingMenuItems.prepend(...menuItems.children);
    } else {
        menuBar.appendChild(menuItems);
    }

    return true;

    function buildMenuItems(items, baseUrl) {
        const menuList = document.createElement('ul');
        menuList.className = 'menu';
        menuList.setAttribute('role', 'menu');
        menuList.setAttribute('tabindex', '-1');

        items.forEach(item => {
            if (!item) return;

            if (typeof item === 'object') {
                const menuItem = buildMenuItem(item, baseUrl);
                menuList.appendChild(menuItem);
            } else if (item === '*') {
                const topics = getTopicsFromSitemap();
                topics.forEach(topic => {
                    const topicItem = buildMenuItem(topic, baseUrl);
                    menuList.appendChild(topicItem);
                });
            } else if (item === '-') {
                const divider = buildMenuDivider();
                menuList.appendChild(divider);
            } else if (typeof item === 'string') {
                const topic = getTopicFromSitemapBtSlug(item);
                if (topic) {
                    const topicItem = buildMenuItem(topic, baseUrl);
                    menuList.appendChild(topicItem);
                }
            }
        });

        return menuList;
    }

    function buildMenuItem(item, baseUrl) {
        const menuItem = document.createElement('li');
        menuItem.setAttribute('role', 'menuitem');
        menuItem.setAttribute('aria-label', item.title);
        menuItem.setAttribute('tabindex', '-1');
        menuItem.className = 'menu-item';

        const link = document.createElement('a');
        link.href = item.url ? (isRelativeUrl(item.url) ? baseUrl + item.url : item.url) : null;
        link.textContent = item.title;

        menuItem.appendChild(link);

        if (item.items && Array.isArray(item.items) && item.items.length > 0) {
            const submenu = buildMenuItems(item.items, baseUrl);
            menuItem.setAttribute('aria-haspopup', 'true');
            menuItem.setAttribute('aria-expanded', 'false');
            menuItem.classList.add('has-submenu');
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
}