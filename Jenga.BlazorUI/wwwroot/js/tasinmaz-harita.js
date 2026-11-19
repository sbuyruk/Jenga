let leafletReady;

function leafletYukle() {
    if (window.L) return Promise.resolve(window.L);
    if (leafletReady) return leafletReady;
    leafletReady = new Promise((resolve, reject) => {
        const css = document.createElement('link');
        css.rel = 'stylesheet';
        css.href = new URL('../lib/leaflet/leaflet.css', import.meta.url).href;
        css.onload = () => {
            const script = document.createElement('script');
            script.src = new URL('../lib/leaflet/leaflet.js', import.meta.url).href;
            script.onload = () => resolve(window.L);
            script.onerror = () => { script.remove(); css.remove(); reject(new Error('Harita yüklenemedi.')); };
            document.head.appendChild(script);
        };
        css.onerror = () => { css.remove(); reject(new Error('Harita stilleri yüklenemedi.')); };
        document.head.appendChild(css);
    }).catch(error => { leafletReady = null; throw error; });
    return leafletReady;
}

function bilgiKartlari(noktalar) {
    const root = document.createElement('div');
    for (const nokta of noktalar) {
        const kart = document.createElement('section');
        kart.className = 'tasinmaz-bilgi';
        const baslik = document.createElement('strong');
        baslik.textContent = `Sicil No: ${nokta.emlakSicilNo || '—'}`;
        kart.appendChild(baslik);
        for (const [etiket, deger] of [
            ['Cinsi', nokta.cinsi], ['Adres', nokta.adres],
            ['İl / İlçe', [nokta.ili, nokta.ilcesi].filter(Boolean).join(' / ')]
        ]) {
            const satir = document.createElement('p');
            const label = document.createElement('b');
            label.textContent = `${etiket}: `;
            satir.append(label, document.createTextNode(deger || '—'));
            kart.appendChild(satir);
        }
        if (nokta.taslak) {
            const not = document.createElement('p');
            not.textContent = 'Koordinat kaynağı: açık taslak';
            kart.appendChild(not);
        }
        const rapor = document.createElement('a');
        rapor.href = `/tbys/tasinmaz-kontrol/${nokta.kontrolId}`;
        rapor.textContent = 'Kontrol Raporunu Aç';
        const google = document.createElement('a');
        google.href = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(`${nokta.enlem},${nokta.boylam}`)}`;
        google.target = '_blank';
        google.rel = 'noopener noreferrer';
        google.textContent = 'Google Haritalar';
        kart.append(rapor, google);
        root.appendChild(kart);
    }
    return root;
}

export async function olustur(element) {
    const L = await leafletYukle();
    if (!element.isConnected) throw new Error('Harita kapatıldı.');
    const map = L.map(element, { zoomControl: false }).setView([39, 35], 6);
    L.control.zoom({ position: 'bottomright' }).addTo(map);
    const tiles = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);
    const uyari = L.control({ position: 'bottomleft' });
    uyari.onAdd = () => {
        const div = document.createElement('div');
        div.className = 'harita-zemin-hatasi';
        div.setAttribute('role', 'status');
        div.textContent = 'Harita zemini yüklenemedi. İnternet bağlantınızı kontrol edin.';
        return div;
    };
    let uyariAcik = false;
    tiles.on('tileerror', () => { if (!uyariAcik) { uyari.addTo(map); uyariAcik = true; } });
    tiles.on('tileload', () => { if (uyariAcik) { uyari.remove(); uyariAcik = false; } });
    const layer = L.featureGroup().addTo(map);
    const sigdir = () => {
        if (layer.getLayers().length) map.fitBounds(layer.getBounds(), { padding: [35, 35], maxZoom: 16, animate: false });
        else map.setView([39, 35], 6);
    };
    const observer = new ResizeObserver(() => {
        map.invalidateSize();
        sigdir();
        layer.eachLayer(marker => { if (marker.isPopupOpen()) marker.getPopup().update(); });
    });
    observer.observe(element);
    let kapali = false;
    return {
        guncelle(noktalar) {
            if (kapali) return;
            layer.clearLayers();
            const konumlar = new Map();
            for (const nokta of noktalar) {
                if (!Number.isFinite(nokta.enlem) || !Number.isFinite(nokta.boylam)) continue;
                const key = `${nokta.enlem},${nokta.boylam}`;
                if (!konumlar.has(key)) konumlar.set(key, []);
                konumlar.get(key).push(nokta);
            }
            for (const grup of konumlar.values()) {
                const nokta = grup[0];
                const icon = L.divIcon({ className: 'tasinmaz-isaret', html: grup.length > 1 ? String(grup.length) : '<i class="bi bi-building" aria-hidden="true"></i>', iconSize: [34, 34], iconAnchor: [17, 17] });
                const title = grup.length > 1 ? `${grup.length} taşınmaz` : `Sicil No: ${nokta.emlakSicilNo || '—'}`;
                L.marker([nokta.enlem, nokta.boylam], { icon, title, alt: title, keyboard: true })
                    .bindPopup(bilgiKartlari(grup), { maxWidth: 320, autoPanPadding: [16, 16] }).addTo(layer);
            }
            sigdir();
        },
        kapat() {
            if (kapali) return;
            kapali = true;
            observer.disconnect();
            map.remove();
        }
    };
}
