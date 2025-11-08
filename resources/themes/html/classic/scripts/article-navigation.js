/**
 * Setup article navigation.
 *
 * This function initializes the article navigation by creating a table of contents
 * based on the headings in the article content. It also sets up a scroll spy feature
 * to highlight the current section in the navigation as the user scrolls through the
 * article.
 *
 * @returns {boolean} True if navigation was set up, false otherwise.
 */
function setupArticleNavigation() {
    const content = document.getElementById('article');
    if (!content) return false;

    const articleNav = document.getElementById('article-navigation');
    if (!articleNav) return false;

    const minLevel = 2;
    const maxLevel = Math.min(6, parseInt(articleNav.dataset.maxLevel || '6', 10));

    if (minLevel > maxLevel) return false;

    const headingSelectors = Array.from(
        { length: maxLevel - minLevel + 1 },
        (_, i) => `h${i + minLevel}`
    ).join(',');

    const headings = Array.from(content.querySelectorAll(headingSelectors));

    if (headings.length === 0) return false;

    const seenIds = new Set();
    headings.forEach((heading, index) => {
        if (!heading.id) heading.id = `heading-${index}`;
        for (let i = 1; seenIds.has(heading.id); i++) {
            heading.id = `${heading.id}-${i}`;
        }
        seenIds.add(heading.id);
    });

    buildNavigation(articleNav, content, headings, maxLevel);
    setupScrollSpy(articleNav, content, headings);
    return true;

    function buildNavigation(articleNav, content, headings, maxLevel) {
        let prevLevel = 1;
        let lastItem = null;
        const stack = [articleNav];
        headings.forEach(heading => {
            const level = parseInt(heading.tagName[1], 10);
            if (level > maxLevel) return;

            if (level > prevLevel) {
                for (let i = prevLevel; i < level; i++) {
                    const list = document.createElement('ul');
                    list.className = 'nav-items';
                    if (lastItem) {
                        lastItem.appendChild(list);
                    } else {
                        stack[stack.length - 1].appendChild(list);
                    }
                    stack.push(list);
                }
            } else if (level < prevLevel) {
                for (let i = prevLevel; i > level; i--) {
                    stack.pop();
                }
            }

            prevLevel = level;

            const li = document.createElement('li');
            li.className = 'nav-item';

            const a = document.createElement('a');
            a.href = `#${heading.id}`;
            a.textContent = heading.textContent;
            a.onclick = function (e) {
                e.preventDefault();
                content.scrollTo({
                    top: heading.offsetTop,
                    behavior: 'smooth'
                });
            }

            li.appendChild(a);
            stack[stack.length - 1].appendChild(li);
            lastItem = li;
        });
    }

    function findActiveHeading(headings) {
        const header = document.querySelector('.header');
        const headerHeight = header ? header.offsetHeight : 0;
        const viewTop = headerHeight + 10;
        for (let i = headings.length - 1; i >= 0; i--) {
            const heading = headings[i];
            const rect = heading.getBoundingClientRect();
            if (rect.top <= viewTop) {
                return heading;
            }
        }
        return null;
    }

    function setupScrollSpy(articleNav, content, headings) {
        let ticking = false;

        const tocLinks = {};
        articleNav.querySelectorAll('a').forEach(link => {
            const id = link.getAttribute('href').substring(1);
            tocLinks[id] = link;
        });

        const onScroll = () => {
            if (ticking) return;
            ticking = true;

            const heading = findActiveHeading(headings);
            const tocItem = heading ? tocLinks[heading.id]?.parentElement : null;

            articleNav.querySelectorAll('.active').forEach(item => item.classList.remove('active'));
            if (tocItem) {
                tocItem.classList.add('active');
                if (!isInView(tocItem, articleNav)) {
                    tocItem.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                }
            }

            ticking = false;
        }

        content.addEventListener('resize', debounce(() => onScroll(), 500));
        content.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
    }
}