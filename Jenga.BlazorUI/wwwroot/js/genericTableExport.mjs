// ES module version — export functions so Blazor can import safely

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