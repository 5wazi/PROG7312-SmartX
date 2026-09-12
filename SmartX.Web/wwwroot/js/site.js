// Smart-X site script. Two small AJAX loops keep the Sensors table and the
// Live Dashboard feeling real-time without turning the app into a SPA — every
// other page (registration, deployment validation, aggregation) is a classic
// server-rendered form POST + redisplay.
window.SmartX = (function () {

    // =====================================
    // SENSORS TABLE (Views/Sensors/Index.cshtml)
    // =====================================

    function pollSensorTable() {
        refreshSensorTable();
        setInterval(refreshSensorTable, 3000);
    }

    async function refreshSensorTable() {
        const table = document.getElementById("sensorTable");
        if (!table) return;

        let sensors;
        try {
            const response = await fetch("/Sensors/List");
            sensors = await response.json();
        } catch {
            return; // API might be briefly unreachable — leave the last render in place
        }

        const countEl = document.getElementById("sensorCount");
        if (countEl) countEl.textContent = sensors.length;

        sensors.forEach(sensor => {
            const row = table.querySelector(`tr[data-sensor-id="${sensor.id}"]`);
            if (!row) return; // new sensors appear after the next full page load

            const statusCell = row.querySelector(".status-cell");
            const latestCell = row.querySelector(".latest-cell");
            const streakCell = row.querySelector(".streak-cell");

            if (statusCell) {
                statusCell.innerHTML = `<span class="badge ${sensor.isOnline ? "online" : "offline"}">${sensor.isOnline ? "Online" : "Offline"}</span>`;
            }
            if (latestCell) {
                latestCell.textContent = sensor.recentValues.length > 0
                    ? `${round(sensor.recentValues[sensor.recentValues.length - 1])}${sensor.unit}`
                    : "—";
            }
            if (streakCell) {
                streakCell.innerHTML = `🔥 ${sensor.currentStreak} <span style="color:var(--muted); font-weight:400;">(best ${sensor.bestStreak})</span>`;
            }
        });
    }

    // =====================================
    // LIVE DASHBOARD (Views/Dashboard/Index.cshtml)
    // =====================================

    function pollDashboard() {
        refreshDashboard();
        setInterval(refreshDashboard, 2000);
    }

    async function refreshDashboard() {
        const cardsContainer = document.getElementById("sensorCards");
        if (!cardsContainer) return;

        let data;
        try {
            const response = await fetch("/Dashboard/LiveData");
            data = await response.json();
        } catch {
            return;
        }

        renderStatsCards(data.stats);
        renderSensorCards(data.sensors);

        const emptyState = document.getElementById("emptyState");
        if (emptyState) {
            emptyState.style.display = data.sensors.length === 0 ? "" : "none";
        }
    }

    function renderStatsCards(stats) {
        const grid = document.getElementById("statsGrid");
        if (!grid || !stats) return;

        grid.innerHTML = `
            <div class="card"><h2>${stats.totalSensors}</h2><span class="subtitle">Registered sensors</span></div>
            <div class="card"><h2>${stats.onlineSensors} / ${stats.totalSensors}</h2><span class="subtitle">Online right now</span></div>
            <div class="card"><h2>🏆 ${stats.bestStreakEver}</h2><span class="subtitle">Best streak ever recorded</span></div>
            <div class="card"><h2>${stats.averageCurrentStreak}</h2><span class="subtitle">Average current streak</span></div>
        `;
    }

    function renderSensorCards(sensors) {
        const container = document.getElementById("sensorCards");
        if (!container) return;

        container.innerHTML = sensors.map(sensor => {
            const last = sensor.recentValues.length > 0 ? sensor.recentValues[sensor.recentValues.length - 1] : null;
            const isAnomaly = last !== null && (last < sensor.safeMin || last > sensor.safeMax);
            const borderColor = isAnomaly ? "#b3432b" : (sensor.isOnline ? "#2e7d5b" : "#cfc9b8");

            return `
                <div class="card sensor-card" data-sensor-id="${sensor.id}" style="border-top:4px solid ${borderColor};">
                    <div style="display:flex; justify-content:space-between; align-items:flex-start;">
                        <div>
                            <strong>${escapeHtml(sensor.macAddress)}</strong>
                            <div class="subtitle" style="margin:2px 0 10px;">${escapeHtml(sensor.deploymentLocation)}</div>
                        </div>
                        <span class="badge ${sensor.isOnline ? "online" : "offline"}">${sensor.isOnline ? "Online" : "Offline"}</span>
                    </div>
                    <div class="sparkline-wrap">
                        ${renderSparkline(sensor.recentValues, sensor.safeMin, sensor.safeMax)}
                        <div>
                            <div class="latest-value" style="font-size:1.3rem; font-weight:700; color:${isAnomaly ? "#b3432b" : "#101a33"};">
                                ${last !== null ? `${round(last)}${sensor.unit}` : "—"}
                            </div>
                            <div class="streak">🔥 ${sensor.currentStreak}-streak</div>
                        </div>
                    </div>
                    ${isAnomaly ? `<div class="alert error anomaly-alert" style="margin-top:10px;">⚠ Reading outside safe range (${sensor.safeMin} – ${sensor.safeMax}${sensor.unit}) — streak reset.</div>` : ""}
                </div>
            `;
        }).join("");
    }

    // Mirrors SmartX.Web.Services.SparklineRenderer.Render() exactly, so the
    // first server-rendered paint and every JS-polled refresh draw identically.
    function renderSparkline(values, safeMin, safeMax) {
        const width = 140, height = 44;
        if (!values || values.length < 2) {
            return `<svg width="${width}" height="${height}"></svg>`;
        }

        const min = Math.min(Math.min(...values), safeMin);
        let max = Math.max(Math.max(...values), safeMax);
        if (Math.abs(max - min) < 0.0001) max += 1;

        const step = width / (values.length - 1);
        const points = values.map((v, i) => {
            const x = i * step;
            const y = height - ((v - min) / (max - min)) * height;
            return `${x.toFixed(1)},${y.toFixed(1)}`;
        }).join(" ");

        const lastInRange = values[values.length - 1] >= safeMin && values[values.length - 1] <= safeMax;
        const stroke = lastInRange ? "#2e7d5b" : "#b3432b";

        return `<svg width="${width}" height="${height}" viewBox="0 0 ${width} ${height}"><polyline fill="none" stroke="${stroke}" stroke-width="2" points="${points}" /></svg>`;
    }

    // =====================================
    // HELPERS
    // =====================================

    function round(value) {
        return Math.round(value * 100) / 100;
    }

    function escapeHtml(value) {
        if (value === null || value === undefined) return "";
        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll("\"", "&quot;")
            .replaceAll("'", "&#039;");
    }

    return {
        pollSensorTable,
        pollDashboard
    };
})();
