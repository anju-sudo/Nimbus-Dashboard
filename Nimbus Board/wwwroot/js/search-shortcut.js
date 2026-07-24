window.nimbusOpenSidebar = function () {
    const sidebar = document.getElementById('app-sidebar');
    const overlay = document.getElementById('sidebar-overlay');
    if (sidebar) sidebar.classList.remove('-translate-x-full');
    if (overlay) overlay.classList.remove('hidden');
};

window.nimbusCloseSidebar = function () {
    const sidebar = document.getElementById('app-sidebar');
    const overlay = document.getElementById('sidebar-overlay');
    if (sidebar) sidebar.classList.add('-translate-x-full');
    if (overlay) overlay.classList.add('hidden');
};

window.nimbusOpenSearch = function () {
    const modal = document.getElementById('search-modal');
    if (!modal) return;
    modal.classList.remove('hidden');
    modal.classList.add('flex');
    const input = document.getElementById('global-search-input');
    if (input) {
        input.value = '';
        input.focus();
    }
};

window.nimbusCloseSearch = function () {
    const modal = document.getElementById('search-modal');
    if (!modal) return;
    modal.classList.add('hidden');
    modal.classList.remove('flex');
};

document.addEventListener('keydown', function (event) {
    if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
        event.preventDefault();
        window.nimbusOpenSearch();
    }

    if (event.key === 'Escape') {
        window.nimbusCloseSearch();
        window.nimbusCloseSidebar();
    }
});
