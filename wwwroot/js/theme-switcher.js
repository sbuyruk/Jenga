// Robust theme-switcher: init(), refresh(), applyStoredTheme()
// Safe to call multiple times; auto-inits on DOM ready.
// Full THEMES object taken from your sample.

(function () {
  const THEMES = {
    corsair: {
      label: 'Corsair',
      variants: {
        light: {
          vars: {
            '--p1': '#000080', '--p2': '#FF0000', '--p3': '#9E2A3A', '--p4': '#3A2525',
            '--bg': '#FFFFFF', '--surface': '#FFF8F8', '--text': '#1E1E1E', '--muted': '#6b6b6b',
            '--card-bg': '#ffffff', '--shadow': 'rgba(16,20,22,0.06)'
          },
          logoCandidates: ['logo-corsair-light.png','logo-corsair.png','logo-light.png']
        },
        dark: {
          vars: {
            '--p1': '#000080', '--p2': '#FF0000', '--p3': '#9E2A3A', '--p4': '#3A2525',
            '--bg': '#08080A', '--surface': 'rgba(255,255,255,0.03)', '--text': '#F6F6F6',
            '--muted': '#b58b8b', '--card-bg': 'rgba(255,255,255,0.02)', '--shadow': 'rgba(0,0,0,0.6)'
          },
          logoCandidates: ['logo-corsair-dark.png','logo-corsair.png','logo-dark.png']
        }
      }
    },
    serene: {
      label: 'Serene',
      variants: {
        light: {
          vars: {
            '--p1': '#333446','--p2': '#7F8CAA','--p3': '#B8CFCE','--p4': '#EAEFEF',
            '--bg': '#EAEFEF','--surface': '#B8CFCE','--text': '#1F2933','--muted': '#5f6b72',
            '--card-bg': '#ffffff','--shadow': 'rgba(16,20,22,0.06)'
          },
          logoCandidates: ['logo-serene-light.png','logo-serene.png','logo-light.png']
        },
        dark: {
          vars: {
            '--p1': '#333446','--p2': '#7F8CAA','--p3': '#B8CFCE','--p4': '#EAEFEF',
            '--bg': '#0F1114','--surface': 'rgba(255,255,255,0.03)','--text': '#EAEFEF',
            '--muted': '#99a4ad','--card-bg': 'rgba(255,255,255,0.02)','--shadow': 'rgba(0,0,0,0.5)'
          },
          logoCandidates: ['logo-serene-dark.png','logo-serene.png','logo-dark.png']
        }
      }
    },
    graphite: {
      label: 'Graphite',
      variants: {
        light: {
          vars: {
            '--p1': '#DDDDDD','--p2': '#222831','--p3': '#30475E','--p4': '#F05454',
            '--bg': '#F7F7F7','--surface': '#FFFFFF','--text': '#222831','--muted': '#5a6670',
            '--card-bg': '#ffffff','--shadow': 'rgba(16,20,22,0.06)'
          },
          logoCandidates: ['logo-graphite-light.png','logo-graphite.png','logo-light.png']
        },
        dark: {
          vars: {
            '--p1': '#DDDDDD','--p2': '#222831','--p3': '#30475E','--p4': '#F05454',
            '--bg': '#0E1112','--surface': 'rgba(255,255,255,0.03)','--text': '#DDDDDD',
            '--muted': '#8f9aa1','--card-bg': 'rgba(255,255,255,0.02)','--shadow': 'rgba(0,0,0,0.5)'
          },
          logoCandidates: ['logo-graphite-dark.png','logo-graphite.png','logo-dark.png']
        }
      }
    },
    mist: {
      label: 'Mist',
      variants: {
        light: {
          vars: {
            '--p1': '#F7F7F7','--p2': '#EEEEEE','--p3': '#393E46','--p4': '#929AAB',
            '--bg': '#F7F7F7','--surface': '#EEEEEE','--text': '#393E46','--muted': '#6f7680',
            '--card-bg': '#ffffff','--shadow': 'rgba(16,20,22,0.06)'
          },
          logoCandidates: ['logo-mist-light.png','logo-mist.png','logo-light.png']
        },
        dark: {
          vars: {
            '--p1': '#F7F7F7','--p2': '#EEEEEE','--p3': '#393E46','--p4': '#929AAB',
            '--bg': '#0D0F12','--surface': 'rgba(255,255,255,0.03)','--text': '#F7F7F7',
            '--muted': '#9aa3ac','--card-bg': 'rgba(255,255,255,0.02)','--shadow': 'rgba(0,0,0,0.5)'
          },
          logoCandidates: ['logo-mist-dark.png','logo-mist.png','logo-dark.png']
        }
      }
    },
    amethyst: {
      label: 'Amethyst',
      variants: {
        light: {
          vars: {
            '--p1': '#20262E','--p2': '#913175','--p3': '#CD5888','--p4': '#E9E8E8',
            '--bg': '#E9E8E8','--surface': '#F4EAF0','--text': '#20262E','--muted': '#6b4b60',
            '--card-bg': '#ffffff','--shadow': 'rgba(16,20,22,0.06)'
          },
          logoCandidates: ['logo-amethyst-light.png','logo-amethyst.png','logo-light.png']
        },
        dark: {
          vars: {
            '--p1': '#20262E','--p2': '#913175','--p3': '#CD5888','--p4': '#E9E8E8',
            '--bg': '#120A0F','--surface': 'rgba(255,255,255,0.03)','--text': '#E9E8E8',
            '--muted': '#b5879d','--card-bg': 'rgba(255,255,255,0.02)','--shadow': 'rgba(0,0,0,0.5)'
          },
          logoCandidates: ['logo-amethyst-dark.png','logo-amethyst.png','logo-dark.png']
        }
      }
    },
    breeze: {
      label: 'Breeze',
      variants: {
        light: {
          vars: {
            '--p1': '#394867','--p2': '#212A3E','--p3': '#9BA4B5','--p4': '#F1F6F9',
            '--bg': '#F1F6F9','--surface': '#E6EEF4','--text': '#212A3E','--muted': '#62707b',
            '--card-bg': '#ffffff','--shadow': 'rgba(16,20,22,0.06)'
          },
          logoCandidates: ['logo-breeze-light.png','logo-breeze.png','logo-light.png']
        },
        dark: {
          vars: {
            '--p1': '#394867','--p2': '#212A3E','--p3': '#9BA4B5','--p4': '#F1F6F9',
            '--bg': '#0B1216','--surface': 'rgba(255,255,255,0.03)','--text': '#F1F6F9',
            '--muted': '#9aa6b0','--card-bg': 'rgba(255,255,255,0.02)','--shadow': 'rgba(0,0,0,0.5)'
          },
          logoCandidates: ['logo-breeze-dark.png','logo-breeze.png','logo-dark.png']
        }
      }
    }
  };

  const STORAGE_THEME = 'app_selected_theme';
  const STORAGE_VARIANT = 'app_selected_variant';

  function safeGet(id) { return document.getElementById(id) || null; }
  function normalizeCandidatePath(s) {
    if (!s) return s;
    if (s.startsWith('/') || s.startsWith('http')) return s;
    return '/images/' + s;
  }

  function tryLoad(list, onFound, onFail) {
    const candidates = (list || []).map(normalizeCandidatePath);
    let i = 0;
    function next() {
      if (i >= candidates.length) { onFail(); return; }
      const src = candidates[i++];
      const img = new Image();
      img.onload = () => onFound(src);
      img.onerror = next;
      img.src = src;
    }
    next();
  }

  function fallbackSVG(variant) {
    if (variant === 'light') {
      const svg = '<svg xmlns="http://www.w3.org/2000/svg" width="360" height="90"><rect width="360" height="90" rx="8" fill="#f5f8fa"/><text x="180" y="56" fill="#1E2022" font-size="26" font-family="Arial" font-weight="700" text-anchor="middle">MY APP</text></svg>';
      return 'data:image/svg+xml;utf8,' + encodeURIComponent(svg);
    } else {
      const svg = '<svg xmlns="http://www.w3.org/2000/svg" width="360" height="90"><rect width="360" height="90" rx="8" fill="#0d0d0f"/><text x="180" y="56" fill="#f6f9fb" font-size="26" font-family="Arial" font-weight="700" text-anchor="middle">MY APP</text></svg>';
      return 'data:image/svg+xml;utf8,' + encodeURIComponent(svg);
    }
  }

  function populateOptions(selectEl) {
    if (!selectEl) return;
    selectEl.innerHTML = '';
    for (const key of Object.keys(THEMES)) {
      const opt = document.createElement('option');
      opt.value = key;
      opt.textContent = THEMES[key].label || key;
      selectEl.appendChild(opt);
    }
  }

  function applyThemeToDom(themeKey, variant, elements, log) {
    const theme = THEMES[themeKey];
    if (!theme) { if (log) console.warn('theme-switcher: unknown theme', themeKey); return; }
    const config = theme.variants[variant];
    if (!config) { if (log) console.warn('theme-switcher: unknown variant', variant); return; }
    if (log) console.info('theme-switcher: apply', themeKey, variant);

    document.documentElement.classList.add('theme-transition');
    for (const [k, v] of Object.entries(config.vars)) {
      document.documentElement.style.setProperty(k, v);
    }

    if (elements.themeSelect) elements.themeSelect.value = themeKey;
    if (elements.variantLight) elements.variantLight.setAttribute('aria-pressed', variant === 'light' ? 'true' : 'false');
    if (elements.variantDark) elements.variantDark.setAttribute('aria-pressed', variant === 'dark' ? 'true' : 'false');
    if (elements.currentInfo) elements.currentInfo.textContent = `${theme.label || themeKey} — ${variant}`;

    tryLoad(config.logoCandidates || [], (src) => {
      if (elements.logoEl) {
        elements.logoEl.style.transform = 'scale(.96)';
        setTimeout(()=>{ elements.logoEl.src = src; elements.logoEl.style.transform = 'scale(1)'; }, 120);
      }
    }, () => {
      if (elements.logoEl) elements.logoEl.src = fallbackSVG(variant);
    });

    window.clearTimeout(window._themeRemoveTimeout);
    window._themeRemoveTimeout = setTimeout(()=> document.documentElement.classList.remove('theme-transition'), 420);

    // persist
    localStorage.setItem(STORAGE_THEME, themeKey);
    localStorage.setItem(STORAGE_VARIANT, variant);
  }

  function applyStoredTheme(elements, log) {
    const storedTheme = localStorage.getItem(STORAGE_THEME) || Object.keys(THEMES)[0];
    const storedVariant = localStorage.getItem(STORAGE_VARIANT) || 'light';
    if (!THEMES[storedTheme]) {
      const first = Object.keys(THEMES)[0];
      applyThemeToDom(first, storedVariant, elements, log);
    } else {
      applyThemeToDom(storedTheme, storedVariant, elements, log);
    }
  }

  function bindHandlers(elements, log) {
    if (elements.themeSelect) {
      populateOptions(elements.themeSelect);
      elements.themeSelect.onchange = function (e) {
        const themeKey = e.target.value;
        const variant = localStorage.getItem(STORAGE_VARIANT) || 'light';
        applyThemeToDom(themeKey, variant, elements, log);
      };
    }
    if (elements.variantLight) {
      elements.variantLight.onclick = function () {
        const themeKey = (elements.themeSelect && elements.themeSelect.value) || Object.keys(THEMES)[0];
        applyThemeToDom(themeKey, 'light', elements, log);
      };
    }
    if (elements.variantDark) {
      elements.variantDark.onclick = function () {
        const themeKey = (elements.themeSelect && elements.themeSelect.value) || Object.keys(THEMES)[0];
        applyThemeToDom(themeKey, 'dark', elements, log);
      };
    }
    if (elements.resetBtn) {
      elements.resetBtn.onclick = function () {
        localStorage.removeItem(STORAGE_THEME);
        localStorage.removeItem(STORAGE_VARIANT);
        const defaultTheme = Object.keys(THEMES)[0];
        applyThemeToDom(defaultTheme, 'light', elements, log);
      };
    }

    // keyboard toggle
    document.onkeydown = function (e) {
      if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
        const cur = localStorage.getItem(STORAGE_VARIANT) || 'light';
        const next = (cur === 'light') ? 'dark' : 'light';
        const themeKey = (safeGet('themeSelect') && safeGet('themeSelect').value) || Object.keys(THEMES)[0];
        applyThemeToDom(themeKey, next, getElements(), log);
      }
    };
  }

  function getElements() {
    return {
      themeSelect: safeGet('themeSelect'),
      variantLight: safeGet('variantLight'),
      variantDark: safeGet('variantDark'),
      resetBtn: safeGet('resetBtn'),
      logoEl: safeGet('logo'),
      themesList: safeGet('themesList'),
      currentInfo: safeGet('currentInfo')
    };
  }

  // Public API
  window.themeSwitcher = {
    init: function (opts) {
      const log = opts && opts.log === true;
      const elements = getElements();
      bindHandlers(elements, log);
      applyStoredTheme(elements, log);
      if (log) console.info('theme-switcher.init completed');
    },
    refresh: function (opts) {
      const log = opts && opts.log === true;
      const elements = getElements();
      bindHandlers(elements, log);
      if (elements.themesList) {
        elements.themesList.innerHTML = '';
        for (const key of Object.keys(THEMES)) {
          const div = document.createElement('div');
          div.textContent = `${THEMES[key].label} — variants: ${Object.keys(THEMES[key].variants).join(', ')}`;
          div.style.marginBottom = '6px';
          div.style.color = getComputedStyle(document.documentElement).getPropertyValue('--muted') || '#888';
          elements.themesList.appendChild(div);
        }
      }
      applyStoredTheme(elements, log);
      if (log) console.info('theme-switcher.refresh completed');
    },
    applyStoredTheme: function (opts) {
      const log = opts && opts.log === true;
      const elements = getElements();
      applyStoredTheme(elements, log);
      if (log) console.info('theme-switcher.applyStoredTheme completed');
    }
  };

  // Auto-init on DOMContentLoaded so first-render gets populated even if Blazor call misses timing
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () {
      try { window.themeSwitcher && window.themeSwitcher.init && window.themeSwitcher.init({ log: false }); } catch (e) { /* ignore */ }
    });
  } else {
    try { window.themeSwitcher && window.themeSwitcher.init && window.themeSwitcher.init({ log: false }); } catch (e) { /* ignore */ }
  }
})();