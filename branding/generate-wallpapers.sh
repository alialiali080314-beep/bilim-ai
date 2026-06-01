#!/usr/bin/env bash
# Generate BilimOS wallpapers as SVG → PNG

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUT_DIR="${SCRIPT_DIR}/../config/includes.chroot/usr/share/bilimos"
mkdir -p "$OUT_DIR"

# Check for rsvg-convert or inkscape
if command -v rsvg-convert &>/dev/null; then
    CONVERTER="rsvg-convert"
elif command -v inkscape &>/dev/null; then
    CONVERTER="inkscape"
else
    echo "Install 'librsvg2-bin' or 'inkscape' to convert SVGs to PNG."
    echo "SVG files will still be created."
    CONVERTER=""
fi

svg_to_png() {
    local svg="$1" png="$2" width="$3" height="$4"
    if [[ "$CONVERTER" == "rsvg-convert" ]]; then
        rsvg-convert -w "$width" -h "$height" "$svg" -o "$png"
    elif [[ "$CONVERTER" == "inkscape" ]]; then
        inkscape "$svg" -w "$width" -h "$height" -o "$png"
    fi
}

# Light wallpaper SVG
cat > "${SCRIPT_DIR}/wallpaper.svg" <<'SVG'
<svg xmlns="http://www.w3.org/2000/svg" width="1920" height="1080" viewBox="0 0 1920 1080">
  <defs>
    <linearGradient id="bg" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%"   stop-color="#1a1a2e"/>
      <stop offset="50%"  stop-color="#16213e"/>
      <stop offset="100%" stop-color="#0f3460"/>
    </linearGradient>
    <filter id="glow">
      <feGaussianBlur stdDeviation="4" result="blur"/>
      <feComposite in="SourceGraphic" in2="blur" operator="over"/>
    </filter>
  </defs>
  <rect width="1920" height="1080" fill="url(#bg)"/>

  <!-- Grid lines -->
  <g stroke="#ffffff08" stroke-width="1">
    <line x1="0" y1="216" x2="1920" y2="216"/>
    <line x1="0" y1="432" x2="1920" y2="432"/>
    <line x1="0" y1="648" x2="1920" y2="648"/>
    <line x1="0" y1="864" x2="1920" y2="864"/>
    <line x1="384" y1="0" x2="384" y2="1080"/>
    <line x1="768" y1="0" x2="768" y2="1080"/>
    <line x1="1152" y1="0" x2="1152" y2="1080"/>
    <line x1="1536" y1="0" x2="1536" y2="1080"/>
  </g>

  <!-- Glowing circles -->
  <circle cx="960" cy="540" r="280" fill="none" stroke="#e94560" stroke-width="1.5" opacity="0.3" filter="url(#glow)"/>
  <circle cx="960" cy="540" r="200" fill="none" stroke="#0f3460" stroke-width="80" opacity="0.4"/>
  <circle cx="960" cy="540" r="140" fill="none" stroke="#e94560" stroke-width="1" opacity="0.5"/>

  <!-- Stars / particles -->
  <g fill="#ffffff" opacity="0.6">
    <circle cx="120" cy="80"   r="1.5"/>
    <circle cx="350" cy="150"  r="1"/>
    <circle cx="580" cy="60"   r="2"/>
    <circle cx="820" cy="130"  r="1.5"/>
    <circle cx="1100" cy="90"  r="1"/>
    <circle cx="1380" cy="170" r="2"/>
    <circle cx="1600" cy="55"  r="1.5"/>
    <circle cx="1800" cy="200" r="1"/>
    <circle cx="200" cy="900"  r="1.5"/>
    <circle cx="450" cy="980"  r="2"/>
    <circle cx="700" cy="920"  r="1"/>
    <circle cx="1200" cy="970" r="1.5"/>
    <circle cx="1500" cy="900" r="2"/>
    <circle cx="1750" cy="960" r="1"/>
  </g>

  <!-- Logo text -->
  <text x="960" y="520" font-family="'Liberation Sans', sans-serif" font-size="72"
        font-weight="bold" fill="#ffffff" text-anchor="middle" opacity="0.95"
        filter="url(#glow)">BilimOS</text>

  <text x="960" y="580" font-family="'Liberation Sans', sans-serif" font-size="24"
        fill="#e94560" text-anchor="middle" opacity="0.85"
        letter-spacing="8">ЗНАНИЕ БЕЗ ГРАНИЦ</text>

  <text x="960" y="620" font-family="'Liberation Sans', sans-serif" font-size="16"
        fill="#ffffff" text-anchor="middle" opacity="0.4">Version 1.0 "Ziyat"</text>

  <!-- Bottom bar -->
  <rect x="0" y="1055" width="1920" height="25" fill="#e9456015"/>
  <text x="960" y="1071" font-family="monospace" font-size="11"
        fill="#e94560" text-anchor="middle" opacity="0.5">Based on Debian Bookworm</text>
</svg>
SVG

# Dark wallpaper
cat > "${SCRIPT_DIR}/wallpaper-dark.svg" <<'SVG'
<svg xmlns="http://www.w3.org/2000/svg" width="1920" height="1080" viewBox="0 0 1920 1080">
  <defs>
    <radialGradient id="bg" cx="50%" cy="50%" r="70%">
      <stop offset="0%"   stop-color="#0d0d0d"/>
      <stop offset="100%" stop-color="#050505"/>
    </radialGradient>
  </defs>
  <rect width="1920" height="1080" fill="url(#bg)"/>
  <g stroke="#ffffff05" stroke-width="1">
    <line x1="0" y1="216" x2="1920" y2="216"/>
    <line x1="0" y1="432" x2="1920" y2="432"/>
    <line x1="0" y1="648" x2="1920" y2="648"/>
    <line x1="0" y1="864" x2="1920" y2="864"/>
    <line x1="384" y1="0" x2="384" y2="1080"/>
    <line x1="768" y1="0" x2="768" y2="1080"/>
    <line x1="1152" y1="0" x2="1152" y2="1080"/>
    <line x1="1536" y1="0" x2="1536" y2="1080"/>
  </g>
  <circle cx="960" cy="540" r="250" fill="none" stroke="#e94560" stroke-width="1" opacity="0.2"/>
  <text x="960" y="520" font-family="'Liberation Sans', sans-serif" font-size="72"
        font-weight="bold" fill="#e94560" text-anchor="middle" opacity="0.9">BilimOS</text>
  <text x="960" y="570" font-family="'Liberation Sans', sans-serif" font-size="20"
        fill="#ffffff" text-anchor="middle" opacity="0.3" letter-spacing="6">ЗНАНИЕ БЕЗ ГРАНИЦ</text>
</svg>
SVG

cp "${SCRIPT_DIR}/wallpaper.svg"      "${OUT_DIR}/wallpaper.svg"
cp "${SCRIPT_DIR}/wallpaper-dark.svg" "${OUT_DIR}/wallpaper-dark.svg"
echo "SVG wallpapers created in ${OUT_DIR}/"

if [[ -n "$CONVERTER" ]]; then
    svg_to_png "${SCRIPT_DIR}/wallpaper.svg"       "${OUT_DIR}/wallpaper.jpg"      1920 1080
    svg_to_png "${SCRIPT_DIR}/wallpaper-dark.svg"  "${OUT_DIR}/wallpaper-dark.jpg" 1920 1080
    svg_to_png "${SCRIPT_DIR}/wallpaper-dark.svg"  "${OUT_DIR}/grub-background.png" 1920 1080
    echo "PNG wallpapers generated."
else
    cp "${OUT_DIR}/wallpaper.svg"      "${OUT_DIR}/wallpaper.jpg"
    cp "${OUT_DIR}/wallpaper-dark.svg" "${OUT_DIR}/wallpaper-dark.jpg"
    cp "${OUT_DIR}/wallpaper-dark.svg" "${OUT_DIR}/grub-background.png"
    echo "Note: SVG files used as wallpapers (PNG conversion requires librsvg2-bin)."
fi
