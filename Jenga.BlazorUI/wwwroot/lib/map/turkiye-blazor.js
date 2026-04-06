window.turkiyeMap = (function () {
    const _maps = new Map();

    function getRegionColor(region) {
        switch (region) {
            case "1": return "tomato";
            case "2": return "mediumseagreen";
            case "3": return "orange";
            case "4": return "dodgerblue";
            case "6": return "violet";
            default: return "#666";
        }
    }

    function init(mapElementId, dotNetRef) {
        dispose(mapElementId);

        const container = document.getElementById(mapElementId);
        if (!container || typeof Raphael === "undefined" || typeof paths === "undefined") {
            return;
        }

        container.innerHTML = "";

        const width = Math.max(container.clientWidth || 980, 980);
        const height = 560;
        const paper = Raphael(mapElementId, width, height);

        const attributes = {
            fill: "#666",
            stroke: "#fff",
            "stroke-width": 0.5,
            "stroke-linejoin": "round"
        };

        const drawn = [];

        for (const countyKey in paths) {
            if (!Object.prototype.hasOwnProperty.call(paths, countyKey)) {
                continue;
            }

            const item = paths[countyKey];
            if (!item || !item.path) {
                continue;
            }

            const obj = paper.path(item.path);
            obj.attr(attributes);

            if (countyKey === "blank") {
                continue;
            }

            obj.node.id = countyKey;
            obj.attr({
                title: item.name,
                fill: getRegionColor(item.bolge)
            });

            obj.click(function () {
                const provinceId = parseInt(item.county, 10);
                if (!Number.isNaN(provinceId) && dotNetRef) {
                    dotNetRef.invokeMethodAsync("OnProvinceClickedFromJs", provinceId);
                }
            });

            drawn.push(obj);
        }

        const fit = () => fitToContent(paper, container, drawn);
        fit();

        const resizeHandler = () => fit();
        window.addEventListener("resize", resizeHandler);

        _maps.set(mapElementId, {
            dotNetRef,
            paper,
            container,
            drawn,
            resizeHandler
        });
    }

    function fitToContent(paper, container, drawn) {
        if (!paper || !container || !drawn || drawn.length === 0) {
            return;
        }

        let minX = Number.POSITIVE_INFINITY;
        let minY = Number.POSITIVE_INFINITY;
        let maxX = Number.NEGATIVE_INFINITY;
        let maxY = Number.NEGATIVE_INFINITY;

        for (const p of drawn) {
            const b = p.getBBox();
            if (!b) continue;
            minX = Math.min(minX, b.x);
            minY = Math.min(minY, b.y);
            maxX = Math.max(maxX, b.x + b.width);
            maxY = Math.max(maxY, b.y + b.height);
        }

        if (!Number.isFinite(minX) || !Number.isFinite(minY) || !Number.isFinite(maxX) || !Number.isFinite(maxY)) {
            return;
        }

        const vbWidth = maxX - minX;
        const vbHeight = maxY - minY;
        const aspect = vbWidth / vbHeight;

        const targetWidth = Math.max(container.clientWidth || 980, 700);
        const targetHeight = Math.round(targetWidth / aspect);

        paper.setSize(targetWidth, targetHeight);
        paper.setViewBox(minX, minY, vbWidth, vbHeight, true);
    }

    function dispose(mapElementId) {
        const mapState = _maps.get(mapElementId);
        if (!mapState) {
            return;
        }

        if (mapState.resizeHandler) {
            window.removeEventListener("resize", mapState.resizeHandler);
        }

        if (mapState.paper) {
            mapState.paper.remove();
        }

        _maps.delete(mapElementId);
    }

    return {
        init,
        dispose
    };
})();