#!/usr/bin/env bash
# BilimOS — Test the ISO in QEMU virtual machine

set -euo pipefail

ISO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/iso-build"
ISO=$(find "$ISO_DIR" -name "*.iso" | head -1)

if [[ -z "$ISO" ]]; then
    echo "No ISO found. Run ./build.sh first."
    exit 1
fi

# Check dependencies
for dep in qemu-system-x86_64; do
    if ! command -v "$dep" &>/dev/null; then
        echo "Installing $dep..."
        sudo apt-get install -y qemu-system-x86 qemu-kvm ovmf
    fi
done

RAM="${QEMU_RAM:-4096}"     # 4 GB RAM
CORES="${QEMU_CORES:-2}"    # 2 CPU cores
DISK_IMG="${ISO_DIR}/test-disk.qcow2"

# Create a virtual disk for testing installation
if [[ ! -f "$DISK_IMG" ]]; then
    echo "Creating 20GB virtual disk for installation testing..."
    qemu-img create -f qcow2 "$DISK_IMG" 20G
fi

echo "Starting BilimOS in QEMU..."
echo "ISO: $ISO"
echo "RAM: ${RAM}MB | Cores: ${CORES}"
echo ""

# Detect KVM support
KVM_FLAG=""
if [[ -r /dev/kvm ]]; then
    KVM_FLAG="-enable-kvm"
    echo "KVM acceleration: enabled"
else
    echo "KVM not available, using software emulation (slower)"
fi

# Run with UEFI (OVMF) if available, else BIOS
BIOS_FLAG=""
if [[ -f /usr/share/OVMF/OVMF_CODE.fd ]]; then
    BIOS_FLAG="-bios /usr/share/OVMF/OVMF_CODE.fd"
    echo "Firmware: UEFI (OVMF)"
else
    echo "Firmware: BIOS"
fi

qemu-system-x86_64 \
    $KVM_FLAG \
    $BIOS_FLAG \
    -m "$RAM" \
    -smp "$CORES" \
    -cpu host \
    -drive file="$DISK_IMG",format=qcow2,if=virtio \
    -cdrom "$ISO" \
    -boot order=dc \
    -vga virtio \
    -display sdl \
    -net nic,model=virtio \
    -net user \
    -soundhw hda \
    -usb -device usb-tablet \
    -name "BilimOS Test"
