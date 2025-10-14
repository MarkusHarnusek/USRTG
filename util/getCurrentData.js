(() => {
    const url = "http://10.214.10.8:8080/packet";
    if (document.getElementById('__live_fetch_panel')) {
        console.warn("Live fetch panel already present.");
        return;
    }

    const panel = document.createElement('div');
    panel.id = '__live_fetch_panel';
    Object.assign(panel.style, {
        position: 'fixed',
        right: '12px',
        top: '12px',
        width: '520px',
        height: '360px',
        zIndex: 999999,
        background: 'rgba(22,22,22,0.95)',
        color: '#e6e6e6',
        fontFamily: 'system-ui, sans-serif',
        fontSize: '13px',
        borderRadius: '8px',
        boxShadow: '0 6px 24px rgba(0,0,0,0.6)',
        padding: '8px',
        display: 'flex',
        flexDirection: 'column',
        gap: '8px'
    });

    // Helper to create styled elements easily
    function createEl(tag, styleObj = {}, props = {}) {
        const el = document.createElement(tag);
        Object.assign(el.style, styleObj);
        Object.assign(el, props);
        return el;
    }

    // First row: label, url span, interval input, buttons
    const row1 = createEl('div', { display: 'flex', gap: '8px', alignItems: 'center' });
    const label = createEl('strong', { fontSize: '13px' }, { innerText: 'Live fetch:' });
    const urlSpan = createEl('span', { opacity: '0.8' });
    urlSpan.textContent = url;

    const intervalInput = createEl('input', {
        width: '90px',
        marginLeft: 'auto',
        padding: '4px',
        borderRadius: '4px',
        border: '1px solid #444',
        background: '#111',
        color: '#eee',
    }, {
        type: 'number',
        min: '200',
        value: '2000',
        id: '__live_interval'
    });

    const startBtn = createEl('button', {
        marginLeft: '8px',
        padding: '6px 8px',
        borderRadius: '5px',
        border: 'none',
        cursor: 'pointer',
        background: '#0f9d58',
        color: 'white',
    }, {
        id: '__live_start',
        innerText: 'Start'
    });

    const stopBtn = createEl('button', {
        padding: '6px 8px',
        borderRadius: '5px',
        border: 'none',
        cursor: 'pointer',
        background: '#d23f31',
        color: 'white',
    }, {
        id: '__live_stop',
        innerText: 'Stop'
    });

    const closeBtn = createEl('button', {
        marginLeft: '6px',
        background: 'transparent',
        border: 'none',
        color: '#aaa',
        cursor: 'pointer',
    }, {
        id: '__live_close',
        title: 'Remove panel',
        innerText: '✕'
    });

    row1.append(label, urlSpan, intervalInput, startBtn, stopBtn, closeBtn);

    // Second row: status, last, and info text
    const row2 = createEl('div', { display: 'flex', gap: '8px', alignItems: 'center' });
    const statusSpan = createEl('span', { opacity: '0.85' }, { id: '__live_status', innerText: 'Stopped' });
    const lastSpan = createEl('span', { opacity: '0.6', marginLeft: '8px' }, { id: '__live_last', innerText: '—' });
    const infoSpan = createEl('span', { marginLeft: 'auto', color: '#999', fontSize: '12px' }, { innerText: 'Errors/backoff shown below' });
    row2.append(statusSpan, lastSpan, infoSpan);

    // Output textarea
    const out = createEl('textarea', {
        flex: '1',
        resize: 'none',
        borderRadius: '6px',
        padding: '8px',
        border: '1px solid #333',
        background: '#0b0b0b',
        color: '#eaeaea',
        overflow: 'auto',
        fontFamily: 'monospace',
        fontSize: '12px',
    }, {
        id: '__live_output',
        readOnly: true,
    });

    // Log div
    const log = createEl('div', {
        height: '46px',
        overflow: 'auto',
        borderRadius: '6px',
        padding: '6px',
        border: '1px solid #222',
        background: '#070707',
        color: '#c9c9c9',
        fontSize: '12px',
    }, {
        id: '__live_log',
    });

    panel.append(row1, row2, out, log);
    document.body.appendChild(panel);

    let running = false;
    let controller = null;
    let timer = null;
    let backoff = 0; // ms extra
    const maxBackoff = 30_000;

    function logMsg(s) {
        const t = new Date().toLocaleTimeString();
        log.innerText = `${t} — ${s}\n` + log.innerText;
    }

    async function fetchOnce() {
        if (!running) return;
        controller = new AbortController();
        const sig = controller.signal;
        const started = Date.now();
        statusSpan.innerText = 'Fetching...';
        try {
            const resp = await fetch(url, { method: 'GET', cache: 'no-cache', mode: 'cors', signal: sig });
            const took = Date.now() - started;
            if (!resp.ok) {
                const txt = await resp.text().catch(() => `<no body; status ${resp.status}>`);
                logMsg(`HTTP ${resp.status} (${took}ms)`);
                statusSpan.innerText = `HTTP ${resp.status}`;
                out.value = txt;
                lastSpan.innerText = `Last: ${new Date().toLocaleTimeString()}`;
                if (resp.status >= 500) backoff = Math.min(maxBackoff, (backoff || 1000) * 2);
                else backoff = 0;
            } else {
                const text = await resp.text();
                out.value = text;
                lastSpan.innerText = `Last: ${new Date().toLocaleTimeString()} (${took}ms)`;
                statusSpan.innerText = `OK (${took}ms)`;
                backoff = 0;
            }
        } catch (err) {
            const msg = (err && err.message) ? err.message : String(err);
            statusSpan.innerText = 'Error';
            logMsg(`Error: ${msg}`);
            if (msg.includes('Failed to fetch') || msg.includes('NetworkError') || msg.includes('CORS')) {
                out.value = `Fetch failed: ${msg}\n\nNote: this may be a CORS block (server must allow this origin) or the server is unreachable from your browser.`;
            } else {
                out.value = `Fetch error: ${msg}`;
            }
            backoff = Math.min(maxBackoff, (backoff || 1000) * 2);
        } finally {
            controller = null;
        }

        if (!running) return;
        const base = Math.max(200, Number(intervalInput.value) || 2000);
        const nextDelay = base + backoff;
        timer = setTimeout(fetchOnce, nextDelay);
        if (backoff) {
            logMsg(`Backoff: +${backoff}ms (next in ${nextDelay}ms)`);
        }
    }

    function start() {
        if (running) return;
        running = true;
        backoff = 0;
        statusSpan.innerText = 'Starting...';
        fetchOnce();
    }
    function stop() {
        running = false;
        if (timer) {
            clearTimeout(timer);
            timer = null;
        }
        if (controller) {
            try { controller.abort(); } catch (e) { }
            controller = null;
        }
        statusSpan.innerText = 'Stopped';
        logMsg('Stopped by user');
    }

    startBtn.addEventListener('click', start);
    stopBtn.addEventListener('click', stop);
    closeBtn.addEventListener('click', () => {
        stop();
        panel.remove();
    });

    // Press ESC to close panel
    const escHandler = (e) => {
        if (e.key === 'Escape') {
            panel.remove();
            window.removeEventListener('keydown', escHandler);
        }
    };
    window.addEventListener('keydown', escHandler);

    logMsg(`Panel ready. Click Start to begin (default ${intervalInput.value}ms).`);
})();
