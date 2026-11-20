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

function bilgiKartlari(noktalar, parsel = false) {
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
        if (parsel) {
            const detay = document.createElement('a');
            detay.href = `/tbys/tasinmaz/${nokta.tasinmazId}?geri=%2Ftbys%2Ftasinmaz-kontrol%2Fharita`;
            detay.textContent = 'Taşınmaz Detayını Aç';
            kart.appendChild(detay);
            root.appendChild(kart);
            continue;
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
    const kontrolKatmani = L.featureGroup().addTo(map);
    const parselKatmani = L.featureGroup().addTo(map);
    const parselSinirlari = L.featureGroup().addTo(parselKatmani);
    const parselIsaretleri = L.layerGroup().addTo(parselKatmani);
    const parselGorunumleri = [];
    L.control.layers({}, {
        'Kontrol konumları': kontrolKatmani,
        'Parsel sınırları': parselKatmani
    }, { collapsed: false }).addTo(map);

    const sigdir = () => {
        const sinirlar = L.latLngBounds([]);
        const kontrolSiniri = kontrolKatmani.getBounds();
        const parselSiniri = parselSinirlari.getBounds();
        const parselGorunur = map.hasLayer(parselKatmani) && parselSiniri.isValid();
        if (map.hasLayer(kontrolKatmani) && kontrolSiniri.isValid()) sinirlar.extend(kontrolSiniri);
        if (parselGorunur) sinirlar.extend(parselSiniri);
        if (sinirlar.isValid()) map.fitBounds(sinirlar, { padding: [35, 35], maxZoom: parselGorunur ? 19 : 16, animate: false });
        else map.setView([39, 35], 6);
    };
    const parselIsaretleriniGuncelle = () => {
        for (const { sinir, isaret } of parselGorunumleri) {
            const kuzeyDogu = map.project(sinir.getNorthEast());
            const guneyBati = map.project(sinir.getSouthWest());
            const boyut = kuzeyDogu.subtract(guneyBati);
            if (Math.max(Math.abs(boyut.x), Math.abs(boyut.y)) < 12) {
                parselIsaretleri.addLayer(isaret);
            } else {
                parselIsaretleri.removeLayer(isaret);
            }
        }
    };
    map.on('zoomend', parselIsaretleriniGuncelle);

    const observer = new ResizeObserver(() => {
        map.invalidateSize();
        sigdir();
        kontrolKatmani.eachLayer(marker => { if (marker.isPopupOpen()) marker.getPopup().update(); });
        parselSinirlari.eachLayer(parsel => { if (parsel.isPopupOpen()) parsel.getPopup().update(); });
    });
    observer.observe(element);
    let kapali = false;
    return {
        guncelle(noktalar) {
            if (kapali) return;
            kontrolKatmani.clearLayers();
            parselSinirlari.clearLayers();
            parselIsaretleri.clearLayers();
            parselGorunumleri.length = 0;
            const konumlar = new Map();
            const parseller = new Map();
            for (const nokta of noktalar) {
                const geometri = nokta.parselGeometrisiGeoJson;
                if (geometri) {
                    if (!parseller.has(geometri)) parseller.set(geometri, []);
                    parseller.get(geometri).push(nokta);
                }
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
                    .bindPopup(bilgiKartlari(grup), { maxWidth: 320, autoPanPadding: [16, 16] }).addTo(kontrolKatmani);
            }
            for (const [geoJson, grup] of parseller) {
                try {
                    const geometri = JSON.parse(geoJson);
                    if (!['Polygon', 'MultiPolygon'].includes(geometri?.type)) continue;
                    const parsel = L.geoJSON(geometri, { style: { color: '#0f766e', weight: 2, fillOpacity: 0.22 } });
                    const sinir = parsel.getBounds();
                    if (!sinir.isValid()) continue;
                    parsel
                        .bindPopup(() => bilgiKartlari(grup, true), { maxWidth: 320, autoPanPadding: [16, 16] })
                        .addTo(parselSinirlari);

                    const merkez = sinir.getCenter();
                    const isaret = L.circleMarker(merkez, {
                        radius: 5, color: '#fff', weight: 1.5, fillColor: '#0f766e', fillOpacity: 1,
                        bubblingMouseEvents: false
                    }).on('click', () => {
                        map.fitBounds(sinir, { padding: [50, 50], maxZoom: 19, animate: false });
                        parsel.openPopup(merkez);
                    });
                    parselGorunumleri.push({ sinir, isaret });
                } catch (error) {
                    console.warn('Parsel sınırı çizilemedi.', grup.map(nokta => nokta.tasinmazId), error);
                }
            }
            sigdir();
            parselIsaretleriniGuncelle();
        },
        kapat() {
            if (kapali) return;
            kapali = true;
            observer.disconnect();
            map.remove();
        }
    };
}
