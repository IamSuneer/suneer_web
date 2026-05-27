/* ============================================================
   DARK MODE
   ============================================================ */
(function () {
    const root = document.getElementById('htmlRoot');
    const btn  = document.getElementById('themeToggle');
    const icon = document.getElementById('themeIcon');

    const DARK  = 'dark';
    const LIGHT = 'light';
    const KEY   = 'suneer-theme';

    function apply(theme) {
        root.setAttribute('data-bs-theme', theme);
        if (icon) {
            icon.className = theme === DARK ? 'bi bi-sun-fill' : 'bi bi-moon-fill';
        }
        localStorage.setItem(KEY, theme);
    }

    // Restore saved preference (default: light)
    apply(localStorage.getItem(KEY) || LIGHT);

    if (btn) {
        btn.addEventListener('click', function () {
            const current = root.getAttribute('data-bs-theme');
            apply(current === DARK ? LIGHT : DARK);
        });
    }
})();

/* ============================================================
   SCROLL ANIMATIONS  (fade-up on all .animate-up elements)
   ============================================================ */
document.addEventListener('DOMContentLoaded', function () {
    const animEls = document.querySelectorAll('.animate-up');
    if (!animEls.length) return;

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.15 });

    animEls.forEach(function (el) { observer.observe(el); });
});

/* ============================================================
   SKILL BAR ANIMATION
   ============================================================ */
document.addEventListener('DOMContentLoaded', function () {
    const bars = document.querySelectorAll('.skill-fill[data-width]');
    if (!bars.length) return;

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.style.width = entry.target.dataset.width + '%';
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.3 });

    bars.forEach(function (bar) { observer.observe(bar); });
});

/* ============================================================
   PROJECT FILTER
   ============================================================ */
document.addEventListener('DOMContentLoaded', function () {
    const pills    = document.querySelectorAll('.filter-pill');
    const cards    = document.querySelectorAll('.project-card-wrapper');
    if (!pills.length) return;

    pills.forEach(function (pill) {
        pill.addEventListener('click', function () {
            pills.forEach(function (p) { p.classList.remove('active'); });
            pill.classList.add('active');

            var tag = pill.dataset.filter;

            cards.forEach(function (card) {
                if (tag === 'all') {
                    card.style.display = '';
                } else {
                    var tags = (card.dataset.tags || '').toLowerCase();
                    card.style.display = tags.includes(tag.toLowerCase()) ? '' : 'none';
                }
            });
        });
    });
});
