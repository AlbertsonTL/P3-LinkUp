(() => {
    const themeStorageKey = 'linkup-theme';
    const root = document.documentElement;

    const getPreferredTheme = () => {
        const savedTheme = localStorage.getItem(themeStorageKey);
        if (savedTheme === 'light' || savedTheme === 'dark') {
            return savedTheme;
        }

        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    };

    const updateThemeToggle = (theme) => {
        document.querySelectorAll('[data-theme-toggle]').forEach((toggle) => {
            const isDark = theme === 'dark';
            toggle.setAttribute('aria-label', isDark ? 'Activar modo claro' : 'Activar modo oscuro');
            toggle.setAttribute('title', isDark ? 'Activar modo claro' : 'Activar modo oscuro');
            toggle.innerHTML = `<i class="fas fa-${isDark ? 'sun' : 'moon'}" aria-hidden="true"></i>`;
        });
    };

    const applyTheme = (theme) => {
        root.dataset.theme = theme;
        updateThemeToggle(theme);
    };

    applyTheme(getPreferredTheme());

    document.addEventListener('click', (event) => {
        const toggle = event.target.closest('[data-theme-toggle]');
        if (!toggle) {
            return;
        }

        const nextTheme = root.dataset.theme === 'dark' ? 'light' : 'dark';
        localStorage.setItem(themeStorageKey, nextTheme);
        applyTheme(nextTheme);
    });
})();

function toggleReplyForm(commentId) {
    const form = document.getElementById('reply-form-' + commentId);
    if (form) {
        form.style.display = form.style.display === 'none' ? 'block' : 'none';
    }
}
