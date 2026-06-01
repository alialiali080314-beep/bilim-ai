#!/usr/bin/env bash
# BilimOS — Build minimal x86 disk image for browser (v86)
# Output: web/images/bilimos-web.img  (~200 МБ)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="${SCRIPT_DIR}/.."
OUT_DIR="${ROOT_DIR}/web/images"
IMG="${OUT_DIR}/bilimos-web.img"
MOUNT_DIR="${ROOT_DIR}/build/webimg-root"

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()   { echo -e "${GREEN}[build-web-image]${NC} $*"; }
warn()  { echo -e "${YELLOW}[WARN]${NC} $*"; }
error() { echo -e "${RED}[ERROR]${NC} $*" >&2; exit 1; }

check_deps() {
    local deps=(debootstrap qemu-img extlinux mtools wget)
    local missing=()
    for d in "${deps[@]}"; do
        command -v "$d" &>/dev/null || missing+=("$d")
    done
    if [[ ${#missing[@]} -gt 0 ]]; then
        log "Установка зависимостей: ${missing[*]}"
        sudo apt-get update -q
        sudo apt-get install -y "${missing[@]}" extlinux syslinux-common
    fi
}

build_image() {
    mkdir -p "$OUT_DIR" "$MOUNT_DIR"

    # 1. Создать образ диска 512 МБ (достаточно для минимальной системы)
    log "Создание образа диска 512 МБ..."
    qemu-img create -f raw "$IMG" 512M
    # Разметить как одну ext4-партицию
    /sbin/parted -s "$IMG" mklabel msdos mkpart primary ext4 1MiB 100%
    /sbin/parted -s "$IMG" set 1 boot on

    # 2. Форматировать
    log "Форматирование ext4..."
    LOOP=$(sudo losetup --show -f -P "$IMG")
    sudo mkfs.ext4 -L bilimos-web "${LOOP}p1"
    sudo mount "${LOOP}p1" "$MOUNT_DIR"

    # 3. debootstrap — минимальный Debian
    log "Установка Debian Bookworm (минимальная)..."
    sudo debootstrap \
        --arch=i386 \
        --variant=minbase \
        --include=linux-image-686,syslinux,extlinux,busybox,\
net-tools,iproute2,curl,bash,coreutils,util-linux,procps \
        bookworm \
        "$MOUNT_DIR" \
        http://deb.debian.org/debian/

    # 4. Настройка системы
    log "Настройка BilimOS Web..."
    sudo chroot "$MOUNT_DIR" bash -c "
        echo 'bilimos-web' > /etc/hostname
        echo 'root:bilimos' | chpasswd
        useradd -m -s /bin/bash bilim
        echo 'bilim:bilimos' | chpasswd

        # Автологин в консоль
        mkdir -p /etc/systemd/system/getty@tty1.service.d
        cat > /etc/systemd/system/getty@tty1.service.d/autologin.conf <<'EOF'
[Service]
ExecStart=
ExecStart=-/sbin/agetty --autologin bilim --noclear %I \$TERM
EOF

        # Приветствие
        cat > /etc/motd <<'MOTD'

  ██████╗ ██╗██╗     ██╗███╗   ███╗ ██████╗ ███████╗
  ██╔══██╗██║██║     ██║████╗ ████║██╔═══██╗██╔════╝
  ██████╔╝██║██║     ██║██╔████╔██║██║   ██║███████╗
  ██╔══██╗██║██║     ██║██║╚██╔╝██║██║   ██║╚════██║
  ██████╔╝██║███████╗██║██║ ╚═╝ ██║╚██████╔╝███████║

  BilimOS 1.0 Web Edition — bilim / bilimos
MOTD

        # Bashrc пользователя
        cat >> /home/bilim/.bashrc <<'BASHRC'
export PS1='\[\033[1;32m\]bilim@bilimos\[\033[0m\]:\[\033[1;34m\]\w\[\033[0m\]\$ '
alias ll='ls -la --color=auto'
alias cls='clear'
echo ''
neofetch 2>/dev/null || echo 'BilimOS 1.0 Web'
BASHRC
    "

    # 5. Extlinux bootloader
    log "Установка загрузчика..."
    sudo extlinux --install "${MOUNT_DIR}/boot"
    sudo dd if=/usr/lib/syslinux/mbr/mbr.bin of="$LOOP" bs=440 count=1 conv=notrunc

    KERNEL=$(ls "${MOUNT_DIR}/boot/vmlinuz-"* 2>/dev/null | head -1 | xargs basename)
    INITRD=$(ls "${MOUNT_DIR}/boot/initrd.img-"* 2>/dev/null | head -1 | xargs basename)

    sudo bash -c "cat > ${MOUNT_DIR}/boot/extlinux.conf <<EOF
DEFAULT bilimos
TIMEOUT 30
PROMPT 0

LABEL bilimos
  MENU LABEL BilimOS 1.0 Web
  LINUX /boot/${KERNEL}
  INITRD /boot/${INITRD}
  APPEND root=/dev/sda1 rw quiet console=tty1 loglevel=3
EOF"

    # 6. Очистить и размонтировать
    sudo chroot "$MOUNT_DIR" apt-get clean
    sudo umount "$MOUNT_DIR"
    sudo losetup -d "$LOOP"

    log "Образ готов: ${IMG}"
    local sz
    sz=$(du -sh "$IMG" | cut -f1)
    log "Размер: ${sz}"
}

compress_image() {
    log "Сжатие образа для веб..."
    gzip -k -9 "$IMG"
    log "Сжатый образ: ${IMG}.gz"
    ls -lh "${IMG}" "${IMG}.gz"
}

main() {
    log "Сборка BilimOS Web Image (для v86)..."
    check_deps
    build_image
    compress_image
    log "Готово! Обнови web/js/app.js: hda.url = 'images/bilimos-web.img'"
}

main "$@"
