window.turkiyeMap = (function () {
    let _dotNetRef = null;
    let _paper = null;
    let _container = null;
    let _resizeHandler = null;

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
        _dotNetRef = dotNetRef;
        _container = document.getElementById(mapElementId);

        if (!_container || typeof Raphael === "undefined" || typeof paths === "undefined") {
            return;
        }

        _container.innerHTML = "";

        const width = Math.max(_container.clientWidth || 980, 980);
        const height = 560;
        _paper = Raphael(mapElementId, width, height);

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

            const obj = _paper.path(item.path);
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
                if (!Number.isNaN(provinceId) && _dotNetRef) {
                    _dotNetRef.invokeMethodAsync("OnProvinceClickedFromJs", provinceId);
                }
            });

            drawn.push(obj);
        }

        fitToContent(drawn);

        _resizeHandler = () => fitToContent(drawn);
        window.addEventListener("resize", _resizeHandler);
    }

    function fitToContent(drawn) {
        if (!_paper || !_container || !drawn || drawn.length === 0) {
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

        const targetWidth = Math.max(_container.clientWidth || 980, 700);
        const targetHeight = Math.round(targetWidth / aspect);

        _paper.setSize(targetWidth, targetHeight);
        _paper.setViewBox(minX, minY, vbWidth, vbHeight, true);
    }

    function dispose() {
        if (_resizeHandler) {
            window.removeEventListener("resize", _resizeHandler);
            _resizeHandler = null;
        }

        _dotNetRef = null;

        if (_paper) {
            _paper.remove();
            _paper = null;
        }

        _container = null;
    }

    return {
        init,
        dispose
    };
})();