#!/usr/bin/env bash
# BilimOS — Test the ARM64 ISO in QEMU (aarch64)

set -euo pipefail

ISO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/iso-build"
ISO=$(find "$ISO_DIR" -name "*arm64*.iso" -o -name "*.iso" | head -1)

if [[ -z "$ISO" ]]; then
    echo "No ISO found. Run ./build.sh first."
    exit 1
fi

# Install QEMU aarch64 if missing
for dep in qemu-system-aarch64; do
    if ! command -v "$dep" &>/dev/null; then
        echo "Installing $dep..."
        sudo apt-get install -y qemu-system-arm qemu-efi-aarch64
    fi
done

RAM="${QEMU_RAM:-4096}"
CORES="${QEMU_CORES:-2}"
DISK_IMG="${ISO_DIR}/test-disk-arm64.qcow2"

if [[ ! -f "$DISK_IMG" ]]; then
    echo "Creating 20GB virtual disk..."
    qemu-img create -f qcow2 "$DISK_IMG" 20G
fi

# ARM64 EFI firmware (OVMF for aarch64)
EFI_CODE="/usr/share/AAVMF/AAVMF_CODE.fd"
EFI_VARS="/usr/share/AAVMF/AAVMF_VARS.fd"
if [[ ! -f "$EFI_CODE" ]]; then
    echo "Installing AAVMF (ARM64 UEFI firmware)..."
    sudo apt-get install -y qemu-efi-aarch64
fi

# Copy VARS to writable location
VARS_COPY="${ISO_DIR}/AAVMF_VARS.fd"
[[ ! -f "$VARS_COPY" ]] && cp "$EFI_VARS" "$VARS_COPY"

echo "Starting BilimOS ARM64 in QEMU..."
echo "ISO: $ISO"
echo "RAM: ${RAM}MB | Cores: ${CORES}"
echo "Architecture: aarch64 (ARM64)"
echo ""

qemu-system-aarch64 \
    -M virt \
    -cpu cortex-a72 \
    -smp "$CORES" \
    -m "$RAM" \
    -drive if=pflash,format=raw,file="$EFI_CODE",readonly=on \
    -drive if=pflash,format=raw,file="$VARS_COPY" \
    -drive file="$DISK_IMG",format=qcow2,if=virtio \
    -cdrom "$ISO" \
    -boot order=dc \
    -device virtio-gpu \
    -display sdl \
    -device virtio-net-pci,netdev=net0 \
    -netdev user,id=net0 \
    -device virtio-keyboard-pci \
    -device virtio-mouse-pci \
    -name "BilimOS ARM64 Test"
