#!/bin/bash
#
# Builds SkyDrop.Desktop for Linux x64 and arm64 using NativeAOT cross-compilation.
#
# Prerequisites (Ubuntu 22.04/Jammy):
#   This script will attempt to install cross-compilation tools if not present.
#   Requires sudo access for package installation.
#
# Note: Cross-compilation on Linux is more fragile than native builds.
#       Consider using separate runners for each architecture if this fails.
#
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT="$REPO_ROOT/src/SkyDrop.Desktop/SkyDrop.Desktop.csproj"
ARTIFACTS_DIR="$REPO_ROOT/artifacts"

echo "Building SkyDrop.Desktop for Linux (x64 and arm64)..."
echo "Repository root: $REPO_ROOT"

# Detect current architecture
CURRENT_ARCH=$(uname -m)
echo "Current architecture: $CURRENT_ARCH"

# Clean previous artifacts
OUTPUT_X64="$ARTIFACTS_DIR/SkyDrop-Linux-x64"
OUTPUT_ARM64="$ARTIFACTS_DIR/SkyDrop-Linux-arm64"

rm -rf "$OUTPUT_X64" "$OUTPUT_ARM64"
mkdir -p "$OUTPUT_X64" "$OUTPUT_ARM64"

setup_cross_compile_arm64() {
    echo ""
    echo "=== Setting up arm64 cross-compilation toolchain ==="

    # Check if already set up
    if dpkg -l | grep -q "gcc-aarch64-linux-gnu"; then
        echo "Cross-compilation tools already installed"
        return 0
    fi

    echo "Installing cross-compilation dependencies..."

    # Add arm64 architecture
    sudo dpkg --add-architecture arm64

    # Get Ubuntu codename
    CODENAME=$(lsb_release -cs)
    echo "Ubuntu codename: $CODENAME"

    # Add arm64 sources
    sudo bash -c "cat > /etc/apt/sources.list.d/arm64.list <<EOF
deb [arch=arm64] http://ports.ubuntu.com/ubuntu-ports/ $CODENAME main restricted
deb [arch=arm64] http://ports.ubuntu.com/ubuntu-ports/ $CODENAME-updates main restricted
deb [arch=arm64] http://ports.ubuntu.com/ubuntu-ports/ $CODENAME-backports main restricted universe multiverse
EOF"

    # Fix existing sources to be amd64-only
    sudo sed -i -e 's/deb http/deb [arch=amd64] http/g' /etc/apt/sources.list
    sudo sed -i -e 's/deb mirror/deb [arch=amd64] mirror/g' /etc/apt/sources.list

    # Update and install
    sudo apt-get update
    sudo apt-get install -y clang llvm binutils-aarch64-linux-gnu gcc-aarch64-linux-gnu zlib1g-dev:arm64
}

setup_cross_compile_x64() {
    echo ""
    echo "=== Setting up x64 cross-compilation toolchain ==="

    # Check if already set up
    if dpkg -l | grep -q "gcc-x86-64-linux-gnu"; then
        echo "Cross-compilation tools already installed"
        return 0
    fi

    echo "Installing cross-compilation dependencies..."

    # Add amd64 architecture
    sudo dpkg --add-architecture amd64

    # Get Ubuntu codename
    CODENAME=$(lsb_release -cs)
    echo "Ubuntu codename: $CODENAME"

    # Update and install
    sudo apt-get update
    sudo apt-get install -y clang llvm binutils-x86-64-linux-gnu gcc-x86-64-linux-gnu zlib1g-dev:amd64
}

# Build based on current architecture
if [ "$CURRENT_ARCH" = "x86_64" ]; then
    # Native x64 build
    echo ""
    echo "=== Building for linux-x64 (native) ==="
    dotnet publish "$PROJECT" -c Release -r linux-x64 -o "$OUTPUT_X64"

    # Cross-compile for arm64
    setup_cross_compile_arm64
    echo ""
    echo "=== Building for linux-arm64 (cross-compile) ==="
    dotnet publish "$PROJECT" -c Release -r linux-arm64 -o "$OUTPUT_ARM64"

elif [ "$CURRENT_ARCH" = "aarch64" ]; then
    # Native arm64 build
    echo ""
    echo "=== Building for linux-arm64 (native) ==="
    dotnet publish "$PROJECT" -c Release -r linux-arm64 -o "$OUTPUT_ARM64"

    # Cross-compile for x64
    setup_cross_compile_x64
    echo ""
    echo "=== Building for linux-x64 (cross-compile) ==="
    dotnet publish "$PROJECT" -c Release -r linux-x64 -o "$OUTPUT_X64"
else
    echo "Unsupported architecture: $CURRENT_ARCH"
    exit 1
fi

# Verify builds
echo ""
echo "=== Build complete ==="

echo ""
echo "x64 output:"
ls -lh "$OUTPUT_X64" | grep -E "SkyDrop|\.so"

echo ""
echo "arm64 output:"
ls -lh "$OUTPUT_ARM64" | grep -E "SkyDrop|\.so"

echo ""
echo "Artifacts location: $ARTIFACTS_DIR"
