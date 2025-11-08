/**
 * Setup breadcrumb visibility and ellipsis.
 *
 * This function adjusts the visibility of breadcrumb items based on the available width
 * of the breadcrumb container. It hides items that do not fit and adds an ellipsis
 * to indicate that there are more items that are not currently visible.
 *
 * @returns {boolean} True if breadcrumb was set up, false otherwise.
 */
function setupBreadcrumbEllipsis() {
    const nav = document.querySelector('.breadcrumb');
    if (!nav) return;

    const list = nav.querySelector('ol') || nav.querySelector('ul');
    if (!list) return;

    const items = Array.from(list.children);

    adjustBreadcrumbVisibility(items, list)
    window.addEventListener('resize', debounce(() => adjustBreadcrumbVisibility(items, list), 100));

    function adjustBreadcrumbVisibility(items, list) {
        resetBreadcrumbItems(items, list);
        const availableWidth = getAvailableBreadcrumbWidth(list);
        if (availableWidth > 0) {
            let ellipsis = null;
            for (let i = 0; i < items.length - 1 && list.scrollWidth > availableWidth; i++) {
                items[i].style.display = 'none';
                if (!ellipsis) {
                    ellipsis = createEllipsis();
                    list.insertBefore(ellipsis, items[i]);
                }
            }
        }
    }

    function resetBreadcrumbItems(items, list) {
        const ellipsis = list.querySelector('.ellipsis');
        if (ellipsis) {
            list.removeChild(ellipsis);
            items.forEach(li => li.style.display = '');
        }
    }

    function createEllipsis() {
        const ellipsis = document.createElement('li');
        ellipsis.className = 'ellipsis';
        ellipsis.textContent = '…';
        return ellipsis;
    }

    function getAvailableBreadcrumbWidth(list) {
        let container = list.parentElement;
        while (container && container !== document.body && container !== document.documentElement) {
            const style = window.getComputedStyle(container);
            if (
                style.maxWidth !== 'none' ||
                style.width !== 'auto' ||
                style.overflow === 'hidden' ||
                style.overflowX === 'hidden'
            ) break;
            container = container.parentElement;
        }

        let availableWidth = container ? container.clientWidth : window.innerWidth;

        const listStyle = window.getComputedStyle(list);
        availableWidth -= (parseFloat(listStyle.marginLeft) || 0) + (parseFloat(listStyle.marginRight) || 0);

        const parent = list.parentElement;
        if (parent) {
            const parentStyle = window.getComputedStyle(parent);
            availableWidth -= (parseFloat(parentStyle.paddingLeft) || 0) + (parseFloat(parentStyle.paddingRight) || 0);

            const usedWidth = Array.from(parent.children).filter(child =>
                child !== list &&
                window.getComputedStyle(child).display !== 'none'
            ).reduce((total, sibling) => {
                const style = window.getComputedStyle(sibling);
                return total + sibling.offsetWidth + (parseFloat(style.marginLeft) || 0) + (parseFloat(style.marginRight) || 0);
            }, 0);
            availableWidth -= usedWidth;
        }

        return availableWidth;
    }
}
