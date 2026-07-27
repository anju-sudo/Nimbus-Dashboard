(function () {
    if (typeof Sortable === 'undefined') {
        return;
    }

    const columns = document.querySelectorAll('.kanban-column');
    columns.forEach(function (column) {
        new Sortable(column, {
            group: 'kanban',
            animation: 150,
            ghostClass: 'opacity-50',
            draggable: '.issue-card',
            onEnd: function (evt) {
                const issueId = evt.item.dataset.issueId;
                const columnId = evt.to.dataset.columnId;
                const sortOrder = evt.newIndex;

                if (!issueId || !columnId) {
                    return;
                }

                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                const headers = { 'Content-Type': 'application/x-www-form-urlencoded' };
                if (token) {
                    headers['RequestVerificationToken'] = token;
                }

                fetch('/app/issues/move', {
                    method: 'POST',
                    headers: headers,
                    body: new URLSearchParams({
                        issueId: issueId,
                        boardColumnId: columnId,
                        sortOrder: sortOrder.toString()
                    })
                }).catch(function (err) {
                    console.error('Failed to move issue', err);
                });
            }
        });
    });
})();
