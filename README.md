# BilimOS

**BilimOS** — Linux-дистрибутив на базе Debian 12 Bookworm.  
«Bilim» (bilim — каз./узб.) означает **«Знание»**.

```
  ██████╗ ██╗██╗     ██╗███╗   ███╗ ██████╗ ███████╗
  ██╔══██╗██║██║     ██║████╗ ████║██╔═══██╗██╔════╝
  ██████╔╝██║██║     ██║██╔████╔██║██║   ██║███████╗
  ██╔══██╗██║██║     ██║██║╚██╔╝██║██║   ██║╚════██║
  ██████╔╝██║███████╗██║██║ ╚═╝ ██║╚██████╔╝███████║
  ╚═════╝ ╚═╝╚══════╝╚═╝╚═╝     ╚═╝ ╚═════╝ ╚══════╝
  Version 1.0 "Ziyat" — Знание без границ
```

## Характеристики

| Параметр | Значение |
|----------|----------|
| Версия | 1.0 "Ziyat" |
| База | Debian 12 Bookworm |
| Рабочий стол | GNOME (тёмная тема) |
| Загрузчик | GRUB 2 (UEFI) |
| Архитектура | **ARM64** (aarch64) |
| Язык по умолчанию | Русский (ru_RU.UTF-8) |
| Часовой пояс | Asia/Almaty |
| Live-пользователь | bilim / bilimos |

## Платформы

| Устройство | Способ запуска |
|------------|----------------|
| ПК / Mac (ARM) | Прямая загрузка с USB / ISO |
| **iPhone / iPad** | **UTM (App Store) → Virtualize → ARM64** |
| Mac (Apple Silicon) | UTM / Parallels / VMware Fusion |
| Android | QEMU / Andronix (ARM64) |
| QEMU на Linux/Mac | `./scripts/test-in-qemu.sh` |

## Запуск на iPhone

1. Собрать ISO: `./build.sh`
2. Скопировать `iso-build/bilimos-1.0-arm64.iso` на iPhone (iCloud / AirDrop / кабель)
3. В **UTM**: `+` → **Virtualize** → Linux → выбрать ISO → RAM 2–4 ГБ → Save → ▶

Подробная инструкция: [docs/IPHONE-UTM.md](docs/IPHONE-UTM.md)

## Что включено

**Рабочий стол**
- GNOME с тёмной темой Adwaita
- Поддержка Wayland и X11
- Горячая клавиша переключения раскладки: `Alt+Shift`

**Приложения**
- Firefox ESR — браузер
- LibreOffice — офисный пакет
- VLC — медиаплеер
- GIMP — редактор изображений
- Thunderbird — почтовый клиент
- GParted — разметка дисков
- Timeshift — резервное копирование

**Система**
- Поддержка Wi-Fi / Bluetooth / принтеров
- Мультимедиа-кодеки (MP3, MP4, AAC, H.264...)
- Поддержка русского и казахского языков
- Интерактивный установщик на диск

## Быстрый старт

```bash
# Собрать ARM64 ISO
./build.sh

# Тест в QEMU (aarch64)
./scripts/test-in-qemu.sh

# Записать на USB (для ARM-устройств)
./scripts/create-usb.sh
```

Документация по сборке: [docs/BUILD.md](docs/BUILD.md)

## Установка

При загрузке с ISO запустите **«Установить BilimOS»** с рабочего стола.  
В UTM: диск будет называться `vda`.

## Структура репозитория

```
bilim-ai/
├── build.sh              # Главный скрипт сборки ISO
├── config/               # Конфигурация live-build
│   ├── package-lists/    # Списки пакетов
│   ├── hooks/            # Сборочные хуки
│   ├── includes.chroot/  # Файлы системы
│   └── bootloaders/      # GRUB EFI (arm64)
├── branding/             # Обои и иконки
├── scripts/              # Утилиты (USB, QEMU aarch64)
├── docs/
│   ├── BUILD.md          # Руководство по сборке
│   └── IPHONE-UTM.md     # Запуск на iPhone через UTM
└── iso-build/            # Готовый ISO (после сборки)
```

## Лицензия

Исходный код BilimOS распространяется под лицензией [GPL v3](https://www.gnu.org/licenses/gpl-3.0.html).  
Включённые пакеты распространяются под своими лицензиями согласно Debian.
