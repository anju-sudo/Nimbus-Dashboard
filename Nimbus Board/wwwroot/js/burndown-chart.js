(function () {
    const canvas = document.getElementById('burndown-chart');
    if (!canvas || !window.nimbusBurndown || !window.Chart) {
        return;
    }

    const data = window.nimbusBurndown;
    new Chart(canvas, {
        type: 'line',
        data: {
            labels: data.labels,
            datasets: [
                {
                    label: 'Actual',
                    data: data.actual,
                    borderColor: '#6366f1',
                    backgroundColor: 'rgba(99, 102, 241, 0.08)',
                    tension: 0.35,
                    fill: true,
                    pointRadius: 3
                },
                {
                    label: 'Ideal',
                    data: data.ideal,
                    borderColor: '#94a3b8',
                    borderDash: [6, 6],
                    tension: 0,
                    fill: false,
                    pointRadius: 0
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { color: '#94a3b8', font: { size: 11 } }
                },
                y: {
                    beginAtZero: true,
                    grid: { color: '#f1f5f9' },
                    ticks: { color: '#94a3b8', font: { size: 11 } }
                }
            }
        }
    });
})();
