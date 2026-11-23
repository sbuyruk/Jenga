let leafletReady;

const parselTurleri = new Map([
    ['arsa', 'arsa'],
    ['isyeri', 'isyeri'], ['dukkan', 'isyeri'],
    ['mesken', 'mesken'], ['mev', 'mesken'],
    ['apartman', 'bina'], ['apt', 'bina'], ['bina', 'bina'], ['ishani', 'bina'],
    ['tarla', 'tarla']
]);

const parselSimgeleri = {
    arsa: { etiket: 'Arsa', ikon: 'bi bi-grid' },
    isyeri: { etiket: 'İşyeri', ikon: 'bi bi-shop' },
    mesken: { etiket: 'Mesken', ikon: 'bi bi-house-door-fill' },
    bina: { etiket: 'Bina', ikon: 'bi bi-buildings' },
    tarla: { etiket: 'Tarla', ikon: 'parsel-tarla-simge' },
    karma: { etiket: 'Farklı cinsler', ikon: 'bi bi-layers-half' },
    diger: { etiket: 'Diğer / belirsiz', ikon: 'bi bi-geo-alt-fill' }
};

function parselSimgeElemani(tur) {
    const simge = document.createElement('i');
    simge.className = parselSimgeleri[tur].ikon;
    simge.setAttribute('aria-hidden', 'true');
    return simge;
}

function parselSimgesi(L, grup) {
    const cinsler = [...new Set(grup.map(nokta => nokta.cinsi?.trim() || 'Cinsi belirtilmemiş'))];
    const turler = new Set(cinsler.map(cins => {
        const anahtar = cins.toLocaleLowerCase('tr').normalize('NFD')
            .replace(/\p{M}/gu, '').replaceAll('ı', 'i').replace(/[\s.]+/g, '');
        return parselTurleri.get(anahtar) ?? 'diger';
    }));
    const tur = turler.size === 1 ? turler.values().next().value : 'karma';
    return {
        title: cinsler.join(' / '),
        icon: L.divIcon({
            className: `tasinmaz-isaret parsel-isaret parsel-isaret-${tur}`,
            html: parselSimgeElemani(tur),
            iconSize: [28, 28],
            iconAnchor: [14, 32]
        })
    };
}

function parselLejanti(L, map, parselKatmani) {
    const kutu = document.createElement('details');
    kutu.className = 'harita-lejant';
    kutu.open = true;
    const baslik = document.createElement('summary');
    baslik.textContent = 'Parsel simgeleri';
    const liste = document.createElement('ul');
    liste.setAttribute('role', 'list');
    for (const [tur, { etiket }] of Object.entries(parselSimgeleri)) {
        const satir = document.createElement('li');
        const simge = parselSimgeElemani(tur);
        simge.classList.add('harita-lejant-simge', `parsel-isaret-${tur}`);
        satir.append(simge, etiket);
        liste.appendChild(satir);
    }
    kutu.append(baslik, liste);
    L.DomEvent.disableClickPropagation(kutu);
    L.DomEvent.disableScrollPropagation(kutu);

    const lejant = L.control({ position: 'bottomleft' });
    lejant.onAdd = () => kutu;
    lejant.addTo(map);
    parselKatmani.on('add remove', () => { kutu.hidden = !map.hasLayer(parselKatmani); });
}

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

function bilgiKartlari(noktalar, parselMerkezi) {
    const root = document.createElement('div');
    for (const nokta of noktalar) {
        const kart = document.createElement('section');
        kart.className = 'tasinmaz-bilgi';
        const baslik = document.createElement('strong');
        baslik.textContent = `Sicil No: ${nokta.emlakSicilNo || '—'}`;
        kart.appendChild(baslik);
        for (const [etiket, deger] of [
            ['Cinsi', nokta.cinsi], ['Kira Durumu', nokta.kiraDurumu?.trim()], ['Adres', nokta.adres],
            ['İl / İlçe', [nokta.ili, nokta.ilcesi].filter(Boolean).join(' / ')]
        ]) {
            const satir = document.createElement('p');
            const label = document.createElement('b');
            label.textContent = `${etiket}: `;
            satir.append(label, document.createTextNode(deger || '—'));
            kart.appendChild(satir);
        }
        const [enlem, boylam] = Number.isFinite(nokta.enlem) && Number.isFinite(nokta.boylam)
            ? [nokta.enlem, nokta.boylam]
            : [parselMerkezi.lat, parselMerkezi.lng];
        const googleKoordinati = encodeURIComponent(`${enlem},${boylam}`);
        if (parselMerkezi) {
            const detay = document.createElement('a');
            detay.href = `/tbys/tasinmaz/${nokta.tasinmazId}?geri=%2Ftbys%2Ftasinmaz-kontrol%2Fharita`;
            detay.textContent = 'Taşınmaz Detayını Aç';
            kart.appendChild(detay);
        } else {
            if (nokta.taslak) {
                const not = document.createElement('p');
                not.textContent = 'Koordinat kaynağı: açık taslak';
                kart.appendChild(not);
            }
            const rapor = document.createElement('a');
            rapor.href = `/tbys/tasinmaz-kontrol/${nokta.kontrolId}`;
            rapor.textContent = 'Kontrol Raporunu Aç';
            const google = document.createElement('a');
            google.href = `https://www.google.com/maps/search/?api=1&query=${googleKoordinati}`;
            google.target = '_blank';
            google.rel = 'noopener noreferrer';
            google.textContent = 'Google Haritalar';
            kart.append(rapor, google);
        }
        const yandexKoordinati = encodeURIComponent(`${boylam},${enlem}`);
        for (const [etiket, adres] of [
            ['Google Sokak Görünümü', `https://www.google.com/maps/@?api=1&map_action=pano&viewpoint=${googleKoordinati}`],
            ['Yandex Sokak Görünümü', `https://yandex.com.tr/maps/?l=stv%2Csta&ll=${yandexKoordinati}&z=18&panorama%5Bpoint%5D=${yandexKoordinati}&panorama%5Bfull%5D=true`]
        ]) {
            const baglanti = document.createElement('a');
            baglanti.href = adres;
            baglanti.target = '_blank';
            baglanti.rel = 'noopener noreferrer';
            baglanti.textContent = etiket;
            kart.appendChild(baglanti);
        }
        root.appendChild(kart);
    }
    return root;
}

export async function olustur(element) {
    const L = await leafletYukle();
    if (!element.isConnected) throw new Error('Harita kapatıldı.');
    const map = L.map(element, { zoomControl: false, zoomSnap: 0.1 });
    const turkiyeSiniri = L.latLngBounds([[35.8, 25.6], [42.2, 44.9]]);
    const turkiyeBosluk = L.point(24, 24);
    const turkiyeyiSigdir = () => map.fitBounds(turkiyeSiniri, { padding: turkiyeBosluk, animate: false });
    const hareketTercihi = window.matchMedia('(prefers-reduced-motion: no-preference)');
    let acilisBekliyor = hareketTercihi.matches;
    if (acilisBekliyor) {
        const hedefZoom = map.getBoundsZoom(turkiyeSiniri, false, turkiyeBosluk.multiplyBy(2));
        map.setView([43, 27], Math.max(1, Math.floor(hedefZoom) - 2));
    } else {
        turkiyeyiSigdir();
    }
    L.control.zoom({ position: 'bottomright' }).addTo(map);
    const tiles = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    let acilisZamanlayicisi;
    function acilisiIptalEt() {
        acilisBekliyor = false;
        clearTimeout(acilisZamanlayicisi);
        tiles.off('load', acilisiPlanla);
        map.off('movestart', acilisiIptalEt);
    }

    function turkiyeyeYaklas() {
        acilisiIptalEt();
        map.flyToBounds(turkiyeSiniri, { padding: turkiyeBosluk, duration: 1.75 });
    }

    function acilisiPlanla() {
        clearTimeout(acilisZamanlayicisi);
        acilisZamanlayicisi = setTimeout(turkiyeyeYaklas, 320);
    }

    if (acilisBekliyor) {
        map.once('movestart', acilisiIptalEt);
        if (tiles.isLoading()) {
            tiles.once('load', acilisiPlanla);
            acilisZamanlayicisi = setTimeout(turkiyeyeYaklas, 1200);
        } else {
            acilisiPlanla();
        }
    }
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
    parselLejanti(L, map, parselKatmani);

    let yaklasma;
    let geriDonus;
    map.on('popupopen', ({ popup }) => {
        if (geriDonus?.popup !== popup) geriDonus = null;
    });
    map.on('popupclose', ({ popup }) => {
        const donus = geriDonus;
        if (donus?.popup !== popup) return;

        // Katman kaldırma veya başka bir kart açma işlemi aynı olay döngüsünde tamamlanabilir.
        queueMicrotask(() => {
            if (geriDonus !== donus) return;
            geriDonus = null;
            if (hareketTercihi.matches && L.Browser.any3d) {
                map.flyTo(donus.merkez, donus.zoom, { duration: 1.1 });
            } else {
                map.setView(donus.merkez, donus.zoom, { animate: false });
            }
        });
    });
    const yaklasmayiIptalEt = () => {
        if (!yaklasma) return;
        map.off('moveend', yaklasma);
        map.off('movestart popupopen', yaklasmayiIptalEt);
        yaklasma = null;
    };
    const konumaYaklas = (kaynak, sinir, merkez, maxZoom = 19) => {
        acilisiIptalEt();
        yaklasmayiIptalEt();
        geriDonus = null;
        map.closePopup();

        const oncekiGorunum = { merkez: map.getCenter(), zoom: map.getZoom() };
        const kartiAc = () => {
            geriDonus = { popup: kaynak.getPopup(), ...oncekiGorunum };
            kaynak.openPopup(merkez);
        };
        const secenekler = { padding: [50, 50], maxZoom };
        if (!hareketTercihi.matches || !L.Browser.any3d) {
            map.fitBounds(sinir, { ...secenekler, animate: false });
            kartiAc();
            return;
        }

        map.flyToBounds(sinir, { ...secenekler, duration: 1.5 });
        const bitti = () => {
            yaklasmayiIptalEt();
            kartiAc();
        };
        yaklasma = bitti;
        map.once('moveend', bitti);
        map.on('movestart popupopen', yaklasmayiIptalEt);
    };

    const sigdir = (animasyonlu = false) => {
        const sinirlar = L.latLngBounds([]);
        const kontrolSiniri = kontrolKatmani.getBounds();
        const parselSiniri = parselSinirlari.getBounds();
        const parselGorunur = map.hasLayer(parselKatmani) && parselSiniri.isValid();
        if (map.hasLayer(kontrolKatmani) && kontrolSiniri.isValid()) sinirlar.extend(kontrolSiniri);
        if (parselGorunur) sinirlar.extend(parselSiniri);
        const sonucVar = sinirlar.isValid();
        const hedef = sonucVar ? sinirlar : turkiyeSiniri;
        const secenekler = sonucVar
            ? { padding: [35, 35], maxZoom: parselGorunur ? 19 : 16 }
            : { padding: turkiyeBosluk };
        if (animasyonlu && hareketTercihi.matches) {
            map.flyToBounds(hedef, { ...secenekler, duration: 1.25 });
        } else {
            map.fitBounds(hedef, { ...secenekler, animate: false });
        }
    };

    let sonuclariSigdir = false;
    let gorunumZamanlayicisi;
    const gorunumGuncellemesiniIptalEt = () => {
        clearTimeout(gorunumZamanlayicisi);
        map.off('movestart', gorunumGuncellemesiniIptalEt);
    };
    const gorunumuGuncelle = () => {
        acilisiIptalEt();
        sonuclariSigdir = true;
        gorunumGuncellemesiniIptalEt();
        if (!hareketTercihi.matches || !L.Browser.any3d) {
            sigdir();
            return;
        }
        gorunumZamanlayicisi = setTimeout(() => {
            gorunumGuncellemesiniIptalEt();
            sigdir(true);
        }, 180);
        map.once('movestart', gorunumGuncellemesiniIptalEt);
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

    let veriYuklendi = false;
    let kapali = false;
    const katmanGorunumuDegisti = () => {
        if (kapali) return;
        geriDonus = null;
        if (yaklasma) {
            yaklasmayiIptalEt();
            map.stop();
        }
        gorunumuGuncelle();
    };
    kontrolKatmani.on('add remove', katmanGorunumuDegisti);
    parselKatmani.on('add remove', katmanGorunumuDegisti);

    const observer = new ResizeObserver(() => {
        const oncekiBoyut = map.getSize();
        if (oncekiBoyut.x !== element.clientWidth || oncekiBoyut.y !== element.clientHeight) {
            yaklasmayiIptalEt();
        }
        map.invalidateSize();
        if (oncekiBoyut.equals(map.getSize())) return;
        if (sonuclariSigdir) sigdir();
        else if (!acilisBekliyor) turkiyeyiSigdir();
        kontrolKatmani.eachLayer(marker => { if (marker.isPopupOpen()) marker.getPopup().update(); });
        parselSinirlari.eachLayer(parsel => { if (parsel.isPopupOpen()) parsel.getPopup().update(); });
    });
    observer.observe(element);
    return {
        guncelle(noktalar) {
            if (kapali) return;
            yaklasmayiIptalEt();
            geriDonus = null;
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
                const merkez = L.latLng(nokta.enlem, nokta.boylam);
                const isaret = L.marker(merkez, { icon, title, alt: title, keyboard: true })
                    .bindPopup(bilgiKartlari(grup), { maxWidth: 320, autoPanPadding: [16, 16] }).addTo(kontrolKatmani);
                const sec = () => {
                    if (geriDonus?.popup === isaret.getPopup()) {
                        isaret.closePopup();
                        return;
                    }
                    const sinir = L.latLngBounds(merkez, merkez);
                    konumaYaklas(isaret, sinir, merkez, Math.max(16, map.getZoom()));
                };
                // Leaflet'in kartı hemen açan varsayılan işleyicilerini kamera geçişiyle değiştir.
                isaret.off('click keypress').on('click', sec).on('keypress', ({ originalEvent }) => {
                    if (originalEvent.key === 'Enter') sec();
                });
            }
            for (const [geoJson, grup] of parseller) {
                try {
                    const geometri = JSON.parse(geoJson);
                    if (!['Polygon', 'MultiPolygon'].includes(geometri?.type)) continue;
                    const parsel = L.geoJSON(geometri, { style: { color: '#0f766e', weight: 2, fillOpacity: 0.22 } });
                    const sinir = parsel.getBounds();
                    if (!sinir.isValid()) continue;
                    const merkez = sinir.getCenter();
                    parsel
                        .bindPopup(() => bilgiKartlari(grup, merkez), { maxWidth: 320, autoPanPadding: [16, 16] })
                        .addTo(parselSinirlari);

                    const isaret = L.marker(merkez, { ...parselSimgesi(L, grup), zIndexOffset: 1000 })
                        .on('click', () => konumaYaklas(parsel, sinir, merkez));
                    parselGorunumleri.push({ sinir, isaret });
                } catch (error) {
                    console.warn('Parsel sınırı çizilemedi.', grup.map(nokta => nokta.tasinmazId), error);
                }
            }
            if (veriYuklendi || sonuclariSigdir) gorunumuGuncelle();
            veriYuklendi = true;
            parselIsaretleriniGuncelle();
        },
        kapat() {
            if (kapali) return;
            kapali = true;
            acilisiIptalEt();
            gorunumGuncellemesiniIptalEt();
            yaklasmayiIptalEt();
            geriDonus = null;
            observer.disconnect();
            map.remove();
        }
    };
}
