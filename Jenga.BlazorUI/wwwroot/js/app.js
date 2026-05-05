// ── Tema sistemi ────────────────────────────────────────────────
window.themeStorage = {
    apply: (themeKey) => {
        document.documentElement.setAttribute('data-theme', themeKey);
    },
    get: (key) => {
        try { return localStorage.getItem(key); } catch { return null; }
    },
    set: (key, value) => {
        try { localStorage.setItem(key, value); } catch { }
    }
};

// Sayfa ilk yüklendiğinde kaydedilmiş temayı uygula (flash olmadan)
(function () {
    try {
        const saved = localStorage.getItem('jenga-theme');
        if (saved) document.documentElement.setAttribute('data-theme', saved);
    } catch { }
})();

window.initScrollSpy = (scrollElementId, navElementId, offset) => {
    const scrollEl = document.getElementById(scrollElementId);
    const navEl = document.getElementById(navElementId);

    if (!scrollEl || !navEl || !window.bootstrap?.ScrollSpy) {
        return;
    }

    const existing = window.bootstrap.ScrollSpy.getInstance(scrollEl);
    if (existing) {
        existing.dispose();
    }

    new window.bootstrap.ScrollSpy(scrollEl, {
        target: `#${navElementId}`,
        rootMargin: `-${offset || 0}px 0px -40%`,
        smoothScroll: true
    });

    window.bootstrap.ScrollSpy.getInstance(scrollEl)?.refresh();
};

window.scrollSpyScrollTo = (scrollElementId, sectionId) => {
    const scrollEl = document.getElementById(scrollElementId);
    const target = document.getElementById(sectionId);

    if (!scrollEl || !target) {
        return;
    }

    const containerRect = scrollEl.getBoundingClientRect();
    const targetRect = target.getBoundingClientRect();
    const top = scrollEl.scrollTop + (targetRect.top - containerRect.top);
    scrollEl.scrollTo({ top, behavior: 'smooth' });
};

window.printElementById = (elementId) => {
    const el = document.getElementById(elementId);
    if (!el) return;

    const original = document.body.innerHTML;
    document.body.innerHTML = el.outerHTML;
    window.print();
    document.body.innerHTML = original;
    window.location.reload();
};

window.isMobileViewport = () => window.innerWidth < 768;