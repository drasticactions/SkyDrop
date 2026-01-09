#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT="$REPO_ROOT/src/SkyDrop.Desktop/SkyDrop.Desktop.csproj"
OUTPUT_DIR="$REPO_ROOT/artifacts/SkyDrop-macOS-universal"

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

# Create universal binary using lipo
echo ""
echo "=== Creating universal binary ==="
lipo -create \
    "$REPO_ROOT/artifacts/osx-arm64/SkyDrop.Desktop" \
    "$REPO_ROOT/artifacts/osx-x64/SkyDrop.Desktop" \
    -output "$OUTPUT_DIR/SkyDrop.Desktop"

# Copy dylibs (already universal from NuGet packages)
echo "Copying universal dylibs..."
cp "$REPO_ROOT/artifacts/osx-arm64/"*.dylib "$OUTPUT_DIR/"

# Copy pdb if exists
if [ -f "$REPO_ROOT/artifacts/osx-arm64/SkyDrop.pdb" ]; then
    cp "$REPO_ROOT/artifacts/osx-arm64/SkyDrop.pdb" "$OUTPUT_DIR/"
fi

# Verify the universal binary
echo ""
echo "=== Verifying universal binary ==="
file "$OUTPUT_DIR/SkyDrop.Desktop"
lipo -info "$OUTPUT_DIR/SkyDrop.Desktop"

echo ""
echo "=== Build complete ==="
echo "Output: $OUTPUT_DIR"
ls -lh "$OUTPUT_DIR"

# Cleanup intermediate builds
rm -rf "$REPO_ROOT/artifacts/osx-arm64"
rm -rf "$REPO_ROOT/artifacts/osx-x64"
