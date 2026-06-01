#!/usr/bin/env bash
# BilimOS — Write ISO to USB flash drive

set -euo pipefail

RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; NC='\033[0m'

ISO="${1:-}"
USB="${2:-}"
ISO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/iso-build"

if [[ -z "$ISO" ]]; then
    ISO=$(find "$ISO_DIR" -name "*.iso" | head -1)
    if [[ -z "$ISO" ]]; then
        echo -e "${RED}No ISO found. Run ./build.sh first.${NC}"
        exit 1
    fi
fi

if [[ -z "$USB" ]]; then
    echo "Available USB devices:"
    lsblk -d -o NAME,SIZE,TRAN,MODEL | grep -i usb || lsblk -d -o NAME,SIZE,TRAN,MODEL
    echo ""
    read -rp "Enter USB device (e.g. sdb, sdc): " dev_input
    USB="/dev/${dev_input}"
fi

if [[ ! -b "$USB" ]]; then
    echo -e "${RED}Invalid device: ${USB}${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}WARNING: All data on ${USB} will be ERASED!${NC}"
echo -e "ISO: ${ISO}"
echo -e "USB: ${USB}"
echo ""
read -rp "Type YES to continue: " confirm
[[ "$confirm" != "YES" ]] && { echo "Aborted."; exit 0; }

echo -e "${GREEN}Writing ${ISO} to ${USB}...${NC}"
sudo umount "${USB}"* 2>/dev/null || true
sudo dd if="$ISO" of="$USB" bs=4M status=progress oflag=sync
sudo sync
echo ""
echo -e "${GREEN}Done! BilimOS is ready on ${USB}${NC}"
echo "You can now boot from the USB drive."
