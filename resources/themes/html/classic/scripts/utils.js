/**
 * Retrieve a value from localStorage.
 *
 * This function attempts to retrieve a value from localStorage using the provided key.
 * If the key does not exist or if localStorage is not available, it returns the default
 * value.
 *
 * @param {*} key The key to retrieve.
 * @param {*} defaultValue The default value to return if the key is not found.
 * @returns {*} The retrieved value or the default value.
 */
function retrieveFromLocalStorage(key, defaultValue = null) {
    try {
        return localStorage.getItem(key) ?? defaultValue;
    } catch (e) {
        // Return default value if localStorage is not available
        return defaultValue;
    }
}

/**
 * Store a value in localStorage.
 *
 * This function attempts to store a value in localStorage using the provided key.
 * If the value is null or undefined, it removes the key from localStorage.
 *
 * @param {*} key The key to store the value under.
 * @param {*} value The value to store.
 */
function storeInLocalStorage(key, value) {
    try {
        if (value === null || value === undefined) {
            localStorage.removeItem(key);
        } else {
            localStorage.setItem(key, value);
        }
    } catch (e) {
        // Ignore errors, e.g., if localStorage is not available
    }
}

/**
 * Debounce a function call.
 *
 * This function returns a debounced version of the provided function,
 * which delays its execution until after a specified wait time has passed
 * since the last time it was invoked.
 *
 * @param {Function} func The function to debounce.
 * @param {number} wait The wait time in milliseconds.
 * @returns {Function} The debounced function.
 */
function debounce(func, wait) {
    let timeout;
    return function () {
        const context = this;
        const args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(context, args), wait);
    };
}

/**
 * Escape HTML special characters in a string.
 *
 * This function replaces special HTML characters in the provided text
 * with their corresponding HTML entities.
 *
 * @param {string} text The text to escape.
 * @returns {string} The escaped text.
 */
function escapeHtml(text) {
    return text
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

/**
 * Escape special characters in a string for use in a regular expression.
 *
 * This function escapes special characters in the provided text so that
 * it can be safely used in a regular expression.
 *
 * @param string} text The text to escape.
 * @returns {string} The escaped text.
 */
function escapeRegex(text) {
    return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

/**
 * Check if an element is in view within a container.
 *
 * This function checks if the provided element is fully visible within the
 * bounds of the specified container element.
 *
 * @param {Element} element The element to check.
 * @param {Element} container The container element to check against.
 * @returns {boolean} True if the element is in view, false otherwise.
 */
function isInView(element, container) {
    const rect = element.getBoundingClientRect();
    const containerRect = container.getBoundingClientRect();
    return (
        rect.top >= containerRect.top &&
        rect.bottom <= containerRect.bottom &&
        rect.left >= containerRect.left &&
        rect.right <= containerRect.right
    );
}

/**
 * Check if a URL is relative.
 *
 * This function checks if the provided URL is a relative URL.
 *
 * @param {string} url The URL to check.
 * @returns {boolean} True if the URL is relative, false otherwise.
 */
function isRelativeUrl(url) {
    return !/^([a-zA-Z][a-zA-Z\d+.-]*:)?\/\//.test(url);
}

/**
 * Get the list of topics from the sitemap.
 *
 * This function retrieves the list of topics from the sitemap. Each element of the
 * returned array is an object containing the `title` and `url` of a topic. If the
 * a topic has subtopics, they are included as well in the `items` array.
 *
 * @returns {Array} An array of topic objects, each containing a title and URL.
 */
function getTopicsFromSitemap() {
    const topicsSection = window.kampose.sitemap.find(section => section.title === 'Topics');
    if (!topicsSection) return [];

    // Exclude home and API index pages
    return topicsSection.items.filter(item => {
        return item.url !== 'index.html' && item.url !== 'api/index.html';
    });
}

/**
 * Get a topic from the sitemap by its slug.
 *
 * @param {string} slug The slug of the topic to retrieve.
 * @returns {Object|null} The topic object if found, null otherwise.
 */
function getTopicFromSitemapBtSlug(slug) {
    const topicsSection = window.kampose.sitemap.find(section => section.title === 'Topics');
    if (!topicsSection) return null;

    const findTopic = (items, slug) => {
        for (const item of items) {
            if (item.url.endsWith(`${slug}.html`) || item.url.endsWith(`${slug}/index.html`)) {
                return item;
            }
            if (item.items) {
                const found = findTopic(item.items, slug);
                if (found) return found;
            }
        }
        return null;
    };

    return findTopic(topicsSection.items, slug);
}

/**
 * Activate a dropdown menu.
 *
 * This function sets up event listeners to open and close the dropdown menu
 * when the user hovers over it. It also ensures that the menu does not overflow
 * the viewport by flipping its placement if necessary.
 *
 * @param {Element} dropdown The dropdown element to activate.
 */
function activateDropdown(dropdown) {
    let hoverTimeout;

    const openMenu = () => {
        clearTimeout(hoverTimeout);
        requestAnimationFrame(() => {
            dropdown.classList.add('open');

            const menu = dropdown.querySelector('.menu');
            if (!menu) return;

            const viewWidth = document.documentElement.clientWidth || window.innerWidth;
            if (!viewWidth) return;

            const rect = menu.getBoundingClientRect();
            if (rect.left < 0 || rect.right > viewWidth) {
                menu.classList.toggle('flip-placement');
            }
        });
    };

    const closeMenu = () => {
        hoverTimeout = setTimeout(() => {
            dropdown.classList.remove('open');
        }, 100);
    };

    dropdown.addEventListener('mouseenter', openMenu);
    dropdown.addEventListener('mouseleave', closeMenu);
}

/**
 * Apply the selected color mode.
 *
 * This function applies the selected color mode to the document by
 * setting the appropriate CSS properties and attributes.
 *
 * @param {string} mode The color mode to apply ('light' or 'dark').
 */
function applyColorMode(mode) {
    const root = document.documentElement;
    if (['light', 'dark'].includes(mode)) {
        root.style.setProperty('color-scheme', mode);
        root.setAttribute('data-color-mode', mode);
    } else {
        root.style.removeProperty('color-scheme');
        root.removeAttribute('data-color-mode');
    }
}
