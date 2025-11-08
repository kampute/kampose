/**
 * Setup site navigation.
 *
 * This function initializes the site navigation, building the navigation structure
 * based on the provided sitemap.
 *
 * @returns {boolean} True if site navigation was set up, false otherwise.
 */
function setupSiteNavigation() {
    const siteNav = document.getElementById('site-navigation');
    if (!siteNav) return false;

    const currentUrl = getCurrentPageUrl();
    const baseUrl = siteNav.getAttribute('data-base-url') || '';

    const builder = getNavigationBuilder();

    const navContent = builder();
    if (navContent.children.length === 0) return false;

    siteNav.appendChild(navContent);

    setupSearch();
    setupScroll(siteNav);
    return true;

    function getNavigationBuilder() {
        const apiSection = window.kampose.sitemap.find(section => section.title === 'API');
        return apiSection
            ? () => buildApiNavigation(apiSection)
            : () => buildTopicsNavigation();
    }

    function buildApiNavigation(apiSection) {
        const hasNamespacePages = window.kampose.config.hasNamespacePages;
        const groupByNamespace = window.kampose.config.groupTypesByNamespace;
        const expandedLevels = hasNamespacePages && groupByNamespace ? 1 : 0;
        const items = !hasNamespacePages || groupByNamespace ? apiSection.items : flattenItems(apiSection.items);

        const mainList = document.createElement('ul');
        mainList.className = 'nav-items nav-api';

        items.forEach(item => {
            const navItem = createNavItem(item, expandedLevels);
            mainList.appendChild(navItem);
        });

        return mainList;
    }

    function buildTopicsNavigation() {
        const mainList = document.createElement('ul');
        mainList.className = 'nav-items nav-topics';

        const topics = getTopicsFromSitemap();
        topics.forEach(topic => {
            const topicItem = createNavItem(topic);
            mainList.appendChild(topicItem);
        });

        return mainList;
    }

    function getCurrentPageUrl() {
        const href = window.location.pathname
        return href.endsWith('.html')
            ? href
            : href.endsWith('/')
                ? href + 'index.html'
                : href + '.html';
    }

    function flattenItems(items) {
        return items
            .flat()
            .map(item => item.items)
            .flat()
            .sort((a, b) => a.title.localeCompare(b.title))
    }

    function createNavItem(data, expandedLevels = 0, maxLevel = Infinity, currentLevel = 0) {
        currentLevel++;
        const listItem = document.createElement('li');
        listItem.className = 'nav-item';

        if (data.url) {
            if (currentUrl.endsWith('/' + data.url)) {
                listItem.classList.add('active');
            }
            const link = document.createElement('a');
            link.href = baseUrl + data.url;
            link.textContent = data.title;
            listItem.appendChild(link);
        } else {
            listItem.setAttribute('aria-expanded', 'false');
            const span = document.createElement('span');
            span.textContent = data.title;
            span.addEventListener('click', (e) => {
                e.stopPropagation();
                listItem.setAttribute('aria-expanded', listItem.classList.toggle('expanded') ? 'true' : 'false');
            });
            listItem.appendChild(span);
        }

        if (currentLevel < maxLevel && data.items?.length) {
            const childList = document.createElement('ul');
            childList.className = 'nav-items';
            childList.setAttribute('role', 'group');

            let hasActiveChild = false;
            data.items.forEach(item => {
                const childListItem = createNavItem(item, expandedLevels, maxLevel, currentLevel);
                childList.appendChild(childListItem);
                hasActiveChild = hasActiveChild || childListItem.classList.contains('active') || childListItem.classList.contains('expanded');
            });

            listItem.appendChild(childList);

            if (currentLevel > expandedLevels) {
                listItem.classList.add('expandable');
                if (hasActiveChild || listItem.classList.contains('active')) {
                    listItem.classList.add('expanded');
                    listItem.setAttribute('aria-expanded', 'true');
                } else {
                    listItem.setAttribute('aria-expanded', 'false');
                }
            }
        }

        return listItem;
    }

    function filterNavigation(searchTerm) {
        const noResultsMsg = siteNav.querySelector('.no-search-results');
        const allNavItems = siteNav.querySelectorAll('.nav-item');

        if (!searchTerm) {
            noResultsMsg?.classList.add('hidden');
            removeFilterHighlights(allNavItems);
            return;
        }

        const escapedSearchTerm = escapeHtml(searchTerm);
        const query = new RegExp(escapeRegex(escapedSearchTerm), 'ig');

        let anyMatches = false;
        allNavItems.forEach(item => {
            item.classList.add('hidden');
            const link = item.querySelector('a');
            if (!link) return;

            const text = link.dataset.originalText || link.textContent;
            const escapedText = escapeHtml(text);
            if (!query.test(escapedText)) {
                clearLinkHighlights(link);
                return;
            }

            anyMatches = true;
            link.dataset.originalText = text;
            link.innerHTML = escapedText.replace(query, '<mark>$&</mark>');

            item.classList.remove('hidden');
            for (let parent = item.parentElement; parent && parent !== siteNav; parent = parent.parentElement) {
                parent.classList.remove('hidden');
                if (parent.classList.contains('expandable')) {
                    parent.classList.add('expanded');
                }
                if (parent.hasAttribute('aria-expanded')) {
                    parent.setAttribute('aria-expanded', 'true');
                }
            }
        });

        noResultsMsg?.classList.toggle('hidden', anyMatches);
    }

    function removeFilterHighlights(allNavItems) {
        allNavItems.forEach(item => {
            const link = item.querySelector('a');
            if (link) {
                clearLinkHighlights(link);
            }
            item.classList.remove('hidden');
            if (item.classList.contains('expandable')) {
                item.classList.remove('expanded');
            }
            if (item.hasAttribute('aria-expanded')) {
                item.setAttribute('aria-expanded', 'false');
            }
        });

        const activeItem = siteNav.querySelector('.nav-item.active');
        if (activeItem) {
            for (let parent = activeItem.parentElement; parent && parent !== siteNav; parent = parent.parentElement) {
                if (parent.classList.contains('expandable')) {
                    parent.classList.add('expanded');
                }
                if (parent.hasAttribute('aria-expanded')) {
                    parent.setAttribute('aria-expanded', 'true');
                }
            }
        }
    }

    function clearLinkHighlights(link) {
        if (link && link.dataset.originalText) {
            link.textContent = link.dataset.originalText;
            delete link.dataset.originalText;
        }
    }

    function setupSearch() {
        const searchInput = document.getElementById('nav-search');
        if (!searchInput) return;

        searchInput.addEventListener('input', debounce(() => {
            clearButton?.classList.toggle('hidden', searchInput.value.length === 0);
            filterNavigation(searchInput.value.toLowerCase().trim());
        }, 250));

        const clearButton = document.getElementById('clear-search');
        if (clearButton) {
            clearButton.addEventListener('click', () => {
                clearButton.classList.add('hidden');
                searchInput.focus();
                filterNavigation(searchInput.value = '');
            });
        }

        document.addEventListener('keydown', (e) => {
            if (e.target.matches('input, textarea, [contenteditable]')) return;

            if (e.key === '/' || (e.key === 'f' && e.ctrlKey)) {
                e.preventDefault();
                searchInput.focus();
            }
        });

        searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                if (searchInput.value) {
                    clearButton?.classList.add('hidden');
                    filterNavigation(searchInput.value = '');
                } else {
                    searchInput.blur();
                }
            }
        });

        return true;
    }

    function setupScroll(siteNav) {
        const navItem = siteNav.querySelector('.nav-item.active');
        if (navItem) {
            const savedScrollTop = retrieveFromLocalStorage('navigation-scroll-top');
            if (savedScrollTop) {
                siteNav.scrollTop = parseInt(savedScrollTop, 10);
            }
            if (!isInView(navItem, siteNav)) {
                navItem.scrollIntoView({ behavior: 'instant', block: 'nearest' });
            }
        }

        window.addEventListener('beforeunload', () => {
            storeInLocalStorage('navigation-scroll-top', siteNav.scrollTop || null);
        });
    }
}