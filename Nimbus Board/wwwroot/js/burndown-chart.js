(function () {
    function renderBurndown() {
        const canvas = document.getElementById('burndown-chart');
        if (!canvas || !window.nimbusBurndown || !window.Chart) {
            return;
        }

        const data = window.nimbusBurndown;
        if (!data.labels || data.labels.length === 0) {
            return;
        }

        const existing = window.Chart.getChart(canvas);
        if (existing) {
            existing.destroy();
        }

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
                        pointRadius: 3,
                        pointHoverRadius: 5
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
                        suggestedMax: Math.max(
                            ...(data.actual || [0]),
                            ...(data.ideal || [0]),
                            1
                        ) + 2,
                        grid: { color: '#f1f5f9' },
                        ticks: { color: '#94a3b8', font: { size: 11 }, precision: 0 }
                    }
                }
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', renderBurndown);
    } else {
        renderBurndown();
    }
})();
