window.genericTableExport = window.genericTableExport || (function () {
  console.info("genericTableExport loaded");

  function downloadFromBase64(filename, base64, contentType) {
    try {
      const link = document.createElement('a');
      link.href = "data:" + contentType + ";base64," + base64;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err) {
      console.error("downloadFromBase64 error:", err);
      throw err;
    }
  }

  // headers: string[], rows: string[][]
  function downloadXlsx(filename, headers, rows) {
    try {
      if (typeof XLSX !== 'undefined') {
        var aoa = [];
        aoa.push(headers || []);
        if (rows && rows.length) {
          for (var i = 0; i < rows.length; i++) {
            aoa.push(rows[i]);
          }
        }

        var ws = XLSX.utils.aoa_to_sheet(aoa);
        var wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "Sheet1");
        var wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'base64' });

        var dataUrl = "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64," + wbout;
        var link = document.createElement('a');
        link.href = dataUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        link.remove();
        return;
      } else {
        console.warn("SheetJS (XLSX) not found. Falling back to CSV.");
      }
    } catch (err) {
      console.error("downloadXlsx failed:", err);
    }

    // Fallback to CSV
    try {
      var csvLines = [];
      if (headers && headers.length) {
        csvLines.push(headers.map(function (h) {
          if (h == null) return '';
          var s = String(h);
          if (s.indexOf(',') >= 0 || s.indexOf('"') >= 0 || s.indexOf('\n') >= 0) {
            return '"' + s.replace(/"/g, '""') + '"';
          }
          return s;
        }).join(','));
      }
      if (rows && rows.length) {
        for (var r = 0; r < rows.length; r++) {
          var row = rows[r].map(function (c) {
            if (c == null) return '';
            var s = String(c);
            if (s.indexOf(',') >= 0 || s.indexOf('"') >= 0 || s.indexOf('\n') >= 0) {
              return '"' + s.replace(/"/g, '""') + '"';
            }
            return s;
          }).join(',');
          csvLines.push(row);
        }
      }
      var csv = csvLines.join('\r\n');
      var b64 = btoa(unescape(encodeURIComponent(csv)));
      var csvName = filename && filename.toLowerCase().endsWith('.xlsx') ? filename.replace(/\.xlsx$/i, '.csv') : (filename + '.csv');
      downloadFromBase64(csvName, b64, "text/csv;charset=utf-8;");
    } catch (ex) {
      console.error("CSV fallback failed:", ex);
      throw ex;
    }
  }

  return {
    downloadFromBase64: downloadFromBase64,
    downloadXlsx: downloadXlsx
  };
})();