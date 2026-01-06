// ES module version — export functions so Blazor can import safely
export function downloadFromBase64(filename, base64, contentType) {
  const link = document.createElement('a');
  link.href = "data:" + contentType + ";base64," + base64;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  link.remove();
}

export function downloadXlsx(filename, headers, rows) {
  try {
    if (typeof window.XLSX !== 'undefined' || typeof XLSX !== 'undefined') {
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
    }
    console.warn("SheetJS not found in page; falling back to CSV.");
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