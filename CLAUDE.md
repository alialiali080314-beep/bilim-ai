# CLAUDE.md — BilimOS

BilimOS is a custom Linux distribution based on Debian 12 Bookworm.

## Build Commands

```bash
# Full ISO build (requires Debian/Ubuntu host with sudo)
./build.sh

# Individual steps:
./build.sh deps       # Install build dependencies (live-build, debootstrap, etc.)
./build.sh configure  # Configure live-build only
./build.sh clean      # Remove build artifacts
```

## Test Commands

```bash
# Test ISO in QEMU virtual machine
./scripts/test-in-qemu.sh

# Write finished ISO to a USB drive
./scripts/create-usb.sh [iso-file] [/dev/sdX]

# Regenerate SVG/PNG wallpapers (requires librsvg2-bin or inkscape)
./branding/generate-wallpapers.sh
```

## Architecture Overview

```
build.sh
  └─ calls live-build (lb config + lb build)
       ├─ config/package-lists/    → apt packages installed into live system
       ├─ config/hooks/live/       → chroot scripts run during image build
       ├─ config/includes.chroot/  → files overlaid onto the live filesystem
       └─ config/bootloaders/      → GRUB / isolinux boot menus
```

Key files:
- `build.sh` — orchestrates the build via `lb config` + `lb build`
- `config/hooks/live/00XX-*.hook.chroot` — numbered hooks: branding, locale, users, GNOME dconf, cleanup
- `config/includes.chroot/usr/local/bin/bilimos-installer` — interactive text-mode disk installer
- `config/includes.chroot/usr/local/bin/bilimos-firstboot` — one-shot systemd service after installation

## Key Conventions

- Hook numbering: `00XX` prefix controls execution order (0010, 0020, ...)
- All hook files must be `chmod +x` and named `*.hook.chroot`
- Package lists use `.list.chroot` suffix in `config/package-lists/`
- Files in `config/includes.chroot/` are overlaid verbatim onto the root filesystem
- Live user: `bilim` / password: `bilimos` — removed after disk installation
- Default locale: `ru_RU.UTF-8`, timezone: `Asia/Almaty`
- ISO output: `iso-build/bilimos-1.0-amd64.iso`
