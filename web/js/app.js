"use strict";

// ── Конфигурация образов ───────────────────────────────────────────────────
// v86 использует готовые образы с copy.sh/v86.
// BilimOS базируется на Debian — используем официальный Debian образ от v86.
// Замени URL на свой образ после сборки через scripts/build-web-image.sh

const IMAGES = {
    // Официальный публичный образ Debian 12 от проекта v86 (как основа BilimOS)
    debian: {
        label: "BilimOS (Debian base)",
        bios:    "https://copy.sh/v86/bios/seabios.bin",
        vga_bios:"https://copy.sh/v86/bios/vgabios.bin",
        cdrom:   null,
        hda:     "https://copy.sh/v86/images/debian-9p-rootfs-flat.img",
        bzimage: "https://copy.sh/v86/images/buildroot-bzimage.bin",
        initrd:  null,
        mem:     256,
    },
    // Минимальный Linux (Buildroot) — загружается быстрее всего
    buildroot: {
        label: "BilimOS Lite (быстрый старт)",
        bios:    "https://copy.sh/v86/bios/seabios.bin",
        vga_bios:"https://copy.sh/v86/bios/vgabios.bin",
        cdrom:   null,
        hda:     null,
        bzimage: "https://copy.sh/v86/images/buildroot-bzimage.bin",
        initrd:  null,
        mem:     128,
    },
    // Alpine Linux — лёгкий, быстрый
    alpine: {
        label: "BilimOS Alpine",
        bios:    "https://copy.sh/v86/bios/seabios.bin",
        vga_bios:"https://copy.sh/v86/bios/vgabios.bin",
        cdrom:   "https://copy.sh/v86/images/alpine-x86-3.12.0.iso",
        hda:     null,
        bzimage: null,
        initrd:  null,
        mem:     256,
    },
};

// ── DOM элементы ───────────────────────────────────────────────────────────
const $ = id => document.getElementById(id);

const btnStart       = $("btn-start");
const btnStop        = $("btn-stop");
const btnRestart     = $("btn-restart");
const btnCtrlAltDel  = $("btn-send-ctrl-alt-del");
const btnSendText    = $("btn-send-text");
const btnFullscreen  = $("btn-fullscreen");
const btnScreenshot  = $("btn-screenshot");
const cfgRam         = $("cfg-ram");
const cfgRamVal      = $("cfg-ram-val");
const cfgDisplay     = $("cfg-display");
const clipboardArea  = $("clipboard-area");
const vmSplash       = $("vm-splash");
const screenContainer= $("screen_container");
const loadProgress   = $("load-progress");
const progressBar    = $("progress-bar");
const progressInfo   = $("progress-info");
const vmStatusbar    = $("vm-statusbar");
const statusText     = $("status-text");
const infoStatus     = $("info-status");
const infoUptime     = $("info-uptime");
const infoCpu        = $("sb-cpu");
const sbBootState    = $("sb-boot-state");

let emulator = null;
let uptimeInterval = null;
let uptimeSeconds  = 0;
let cpuInterval    = null;

// ── Проверка требований ────────────────────────────────────────────────────
function checkRequirements() {
    // WebAssembly
    const wasmOk = typeof WebAssembly !== "undefined";
    setReq("req-wasm", wasmOk, "WebAssembly");

    // Оперативная память (грубая оценка)
    const memOk = navigator.deviceMemory == null || navigator.deviceMemory >= 2;
    setReq("req-mem", memOk, "Память браузера");

    // Сеть — просто показываем ок
    setReq("req-net", true, "Загрузка образа");

    if (!wasmOk) {
        btnStart.disabled = true;
        setStatus("Браузер не поддерживает WebAssembly");
    }
}

function setReq(id, ok, label) {
    const el = $(id);
    el.className = "req-item " + (ok ? "ok" : "fail");
    el.innerHTML = `<span class="req-icon">${ok ? "✓" : "✗"}</span> ${label}`;
}

// ── Управление статусом ────────────────────────────────────────────────────
function setStatus(msg) { statusText.textContent = msg; }

function setBadge(state) {
    infoStatus.className = "status-badge status-" + state;
    const labels = { off: "Выкл", loading: "Загрузка", running: "Работает", stopped: "Стоп" };
    infoStatus.textContent = labels[state] || state;
}

function setButtons(running) {
    btnStart.disabled      = running;
    btnStop.disabled       = !running;
    btnRestart.disabled    = !running;
    btnCtrlAltDel.disabled = !running;
    btnSendText.disabled   = !running;
    cfgRam.disabled        = running;
    cfgDisplay.disabled    = running;
}

// ── Таймер uptime ──────────────────────────────────────────────────────────
function startUptime() {
    uptimeSeconds = 0;
    uptimeInterval = setInterval(() => {
        uptimeSeconds++;
        const m = Math.floor(uptimeSeconds / 60);
        const s = String(uptimeSeconds % 60).padStart(2, "0");
        infoUptime.textContent = `${m}:${s}`;
    }, 1000);
}

function stopUptime() {
    clearInterval(uptimeInterval);
    uptimeInterval = null;
}

// ── RAM slider ─────────────────────────────────────────────────────────────
cfgRam.addEventListener("input", () => {
    cfgRamVal.textContent = cfgRam.value + " МБ";
    $("info-ram").textContent = cfgRam.value + " МБ";
});

// ── Запуск ─────────────────────────────────────────────────────────────────
function startVM() {
    const ram      = parseInt(cfgRam.value);
    const [w, h]   = cfgDisplay.value.split("x").map(Number);
    const profile  = IMAGES.debian; // можно сделать выбор из UI

    // Переключить UI
    vmSplash.style.display    = "none";
    loadProgress.style.display= "flex";
    vmStatusbar.style.display = "flex";
    setStatus("Загрузка образа BilimOS...");
    setBadge("loading");
    setButtons(true);

    const config = {
        wasm_path: "https://cdn.jsdelivr.net/gh/copy/v86@master/build/v86.wasm",
        memory_size:     ram * 1024 * 1024,
        vga_memory_size: 8   * 1024 * 1024,
        screen_container: screenContainer,
        bios:     { url: profile.bios     },
        vga_bios: { url: profile.vga_bios },
        autostart: true,
        disable_keyboard: false,
        acpi: true,
    };

    if (profile.bzimage) config.bzimage = { url: profile.bzimage };
    if (profile.hda)     config.hda     = { url: profile.hda,    async: true, size: 0 };
    if (profile.cdrom)   config.cdrom   = { url: profile.cdrom,  async: true, size: 0 };
    if (profile.initrd)  config.initrd  = { url: profile.initrd  };

    try {
        emulator = new V86(config);
    } catch (e) {
        showError("Ошибка запуска эмулятора: " + e.message);
        return;
    }

    // Прогресс загрузки сетевых ресурсов
    emulator.add_listener("download_progress", ev => {
        if (ev.file_name && ev.loaded != null && ev.total) {
            const pct = Math.round(ev.loaded / ev.total * 100);
            progressBar.style.width = pct + "%";
            const mb  = (ev.loaded / 1048576).toFixed(1);
            const tot = (ev.total  / 1048576).toFixed(1);
            progressInfo.textContent = `${mb} МБ / ${tot} МБ`;
        }
    });

    emulator.add_listener("emulator-ready", () => {
        loadProgress.style.display    = "none";
        screenContainer.style.display = "flex";
        setStatus("BilimOS запускается...");
        sbBootState.textContent = "Загрузка ядра...";
        setBadge("running");
        startUptime();

        // CPU мониторинг
        cpuInterval = setInterval(() => {
            if (emulator) {
                const stats = emulator.v86.cpu.get_statistics
                    ? emulator.v86.cpu.get_statistics()
                    : null;
                if (stats) {
                    const pct = Math.min(100, Math.round(stats.cpu_time_percent || 0));
                    infoCpu.textContent = `CPU: ${pct}%`;
                }
            }
        }, 2000);
    });

    emulator.add_listener("screen-set-mode", mode => {
        if (mode === "text") {
            sbBootState.textContent = "Текстовый режим";
        } else {
            sbBootState.textContent = "Графический режим";
            setStatus("BilimOS работает");
        }
    });

    emulator.add_listener("emulator-stopped", () => {
        setStatus("ВМ остановлена");
        setBadge("stopped");
        sbBootState.textContent = "Остановлена";
        stopUptime();
        clearInterval(cpuInterval);
        setButtons(false);
    });
}

function showError(msg) {
    loadProgress.style.display = "none";
    vmSplash.style.display     = "flex";
    setStatus("Ошибка: " + msg);
    setBadge("stopped");
    setButtons(false);
    console.error("[BilimOS]", msg);
}

// ── Кнопки управления ──────────────────────────────────────────────────────
btnStart.addEventListener("click", startVM);

btnStop.addEventListener("click", () => {
    if (!emulator) return;
    emulator.stop();
    screenContainer.style.display = "none";
    vmSplash.style.display        = "flex";
    vmStatusbar.style.display     = "none";
    setButtons(false);
    setBadge("off");
    setStatus("ВМ остановлена");
    stopUptime();
    clearInterval(cpuInterval);
    emulator = null;
});

btnRestart.addEventListener("click", () => {
    if (!emulator) return;
    emulator.restart();
    setStatus("Перезагрузка...");
    sbBootState.textContent = "Перезагрузка...";
    uptimeSeconds = 0;
});

btnCtrlAltDel.addEventListener("click", () => {
    if (!emulator) return;
    emulator.keyboard_send_scancodes([
        0x1d, 0x38, 0x53,  // нажать Ctrl+Alt+Del
        0xd3, 0xb8, 0x9d,  // отпустить
    ]);
});

// Отправить текст из буфера обмена в ВМ
btnSendText.addEventListener("click", () => {
    if (!emulator || !clipboardArea.value) return;
    emulator.keyboard_send_text(clipboardArea.value);
    clipboardArea.value = "";
});

// Полный экран
btnFullscreen.addEventListener("click", () => {
    const el = document.documentElement;
    if (!document.fullscreenElement) {
        el.requestFullscreen?.() || el.webkitRequestFullscreen?.();
    } else {
        document.exitFullscreen?.() || document.webkitExitFullscreen?.();
    }
});

// Скриншот
btnScreenshot.addEventListener("click", () => {
    if (!emulator) return;
    const canvas = document.getElementById("vga_canvas");
    if (!canvas) return;
    const a = document.createElement("a");
    a.download = "bilimos-screenshot.png";
    a.href = canvas.toDataURL("image/png");
    a.click();
});

// ── Init ───────────────────────────────────────────────────────────────────
checkRequirements();
