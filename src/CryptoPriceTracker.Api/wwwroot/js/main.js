// wwwroot/js/main.js
(() => {
  const tbody      = document.querySelector("#pricesTable tbody");
  const btnUpdate  = document.getElementById("updateBtn");
  const statusSpan = document.getElementById("statusMsg");
  const chartEl    = document.getElementById("priceChart");

  let priceChart;   // initiate Chart.js – one per symbol

  /* ---------- Helpers ---------- */
  const toLocal = utc =>
    luxon.DateTime.fromISO(utc, { zone: "utc" })
         .setZone(luxon.DateTime.local().zoneName)
         .toLocaleString(luxon.DateTime.DATETIME_MED);

  const setStatus = html => { statusSpan.innerHTML = html; };

  /* ---------- Call API for history and draw chart ---------- */
  async function drawChart(symbol) {
    const days   = 30; // range
    const res    = await fetch(`/api/crypto/history/${symbol}?days=${days}`);
    const series = await res.json();                 

    if (!series.length) { setStatus("No data yet"); return; }

    const labels = series.map(p => toLocal(p.dateUtc));
    const data   = series.map(p => p.price);

    if (priceChart) priceChart.destroy();

    priceChart = new Chart(chartEl, {
      type: "line",
      data: {
        labels,
        datasets: [{
          label: symbol,
          data,
          borderWidth: 2,
          fill: false,
          tension: 0.3
        }]
      },
      options: {
        responsive: true,
        plugins: { legend: { display: true } },
        scales:   { y: { beginAtZero: false } }
      }
    });
  }

  /* ---------- Load price table ---------- */
  async function loadTable() {
    tbody.innerHTML = "";
    const res  = await fetch("/api/crypto/latest-prices");
    const list = await res.json();

    list.forEach(r => {
      const trendClass =
          r.trend === "🔼" ? "trend-up"
        : r.trend === "🔽" ? "trend-down"
        : "trend-flat";

      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td>${r.iconUrl ? `<img src="${r.iconUrl}" width="24">`
                        : r.symbol}</td>
        <td><button class="link" data-sym="${r.symbol}">${r.name}</button></td>
        <td>${r.symbol}</td>
        <td>$${r.price.toLocaleString()}</td>
        <td>${toLocal(r.timestampUtc)}</td>
        <td>${r.percentageChange != null
              ? r.percentageChange.toFixed(2) + " %" : "-"}</td>
        <td class="${trendClass}">${r.trend}</td>
      `;
      tr.classList.add("fade-in");
      tbody.appendChild(tr);
    });

    /* click on name → chart */
    tbody.querySelectorAll(".link").forEach(btn => {
      btn.onclick = () => drawChart(btn.dataset.sym);
    });
  }

  /* ---------- Update Button ---------- */
  btnUpdate.addEventListener("click", async () => {
    btnUpdate.disabled = true;
    setStatus('<span class="spin"></span>&nbsp;Updating…');

    const res  = await fetch("/api/crypto/update-prices", { method: "POST" });
    const body = res.ok ? await res.json() : null;

    const count = body?.inserted ?? body?.updatedCount ?? 0;
    setStatus(res.ok ? `✅ Updated (${count})` : "❌ Update failed");

    await loadTable();
    btnUpdate.disabled = false;
  });

  /* ---------- First load ---------- */
  loadTable();
})();
