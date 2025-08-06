window.setBootstrapTheme = (theme) => {
    document.documentElement.setAttribute('data-bs-theme', theme);
    localStorage.setItem('theme', theme);
};

window.getBootstrapTheme = () => {
    return localStorage.getItem('theme') || 'light';
};
