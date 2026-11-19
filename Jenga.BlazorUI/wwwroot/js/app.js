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

window.printElementById = (elementId, printTitle) => {
    const el = document.getElementById(elementId);
    if (!el) return;

    const original = document.body.innerHTML;
    const originalTitle = document.title;
    try {
        document.body.innerHTML = el.outerHTML;
        if (typeof printTitle === 'string' && printTitle.trim()) document.title = printTitle;
        window.print();
    } finally {
        document.body.innerHTML = original;
        document.title = originalTitle;
        window.location.reload();
    }
};

window.taskApprovalReport = {
    print: (elementId, title) => new Promise((resolve, reject) => {
        const element = document.getElementById(elementId);
        if (!element) { reject(new Error("Rapor bulunamadı.")); return; }
        const frame = document.createElement("iframe");
        frame.setAttribute("aria-hidden", "true");
        frame.style.cssText = "position:fixed;left:-10000px;top:0;width:1123px;height:794px;border:0;";
        frame.onload = async () => {
            try {
                const target = frame.contentWindow;
                await frame.contentDocument.fonts.ready;
                await Promise.all(Array.from(frame.contentDocument.images).map(img => img.complete ? Promise.resolve() :
                    new Promise(done => { img.onload = done; img.onerror = done; })));
                target.addEventListener("afterprint", () => frame.remove(), { once: true });
                target.focus();
                target.print();
                resolve();
            } catch (error) { frame.remove(); reject(error); }
        };
        const head = document.createElement("head");
        const base = document.createElement("base");
        base.href = document.baseURI;
        head.appendChild(base);
        const caption = document.createElement("title");
        caption.textContent = title || "TSKGV Görev Onayı";
        head.appendChild(caption);
        document.querySelectorAll('link[rel="stylesheet"], style').forEach(style => head.appendChild(style.cloneNode(true)));
        frame.srcdoc = "<!doctype html><html lang=\"tr\">" + head.outerHTML + "<body>" + element.outerHTML + "</body></html>";
        document.body.appendChild(frame);
    })
};

window.isMobileViewport = () => window.innerWidth < 768;

window.kontrolKonum = {
    al: () => new Promise(resolve => {
        if (!window.isSecureContext) {
            resolve({ hata: "Konum almak için uygulamayı HTTPS bağlantısı üzerinden açın." });
            return;
        }
        if (!navigator.geolocation) {
            resolve({ hata: "Tarayıcınız konum özelliğini desteklemiyor." });
            return;
        }
        navigator.geolocation.getCurrentPosition(
            position => resolve({ koordinat: `${position.coords.latitude.toFixed(6)}, ${position.coords.longitude.toFixed(6)}` }),
            error => resolve({ hata: ({
                1: "Konum izni verilmedi. Tarayıcının site izinlerinden konum erişimini açıp yeniden deneyin.",
                2: "Konum belirlenemedi. Cihazınızın konum ayarlarını kontrol edip yeniden deneyin.",
                3: "Konum isteği zaman aşımına uğradı. Yeniden deneyin."
            })[error.code] || "Konum alınamadı. Yeniden deneyin." }),
            { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 }
        );
    })
};
