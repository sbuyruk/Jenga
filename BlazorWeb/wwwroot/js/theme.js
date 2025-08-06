function setThemeMode(theme) {
    //["body", ".page-layout", ".sidebar-area", ".main-area", ".toprow", ".content"].forEach(selector => {
    //    const el = selector === "body" ? document.body : document.querySelector(selector);
    //    if (el) {
    //        el.classList.remove("light", "dark");
    //        el.classList.add(theme); // "light" veya "dark"
    //    }
    //});
    document.documentElement.classList.remove("light", "dark");
    document.documentElement.classList.add(theme); // "dark" veya "light"

    // Opsiyonel: tercih kaydetmek istersen
    localStorage.setItem("theme", theme);
}
