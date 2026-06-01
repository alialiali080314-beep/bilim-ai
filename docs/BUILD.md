# BilimOS — Руководство по сборке

## Системные требования для сборки

- **ОС:** Debian 12 (Bookworm) или Ubuntu 22.04+
- **RAM:** минимум 4 ГБ (рекомендуется 8 ГБ)
- **Диск:** 20+ ГБ свободного места
- **Права:** sudo

## Быстрый старт

```bash
# 1. Клонировать репозиторий
git clone https://github.com/alialiali080314-beep/bilim-ai
cd bilim-ai

# 2. Установить зависимости и собрать ISO
./build.sh

# Или по шагам:
./build.sh deps       # только установить зависимости
./build.sh configure  # только настроить
./build.sh build      # полная сборка
./build.sh clean      # очистить артефакты
```

## Создание загрузочного USB

```bash
# Автоматически (интерактивно)
./scripts/create-usb.sh

# Указать ISO и устройство явно
./scripts/create-usb.sh iso-build/bilimos-1.0-amd64.iso /dev/sdb
```

## Тестирование в виртуальной машине

```bash
# QEMU (4 ГБ RAM, 2 ядра)
./scripts/test-in-qemu.sh

# Параметры через переменные окружения:
QEMU_RAM=8192 QEMU_CORES=4 ./scripts/test-in-qemu.sh
```

## Структура проекта

```
bilim-ai/
├── build.sh                      # Главный скрипт сборки
├── config/
│   ├── package-lists/            # Список пакетов
│   │   ├── desktop.list.chroot   # GNOME рабочий стол
│   │   ├── apps.list.chroot      # Приложения
│   │   ├── system.list.chroot    # Системные пакеты
│   │   └── live.list.chroot      # Live-система
│   ├── hooks/
│   │   ├── live/                 # Хуки для live-образа
│   │   │   ├── 0010-bilimos-branding.hook.chroot
│   │   │   ├── 0020-locale-setup.hook.chroot
│   │   │   ├── 0030-user-setup.hook.chroot
│   │   │   ├── 0040-gnome-settings.hook.chroot
│   │   │   └── 0050-cleanup.hook.chroot
│   │   └── normal/               # Хуки для установленной системы
│   ├── includes.chroot/          # Файлы, копируемые в систему
│   │   ├── etc/                  # Конфигурация
│   │   └── usr/                  # Исполняемые файлы и данные
│   └── bootloaders/              # Загрузчики GRUB/isolinux
├── branding/                     # Обои, иконки, темы
│   └── generate-wallpapers.sh
├── scripts/
│   ├── create-usb.sh             # Запись на USB
│   └── test-in-qemu.sh           # Тестирование в QEMU
└── iso-build/                    # Готовый ISO (после сборки)
```

## Кастомизация

### Изменить список пакетов
Отредактируйте файлы в `config/package-lists/`:
- Добавьте пакет — одна строка с именем пакета
- Закомментируйте строку (`#`) — пакет не устанавливается

### Изменить фоновые обои
Замените SVG-файлы в `branding/` или добавьте PNG напрямую в
`config/includes.chroot/usr/share/bilimos/`.

### Изменить настройки GNOME
Редактируйте `config/hooks/live/0040-gnome-settings.hook.chroot` —
там находятся все dconf ключи для тем, раскладки, шрифтов и т.д.

### Изменить язык по умолчанию
В `config/hooks/live/0020-locale-setup.hook.chroot` измените значения
`LANG` и `LANGUAGE`.

## Технические детали

- **Базовый дистрибутив:** Debian 12 Bookworm
- **Рабочий стол:** GNOME (Adwaita Dark)
- **Загрузчик:** GRUB (UEFI + BIOS/MBR)
- **Инструмент сборки:** live-build (lb)
- **Файловая система live:** squashfs
- **Гибридный образ:** поддержка USB и DVD

## Требования к ПК для запуска BilimOS

| Компонент | Минимум | Рекомендуется |
|-----------|---------|---------------|
| CPU       | 64-bit dual-core | Quad-core 2+ GHz |
| RAM       | 2 ГБ | 4+ ГБ |
| Диск      | 20 ГБ | 50+ ГБ SSD |
| GPU       | VGA | OpenGL 2.0+ |
| Сеть      | — | Ethernet/Wi-Fi |
