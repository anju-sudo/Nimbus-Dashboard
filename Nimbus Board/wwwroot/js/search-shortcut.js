document.addEventListener('keydown', function (event) {
    if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
        event.preventDefault();
        const modal = document.getElementById('search-modal');
        if (modal) {
            modal.classList.remove('hidden');
        }
    }

    if (event.key === 'Escape') {
        const modal = document.getElementById('search-modal');
        if (modal) {
            modal.classList.add('hidden');
        }
    }
});
