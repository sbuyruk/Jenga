export function dismissSplash() {
    document.getElementById('jenga-startup-overlay')?.remove();
}

if (globalThis.Blazor?.addEventListener) {
    Blazor.addEventListener('enhancedload', dismissSplash);
}
