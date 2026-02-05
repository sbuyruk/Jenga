window.initScrollSpy = (scrollElementId, navElementId, offset) => {
    const scrollEl = document.getElementById(scrollElementId);
    const navEl = document.getElementById(navElementId);

    if (!scrollEl || !navEl || !window.bootstrap?.ScrollSpy) {
        return;
    }

    // varsa eski instance'ı temizle (yeniden render vb. durumlar için)
    const existing = window.bootstrap.ScrollSpy.getInstance(scrollEl);
    if (existing) {
        existing.dispose();
    }

    new window.bootstrap.ScrollSpy(scrollEl, {
        target: `#${navElementId}`,
        rootMargin: `-${offset || 0}px 0px -40%`,
        smoothScroll: true
    });

    // anchor sayısı dinamik değişebilir -> refresh
    window.bootstrap.ScrollSpy.getInstance(scrollEl)?.refresh();
};