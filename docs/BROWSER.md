# BilimOS в браузере — Руководство

BilimOS Web использует **v86** — эмулятор x86 на WebAssembly.  
Linux запускается прямо в браузере, без установки и без сервера.

---

## Быстрый запуск (готовый образ)

Открыть `web/index.html` в браузере, нажать **«▶ Запустить»**.  
По умолчанию загружается образ Debian с серверов copy.sh/v86.

Для локального запуска:

```bash
./scripts/serve-web.sh        # http://localhost:8080
./scripts/serve-web.sh 3000   # другой порт
```

---

## Сборка своего образа

Чтобы загружался именно BilimOS (с брендингом, настройками), собери образ:

```bash
# Требует Debian/Ubuntu с sudo, ~20 минут
sudo ./scripts/build-web-image.sh
```

После сборки в `web/images/` появится `bilimos-web.img`.  
Обнови в `web/js/app.js`:

```js
hda: { url: "images/bilimos-web.img", async: true, size: 0 },
```

---

## Развёртывание на хостинг

BilimOS Web — статический сайт (HTML + JS + образ диска).  
Работает на любом хостинге со статическими файлами.

### GitHub Pages

```bash
# Из корня репозитория
git subtree push --prefix web origin gh-pages
```

Доступ: `https://alialiali080314-beep.github.io/bilim-ai/`

### Netlify / Vercel (перетащить папку web/)

1. Открыть netlify.com → «Add new site» → «Deploy manually»
2. Перетащить папку `web/` в браузер
3. Готово — сайт доступен мгновенно

### Nginx

```nginx
server {
    listen 80;
    server_name bilimos.example.com;
    root /var/www/bilimos-web;
    index index.html;

    # Нужны правильные заголовки для SharedArrayBuffer (COOP/COEP)
    add_header Cross-Origin-Opener-Policy  "same-origin";
    add_header Cross-Origin-Embedder-Policy "require-corp";

    # Кэширование образов дисков
    location ~* \.(img|iso|bin|wasm)$ {
        expires 30d;
        add_header Cache-Control "public, immutable";
    }
}
```

---

## Структура веб-приложения

```
web/
├── index.html          # Главная страница
├── css/
│   └── style.css       # Тёмная тема BilimOS
├── js/
│   └── app.js          # Логика v86, управление ВМ
├── assets/
│   └── favicon.svg     # Иконка
└── images/             # Образы дисков (после сборки)
    ├── bilimos-web.img
    └── bilimos-web.img.gz
```

---

## Возможности интерфейса

| Функция | Описание |
|---------|----------|
| ▶ Запустить | Загрузить образ и запустить эмулятор |
| ■ Остановить | Завершить работу ВМ |
| ↺ Перезагрузить | Программная перезагрузка |
| Ctrl+Alt+Del | Отправить в ВМ |
| Буфер обмена | Скопировать текст из браузера в ВМ |
| Снимок экрана | Сохранить PNG с экрана ВМ |
| Полный экран | Развернуть на весь экран |
| Настройка RAM | 128 / 256 / 384 / 512 МБ |
| Разрешение | 640×480 / 800×600 / 960×600 |

---

## Требования к браузеру

| Браузер | Поддержка |
|---------|-----------|
| Chrome 89+ | Полная |
| Firefox 89+ | Полная |
| Safari 15+ | Полная |
| Edge 89+ | Полная |
| Safari iOS 15+ | Работает (медленнее) |
| Chrome Android | Работает |

**Минимум:** WebAssembly + 1 ГБ свободной RAM в браузере.

---

## Производительность

v86 эмулирует x86 CPU программно. Скорость зависит от хоста:

| Устройство | Относительная скорость |
|------------|------------------------|
| MacBook M2 / M3 | ~30–50 МГц x86 |
| ПК Intel Core i7 | ~20–40 МГц x86 |
| iPhone 15 | ~15–25 МГц x86 |
| iPhone 12 | ~10–20 МГц x86 |

Для максимальной скорости используй **BilimOS Lite (Buildroot)** — загружается за 5–10 секунд.
