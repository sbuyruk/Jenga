// ES module version � export functions so Blazor can import safely

export async function copyTextToClipboard(text) {
    try {
        if (navigator.clipboard && typeof navigator.clipboard.writeText === 'function') {
            await navigator.clipboard.writeText(text ?? '');
            return;
        }
    } catch {
        // ignore and fallback below
    }

    // Fallback (requires permission in some browsers)
    const ta = document.createElement('textarea');
    ta.value = text ?? '';
    ta.setAttribute('readonly', '');
    ta.style.position = 'fixed';
    ta.style.left = '-9999px';
    ta.style.top = '0';
    document.body.appendChild(ta);
    ta.select();
    document.execCommand('copy');
    ta.remove();
}

export async function copyTableToClipboard(tableId) {
    const table = document.getElementById(tableId);
    if (!table) throw new Error(`Table not found: ${tableId}`);

    // Clone to avoid copying event handlers or weird state; keep structure (rowspan/colspan)
    const clone = table.cloneNode(true);

    // Optional: remove interactive-only elements you might add later (sort icons, buttons, etc.)
    // (Şimdilik dokunmuyoruz; birebir kopya istendiği için yapıyı koruyoruz.)

    // Wrap in minimal HTML so Excel/Word parses as a table
    const html = `<!doctype html><html><head><meta charset="utf-8"></head><body>${clone.outerHTML}</body></html>`;

    // Also provide a plain-text fallback (TSV) derived from DOM (keeps column count more accurate than reflection)
    const text = tableToTsv(table);

    // Preferred: write both html and plain
    try {
        if (navigator.clipboard && typeof window.ClipboardItem === 'function') {
            const item = new ClipboardItem({
                'text/html': new Blob([html], { type: 'text/html' }),
                'text/plain': new Blob([text], { type: 'text/plain' })
            });
            await navigator.clipboard.write([item]);
            return;
        }
    } catch {
        // ignore and fallback below
    }

    // Fallback: plain text only
    await copyTextToClipboard(text);
}

function tableToTsv(table) {
    const rows = Array.from(table.querySelectorAll('tr'));
    const lines = [];

    for (const tr of rows) {
        const cells = Array.from(tr.querySelectorAll('th,td'))
            // only visible cells (optional; comment out if you want hidden too)
            .filter(cell => !!(cell.offsetParent || cell.getClientRects().length));

        const parts = cells.map(cell => cellText(cell));
        lines.push(parts.join('\t'));
    }

    return lines.join('\n');
}

function cellText(cell) {
    let s = (cell.innerText ?? cell.textContent ?? '').trim();
    s = s.replace(/\r\n/g, '\n').replace(/\r/g, '\n');
    s = s.replace(/\t/g, ' ');
    return s;
}

export function downloadFromBase64(filename, base64, contentType) {
    const link = document.createElement('a');
    link.href = "data:" + contentType + ";base64," + base64;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    link.remove();
}

async function ensureSheetJs(timeoutMs = 3000) {
    if (typeof window.XLSX !== 'undefined' || typeof XLSX !== 'undefined') return true;

    // Try to load local SheetJS automatically
    return await new Promise((resolve) => {
        try {
            const existing = document.querySelector('script[data-sheetjs="1"]');
            if (existing) {
                // Wait for existing script to load (or timeout)
                if (existing.getAttribute('data-loaded') === '1') return resolve(true);
                existing.addEventListener('load', () => resolve(true));
                existing.addEventListener('error', () => resolve(false));
                setTimeout(() => resolve(!!(window.XLSX || typeof XLSX !== 'undefined')), timeoutMs);
                return;
            }

            const script = document.createElement('script');
            script.src = '/lib/sheetjs/xlsx.full.min.js';
            script.async = true;
            script.setAttribute('data-sheetjs', '1');

            const onLoad = () => {
                script.setAttribute('data-loaded', '1');
                resolve(true);
            };
            const onError = () => resolve(false);

            script.addEventListener('load', onLoad);
            script.addEventListener('error', onError);

            document.head.appendChild(script);

            // Timeout fallback
            setTimeout(() => resolve(!!(window.XLSX || typeof XLSX !== 'undefined')), timeoutMs);
        } catch (err) {
            console.error('ensureSheetJs error', err);
            resolve(false);
        }
    });
}

export async function downloadXlsx(filename, headers, rows) {
    try {
        const sheetReady = await ensureSheetJs();
        if (sheetReady && (typeof window.XLSX !== 'undefined' || typeof XLSX !== 'undefined')) {
            const XLS = (typeof window.XLSX !== 'undefined') ? window.XLSX : XLSX;
            const aoa = [];
            aoa.push(headers || []);
            if (rows && rows.length) {
                for (let i = 0; i < rows.length; i++) {
                    aoa.push(rows[i]);
                }
            }
            const ws = XLS.utils.aoa_to_sheet(aoa);
            const wb = XLS.utils.book_new();
            XLS.utils.book_append_sheet(wb, ws, "Sheet1");
            const wbout = XLS.write(wb, { bookType: 'xlsx', type: 'base64' });
            const dataUrl = "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64," + wbout;
            const link = document.createElement('a');
            link.href = dataUrl;
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            link.remove();
            return;
        } else {
            console.warn("SheetJS not available; performing CSV fallback.");
        }
    } catch (err) {
        console.error("downloadXlsx error:", err);
    }

    // Fallback: build CSV and download
    try {
        const csvLines = [];
        if (headers && headers.length) {
            csvLines.push(headers.map(h => {
                if (h == null) return '';
                const s = String(h);
                if (s.includes(',') || s.includes('"') || s.includes('\n')) return `"${s.replace(/"/g, '""')}"`;
                return s;
            }).join(','));
        }
        if (rows && rows.length) {
            for (let r = 0; r < rows.length; r++) {
                const row = rows[r].map(c => {
                    if (c == null) return '';
                    const s = String(c);
                    if (s.includes(',') || s.includes('"') || s.includes('\n')) return `"${s.replace(/"/g, '""')}"`;
                    return s;
                }).join(',');
                csvLines.push(row);
            }
        }
        const csv = csvLines.join('\r\n');
        const b64 = btoa(unescape(encodeURIComponent(csv)));
        const csvName = filename && filename.toLowerCase().endsWith('.xlsx') ? filename.replace(/\.xlsx$/i, '.csv') : (filename + '.csv');
        downloadFromBase64(csvName, b64, "text/csv;charset=utf-8;");
    } catch (ex) {
        console.error("CSV fallback failed:", ex);
        throw ex;
    }
}

export async function exportTableToExcel(tableId, filename) {
    const table = document.getElementById(tableId);
    if (!table) throw new Error(`Table not found: ${tableId}`);

    const safeName = (filename && String(filename).trim().length > 0)
        ? String(filename).trim()
        : `export-${new Date().toISOString().replace(/[-:]/g, '').replace('T', '').slice(0, 14)}.xlsx`;

    const headers = readHeaderTexts(table);
    const rows = readBodyRows(table);

    await downloadXlsx(safeName, headers, rows);
}

function readHeaderTexts(table) {
    const headerRow = table.querySelector('thead tr');
    if (!headerRow) return [];

    const headerCells = Array.from(headerRow.querySelectorAll('th,td'))
        .filter(cell => !!(cell.offsetParent || cell.getClientRects().length));

    return headerCells.map(cell => cellText(cell));
}

function readBodyRows(table) {
    const bodyRows = Array.from(table.querySelectorAll('tbody tr'));
    const rows = [];

    for (const tr of bodyRows) {
        const cells = Array.from(tr.querySelectorAll('td,th'))
            .filter(cell => !!(cell.offsetParent || cell.getClientRects().length));

        rows.push(cells.map(cell => cellText(cell)));
    }

    return rows;
}