(function () {
    const memberId = document.body?.dataset?.memberId || '1';
    const badge = document.getElementById('notification-badge');

    function updateBadge(delta) {
        if (!badge) return;
        const current = parseInt(badge.dataset.count || '0', 10) || 0;
        const next = Math.max(0, current + delta);
        badge.dataset.count = String(next);
        if (next > 0) {
            badge.textContent = String(next);
            badge.classList.remove('hidden');
        } else {
            badge.textContent = '';
            badge.classList.add('hidden');
        }
    }

    function showToast(message, linkUrl) {
        const root = document.getElementById('toast-root');
        if (!root) return;
        const el = document.createElement('div');
        el.className = 'pointer-events-auto rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm shadow-lg';
        el.innerHTML = linkUrl
            ? `<a href="${linkUrl}" class="font-medium text-slate-900 hover:text-indigo-600">${message}</a>`
            : `<span class="font-medium text-slate-900">${message}</span>`;
        root.appendChild(el);
        setTimeout(() => el.remove(), 5000);
    }

    if (!window.signalR) {
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`/app/hubs/notifications?memberId=${encodeURIComponent(memberId)}`)
        .withAutomaticReconnect()
        .build();

    connection.on('notificationReceived', function (payload) {
        updateBadge(1);
        if (payload?.message) {
            showToast(payload.message, payload.linkUrl);
        }
    });

    connection.start().catch(function (err) {
        console.warn('SignalR connection failed', err);
    });
})();
