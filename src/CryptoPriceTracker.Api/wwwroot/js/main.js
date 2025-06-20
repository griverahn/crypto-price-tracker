// wwwroot/js/main.js  
(() => {
  const tbody      = document.querySelector("#pricesTable tbody");
  const btnUpdate  = document.getElementById("updateBtn");
  const statusSpan = document.getElementById("statusMsg");
  const chartEl    = document.getElementById("priceChart");

  let chart;  // Chart.js instance
  const colors = { BTC:'#f7931a', ETH:'#3c6df0', ADA:'#0033ad' };

  /* ---------- Helpers ---------- */
  const toLocal = iso =>
    luxon.DateTime.fromISO(iso, { zone:'utc' })
         .setZone(luxon.DateTime.local().zoneName)
         .toLocaleString(luxon.DateTime.DATETIME_MED);

  const setStatus = (html, err = false) =>
    statusSpan.innerHTML =
      `<span class="badge ${err ? 'err' : 'ok'}">${html}</span>`;

  /* ---------- Draw chart  ---------- */
  async function draw(symbol) {
    const range = 30;  // last n days
    const res   = await fetch(`/api/crypto/history/${symbol}?days=${range}`);
    const rows  = await res.json();
    if (!rows.length) { setStatus('No data for ' + symbol, true); return; }

    const labels = rows.map(r => toLocal(r.dateUtc));
    const data   = rows.map(r => r.price);

    // Destroy old chart so scale resets
    if (chart) chart.destroy();
    chart = new Chart(chartEl, {
      type: 'line',
      data: {
        labels,
        datasets: [{
          label: symbol,
          data,
          borderColor: colors[symbol] ?? '#0ea5e9',
          pointRadius: 3,
          tension: 0.3,
          fill: false
        }]
      },
      options: {
        responsive: true,
        interaction: { intersect: false, mode: 'index' },
        scales: {
          y: {
            ticks: { callback: v => '$' + v.toLocaleString() }
          }
        }
      }
    });
  }

  /* ---------- Load table ---------- */
  async function loadTable() {
    tbody.innerHTML = '';
    const list = await fetch('/api/crypto/latest-prices').then(r => r.json());

    list.forEach(r => {
      const trendClass =
          r.trend === '▲' ? 'trend-up'
        : r.trend === '▼' ? 'trend-down'
        : 'trend-flat';

      const pctClass =
          r.percentageChange > 0 ? 'pct-up'
        : r.percentageChange < 0 ? 'pct-down'
        : '';

      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${r.iconUrl ? `<img src="${r.iconUrl}" width="24">` : r.symbol}</td>
        <td><button class="link" data-sym="${r.symbol}">${r.name}</button></td>
        <td>${r.symbol}</td>
        <td>$${r.price.toLocaleString()}</td>
        <td>${toLocal(r.timestampUtc)}</td>
        <td class="${pctClass}">
            ${r.percentageChange != null
              ? r.percentageChange.toFixed(2) + ' %' : '—'}
        </td>
        <td class="${trendClass}">${r.trend}</td>`;
      tr.classList.add('fade-in');
      tbody.appendChild(tr);
    });

    /* Click on name ⇒ draw chart */
    tbody.querySelectorAll('.link').forEach(btn => {
      btn.onclick = () => draw(btn.dataset.sym);
    });
  }

  /* ---------- Update Prices ---------- */
  btnUpdate.addEventListener('click', async () => {
    btnUpdate.disabled = true;
    btnUpdate.classList.add('disabled');
    setStatus('<span class="spin"></span> Updating…');

    const res  = await fetch('/api/crypto/update-prices', { method: 'POST' });
    const json = res.ok ? await res.json() : null;

    const cnt = json?.inserted ?? json?.updatedCount ?? 0;
    setStatus(res.ok ? `✅ Updated (${cnt})` : 'Update failed', !res.ok);
    await loadTable();

    btnUpdate.disabled = false;
    btnUpdate.classList.remove('disabled');
  });

  /* ---------- First load ---------- */
  loadTable();
})();
