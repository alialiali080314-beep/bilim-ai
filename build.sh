#!/usr/bin/env bash
# BilimOS - Main build script
# Builds a live ISO image based on Debian Bookworm

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD_DIR="${SCRIPT_DIR}/build"
CONFIG_DIR="${SCRIPT_DIR}/config"
ISO_DIR="${SCRIPT_DIR}/iso-build"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log()   { echo -e "${GREEN}[BilimOS]${NC} $*"; }
warn()  { echo -e "${YELLOW}[WARN]${NC} $*"; }
error() { echo -e "${RED}[ERROR]${NC} $*" >&2; exit 1; }
info()  { echo -e "${BLUE}[INFO]${NC} $*"; }

check_dependencies() {
    log "Checking build dependencies..."
    local deps=(live-build debootstrap squashfs-tools xorriso grub-efi-arm64-bin qemu-user-static binfmt-support)
    local missing=()

    for dep in "${deps[@]}"; do
        if ! dpkg -l "$dep" &>/dev/null; then
            missing+=("$dep")
        fi
    done

    if [[ ${#missing[@]} -gt 0 ]]; then
        warn "Missing dependencies: ${missing[*]}"
        log "Installing missing dependencies..."
        sudo apt-get update -q
        sudo apt-get install -y "${missing[@]}"
    fi
    log "All dependencies satisfied."
}

clean_build() {
    log "Cleaning previous build artifacts..."
    if [[ -d "$BUILD_DIR" ]]; then
        sudo lb clean --all 2>/dev/null || true
        rm -rf "$BUILD_DIR"
    fi
    rm -f "${ISO_DIR}"/*.iso 2>/dev/null || true
    log "Clean complete."
}

configure_build() {
    log "Configuring live-build for BilimOS..."
    mkdir -p "$BUILD_DIR"
    cd "$BUILD_DIR"

    lb config \
        --architecture arm64 \
        --distribution bookworm \
        --archive-areas "main contrib non-free non-free-firmware" \
        --binary-image iso \
        --bootloader grub-efi \
        --debian-installer false \
        --memtest none \
        --iso-application "BilimOS" \
        --iso-publisher "BilimOS Project" \
        --iso-volume "BILIMOS_1_0_ARM64" \
        --system live \
        --username bilim \
        --hostname bilimos \
        --bootappend-live "boot=live components quiet splash locale=ru_RU.UTF-8 keyboard-layouts=ru,en" \
        --mirror-bootstrap "http://deb.debian.org/debian/" \
        --mirror-chroot "http://deb.debian.org/debian/" \
        --mirror-binary "http://deb.debian.org/debian/" \
        --firmware-chroot true \
        --firmware-binary true \
        2>&1 | tail -5

    cp -r "${CONFIG_DIR}/." "${BUILD_DIR}/config/"
    log "Configuration complete."
}

build_iso() {
    log "Building BilimOS ISO — this will take 20-60 minutes..."
    cd "$BUILD_DIR"
    sudo lb build 2>&1 | tee "${ISO_DIR}/build.log"

    local iso_file
    iso_file=$(find "$BUILD_DIR" -maxdepth 1 -name "*.iso" | head -1)
    if [[ -z "$iso_file" ]]; then
        error "ISO build failed. Check ${ISO_DIR}/build.log for details."
    fi

    mkdir -p "$ISO_DIR"
    cp "$iso_file" "${ISO_DIR}/bilimos-1.0-arm64.iso"
    log "ISO created: ${ISO_DIR}/bilimos-1.0-arm64.iso"

    local iso_size
    iso_size=$(du -sh "${ISO_DIR}/bilimos-1.0-arm64.iso" | cut -f1)
    log "ISO size: $iso_size"

    md5sum "${ISO_DIR}/bilimos-1.0-arm64.iso" > "${ISO_DIR}/bilimos-1.0-arm64.iso.md5"
    sha256sum "${ISO_DIR}/bilimos-1.0-arm64.iso" > "${ISO_DIR}/bilimos-1.0-arm64.iso.sha256"
    log "Checksums generated."
}

usage() {
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  build     Full clean build (default)"
    echo "  clean     Remove build artifacts"
    echo "  deps      Install build dependencies only"
    echo "  configure Configure live-build only"
    echo "  help      Show this help"
}

main() {
    echo ""
    echo -e "${BLUE}╔══════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║        BilimOS Build System          ║${NC}"
    echo -e "${BLUE}║   Debian-based Linux Distribution    ║${NC}"
    echo -e "${BLUE}╚══════════════════════════════════════╝${NC}"
    echo ""

    case "${1:-build}" in
        build)
            check_dependencies
            clean_build
            configure_build
            build_iso
            log "BilimOS build complete!"
            ;;
        clean)
            clean_build
            ;;
        deps)
            check_dependencies
            ;;
        configure)
            check_dependencies
            configure_build
            ;;
        help|--help|-h)
            usage
            ;;
        *)
            error "Unknown command: $1. Run '$0 help' for usage."
            ;;
    esac
}

main "$@"
