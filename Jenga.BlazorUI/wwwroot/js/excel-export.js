window.exportTableToXlsx = (tableElementId, fileName) => {
    const table = document.getElementById(tableElementId);

    if (!table) {
        throw new Error(`Table not found: ${tableElementId}`);
    }

    if (!window.XLSX) {
        throw new Error("XLSX is not loaded.");
    }

    const wb = window.XLSX.utils.book_new();
    const ws = window.XLSX.utils.table_to_sheet(table, { raw: false, cellStyles: true });

    const borderThin = { style: "thin", color: { rgb: "000000" } };
    const borderThick = { style: "medium", color: { rgb: "000000" } };

    const tryParseTrNumber = (value) => {
        if (value === null || value === undefined) return null;
        if (typeof value === "number" && Number.isFinite(value)) return value;

        let s = String(value).trim();
        if (!s) return null;

        // Rakam yoksa sayı değildir
        if (!/\d/.test(s)) return null;

        // boşluk + NBSP kaldır
        s = s.replace(/\u00A0/g, "").replace(/\s/g, "");

        // Parantezli negatif: (1.234,56)
        let negative = false;
        if (s.startsWith("(") && s.endsWith(")")) {
            negative = true;
            s = s.slice(1, -1);
        }

        // Rakam/ayırıcı/işaret dışındaki her şeyi at (TL, harf vs.)
        s = s.replace(/[^\d.,-]/g, "");

        if (!s || !/\d/.test(s)) return null;

        // TR normalize: 1.234,56 -> 1234.56
        //  - binlik '.' kaldır
        //  - ondalık ',' -> '.'
        const normalized = s.replace(/\./g, "").replace(/,/g, ".");
        const n = Number(normalized);

        if (!Number.isFinite(n)) return null;

        return negative ? -n : n;
    };

    const hasFraction = (n) => Number.isFinite(n) && Math.abs(n % 1) > 1e-9;

    const range = window.XLSX.utils.decode_range(ws["!ref"] || "A1:A1");

    // 1) Ensure cells exist + normalize string -> number
    for (let R = range.s.r; R <= range.e.r; ++R) {
        for (let C = range.s.c; C <= range.e.c; ++C) {
            const addr = window.XLSX.utils.encode_cell({ r: R, c: C });

            let cell = ws[addr];
            if (!cell) {
                cell = ws[addr] = { t: "s", v: "" };
            }

            const isHeaderRow = R <= 2; // thead = 3 satır
            if (isHeaderRow) {
                continue;
            }

            if (cell.t === "s") {
                const parsed = tryParseTrNumber(cell.v);
                if (parsed !== null) {
                    cell.v = parsed;
                    cell.t = "n";
                }
            }
        }
    }

    // 2) Column scan: if any decimal exists in the column => format whole column as 0.00
    const colHasFraction = {};
    for (let C = range.s.c; C <= range.e.c; ++C) {
        colHasFraction[C] = false;
    }

    for (let R = range.s.r; R <= range.e.r; ++R) {
        for (let C = range.s.c; C <= range.e.c; ++C) {
            const addr = window.XLSX.utils.encode_cell({ r: R, c: C });
            const cell = ws[addr];
            if (!cell) continue;

            const isNumber = cell.t === "n" || typeof cell.v === "number";
            if (!isNumber) continue;

            const n = Number(cell.v);
            if (hasFraction(n)) {
                colHasFraction[C] = true;
            }
        }
    }

    // 3) Styles + borders
    for (let R = range.s.r; R <= range.e.r; ++R) {
        for (let C = range.s.c; C <= range.e.c; ++C) {
            const addr = window.XLSX.utils.encode_cell({ r: R, c: C });
            const cell = ws[addr];
            if (!cell) continue;

            const isNumber = cell.t === "n" || typeof cell.v === "number";
            const isHeaderRow = R <= 2;

            const domRow = table.rows?.[R];
            const isTotalRow = !!domRow?.classList?.contains("fw-bold") || !!domRow?.classList?.contains("table-secondary");

            const isOuterTop = R === range.s.r;
            const isOuterBottom = R === range.e.r;
            const isOuterLeft = C === range.s.c;
            const isOuterRight = C === range.e.c;

            const makeRowThick = isHeaderRow || isTotalRow;

            cell.s = cell.s || {};
            cell.s.border = {
                top: (makeRowThick || isOuterTop) ? borderThick : borderThin,
                bottom: (makeRowThick || isOuterBottom) ? borderThick : borderThin,
                left: (makeRowThick || isOuterLeft) ? borderThick : borderThin,
                right: (makeRowThick || isOuterRight) ? borderThick : borderThin
            };

            cell.s.alignment = cell.s.alignment || {};
            cell.s.alignment.vertical = "center";
            cell.s.alignment.wrapText = true;
            cell.s.alignment.horizontal = isNumber ? "right" : "center";

            if (isNumber) {
                cell.z = colHasFraction[C] ? "#,##0.00" : "0";
            }

            if (isHeaderRow || isTotalRow) {
                cell.s.font = cell.s.font || {};
                cell.s.font.bold = true;
            }
        }
    }

    // Auto-fit column widths (approx)
    const colWidths = [];
    for (let C = range.s.c; C <= range.e.c; ++C) {
        let maxLen = 0;

        for (let R = range.s.r; R <= range.e.r; ++R) {
            const addr = window.XLSX.utils.encode_cell({ r: R, c: C });
            const cell = ws[addr];
            if (!cell || cell.v === null || cell.v === undefined) continue;

            const text = String(cell.v);
            if (text.length > maxLen) {
                maxLen = text.length;
            }
        }

        const wch = Math.min(Math.max(maxLen + 2, 6), 40);
        colWidths.push({ wch });
    }

    ws["!cols"] = colWidths;

    window.XLSX.utils.book_append_sheet(wb, ws, "Rapor");

    const safeName = (fileName && fileName.length > 0) ? fileName : "rapor.xlsx";
    window.XLSX.writeFile(wb, safeName, { cellStyles: true });
};