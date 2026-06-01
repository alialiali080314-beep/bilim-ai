#!/usr/bin/env bash
# BilimOS — Запустить локальный веб-сервер для тестирования

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WEB_DIR="${SCRIPT_DIR}/../web"
PORT="${1:-8080}"

echo ""
echo "  BilimOS Web Server"
echo "  ─────────────────────────────────"
echo "  URL: http://localhost:${PORT}"
echo "  Dir: ${WEB_DIR}"
echo "  Ctrl+C для остановки"
echo ""

cd "$WEB_DIR"

# Выбрать доступный сервер
if command -v python3 &>/dev/null; then
    echo "  Сервер: Python 3"
    python3 -m http.server "$PORT" \
        --bind 0.0.0.0 \
        --directory "$WEB_DIR"
elif command -v python &>/dev/null; then
    echo "  Сервер: Python 2"
    python -m SimpleHTTPServer "$PORT"
elif command -v npx &>/dev/null; then
    echo "  Сервер: Node.js serve"
    npx serve "$WEB_DIR" -p "$PORT" -s
elif command -v php &>/dev/null; then
    echo "  Сервер: PHP"
    php -S "0.0.0.0:${PORT}" -t "$WEB_DIR"
else
    echo "  Установи Python3, Node.js или PHP для запуска сервера."
    echo "  Или просто открой web/index.html в браузере."
    exit 1
fi
