#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT="$REPO_ROOT/src/SkyDrop.Desktop/SkyDrop.Desktop.csproj"
OUTPUT_DIR="$REPO_ROOT/artifacts"
APP_NAME="SkyDrop"
APP_BUNDLE="$OUTPUT_DIR/$APP_NAME.app"
ASSETS_DIR="$REPO_ROOT/Assets/Assets.xcassets/AppIcon.appiconset"

echo "Building SkyDrop.Desktop for macOS universal binary..."
echo "Repository root: $REPO_ROOT"

# Clean previous artifacts
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Build for arm64
echo ""
echo "=== Building for osx-arm64 ==="
dotnet publish "$PROJECT" -c Release -r osx-arm64 -o "$REPO_ROOT/artifacts/osx-arm64"

# Build for x64
echo ""
echo "=== Building for osx-x64 ==="
dotnet publish "$PROJECT" -c Release -r osx-x64 -o "$REPO_ROOT/artifacts/osx-x64"

# Create app bundle structure
echo ""
echo "=== Creating macOS app bundle ==="
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Create universal binary using lipo
echo "Creating universal binary..."
lipo -create \
    "$REPO_ROOT/artifacts/osx-arm64/SkyDrop.Desktop" \
    "$REPO_ROOT/artifacts/osx-x64/SkyDrop.Desktop" \
    -output "$APP_BUNDLE/Contents/MacOS/SkyDrop"

# Copy dylibs (already universal from NuGet packages)
echo "Copying universal dylibs..."
cp "$REPO_ROOT/artifacts/osx-arm64/"*.dylib "$APP_BUNDLE/Contents/MacOS/"

# Create iconset and generate .icns
echo "Generating app icon..."
ICONSET_DIR="$OUTPUT_DIR/AppIcon.iconset"
mkdir -p "$ICONSET_DIR"

# Copy and rename icons for iconutil (macOS iconset format)
cp "$ASSETS_DIR/16.png" "$ICONSET_DIR/icon_16x16.png"
cp "$ASSETS_DIR/32.png" "$ICONSET_DIR/icon_16x16@2x.png"
cp "$ASSETS_DIR/32.png" "$ICONSET_DIR/icon_32x32.png"
cp "$ASSETS_DIR/64.png" "$ICONSET_DIR/icon_32x32@2x.png"
cp "$ASSETS_DIR/128.png" "$ICONSET_DIR/icon_128x128.png"
cp "$ASSETS_DIR/256.png" "$ICONSET_DIR/icon_128x128@2x.png"
cp "$ASSETS_DIR/256.png" "$ICONSET_DIR/icon_256x256.png"
cp "$ASSETS_DIR/512.png" "$ICONSET_DIR/icon_256x256@2x.png"
cp "$ASSETS_DIR/512.png" "$ICONSET_DIR/icon_512x512.png"
cp "$ASSETS_DIR/1024.png" "$ICONSET_DIR/icon_512x512@2x.png"

# Convert iconset to icns
iconutil -c icns "$ICONSET_DIR" -o "$APP_BUNDLE/Contents/Resources/AppIcon.icns"
rm -rf "$ICONSET_DIR"

# Get version from project file or use default
VERSION="1.0.0"
if [ -f "$PROJECT" ]; then
    PROJ_VERSION=$(grep -o '<Version>[^<]*</Version>' "$PROJECT" 2>/dev/null | sed 's/<[^>]*>//g' || true)
    if [ -n "$PROJ_VERSION" ]; then
        VERSION="$PROJ_VERSION"
    fi
fi

# Create Info.plist
echo "Creating Info.plist..."
cat > "$APP_BUNDLE/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleExecutable</key>
    <string>SkyDrop</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon</string>
    <key>CFBundleIdentifier</key>
    <string>com.drasticactions.skydrop</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>SkyDrop</string>
    <key>CFBundleDisplayName</key>
    <string>SkyDrop</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>${VERSION}</string>
    <key>CFBundleVersion</key>
    <string>${VERSION}</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
    <key>LSApplicationCategoryType</key>
    <string>public.app-category.games</string>
</dict>
</plist>
EOF

# Verify the universal binary
echo ""
echo "=== Verifying universal binary ==="
file "$APP_BUNDLE/Contents/MacOS/SkyDrop"
lipo -info "$APP_BUNDLE/Contents/MacOS/SkyDrop"

echo ""
echo "=== Build complete ==="
echo "Output: $APP_BUNDLE"
echo ""
echo "Bundle contents:"
find "$APP_BUNDLE" -type f | sed "s|$APP_BUNDLE|$APP_NAME.app|g"

# Cleanup intermediate builds
rm -rf "$REPO_ROOT/artifacts/osx-arm64"
rm -rf "$REPO_ROOT/artifacts/osx-x64"

echo ""
echo "To run the app:"
echo "  open $APP_BUNDLE"
