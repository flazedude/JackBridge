#!/bin/bash

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo ""
echo "==================================="
echo "JackBridge Setup Script"
echo "==================================="
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "ERROR: This script must be run as root"
    echo "Please run: sudo ./setup.sh"
    exit 1
fi

# Detect Linux distribution
detect_distro() {
    if [ -f /etc/os-release ]; then
        . /etc/os-release
        DISTRO=$ID
        DISTRO_LIKE=$ID_LIKE
    elif [ -f /etc/lsb-release ]; then
        . /etc/lsb-release
        DISTRO=$DISTRIB_ID
    else
        DISTRO=$(uname -s)
    fi
    echo "Detected distribution: $DISTRO"
}

# Install dependencies based on distribution
install_dependencies() {
    echo ""
    echo "Checking and installing dependencies..."
    
    # Normalize distro name using ID_LIKE fallback
    local distro_family="$DISTRO"
    if [ -n "$DISTRO_LIKE" ]; then
        case "$DISTRO_LIKE" in
            *ubuntu*|*debian*) distro_family="debian" ;;
            *fedora*) distro_family="fedora" ;;
            *rhel*|*centos*) distro_family="rhel" ;;
            *arch*) distro_family="arch" ;;
            *suse*) distro_family="opensuse" ;;
        esac
    fi
    
    case "$distro_family" in
        ubuntu|debian|linuxmint|pop|elementary|zorin|kali|raspbian|mx|antix|deepin|lmde)
            echo "Using apt package manager..."
            apt-get update -qq
            # libgtk-3-0 is for runtime, libnetfilter-queue1 for functionality
            apt-get install -y libnetfilter-queue1 libnfnetlink0 iptables libgtk-3-0
            ;;
        fedora)
            echo "Using dnf package manager..."
            dnf install -y libnetfilter_queue libnfnetlink iptables gtk3
            ;;
        rhel|centos|rocky|almalinux)
            echo "Using yum package manager..."
            yum install -y libnetfilter_queue libnfnetlink iptables gtk3
            ;;
        arch|manjaro|endeavouros|garuda)
            echo "Using pacman package manager..."
            pacman -Sy --noconfirm libnetfilter_queue libnfnetlink iptables gtk3
            ;;
        opensuse*|sles)
            echo "Using zypper package manager..."
            zypper install -y libnetfilter_queue1 libnfnetlink0 iptables gtk3
            ;;
        void)
            echo "Using xbps package manager..."
            xbps-install -Sy libnetfilter_queue libnfnetlink iptables gtk3
            ;;
        *)
            echo "WARNING: Unknown distribution '$DISTRO' (family: '$DISTRO_LIKE')"
            echo ""
            echo "Please manually install the following packages:"
            echo "  Debian/Ubuntu: sudo apt install libnetfilter-queue1 libnfnetlink0 iptables libgtk-3-0"
            echo "  Fedora:        sudo dnf install libnetfilter_queue libnfnetlink iptables gtk3"
            echo "  Arch:          sudo pacman -S libnetfilter_queue libnfnetlink iptables gtk3"
            echo ""
            read -p "Continue anyway? (y/n) " -n 1 -r
            echo
            if [[ ! $REPLY =~ ^[Yy]$ ]]; then
                exit 1
            fi
            ;;
    esac
    
    echo "Dependencies installed"
}

# Use /usr/local/lib (matches RPATH in binary)
detect_lib_path() {
    LIB_PATH="/usr/local/lib"
    echo "Library installation path: $LIB_PATH"
}

# Check if files exist in current directory
check_files() {
    echo ""
    echo "Checking for required files..."
    
    if [ ! -f "$SCRIPT_DIR/libjackbridge.so" ]; then
        echo "ERROR: libjackbridge.so not found in $SCRIPT_DIR"
        exit 1
    fi
    
    if [ ! -f "$SCRIPT_DIR/JackBridge" ]; then
        echo "ERROR: JackBridge binary not found in $SCRIPT_DIR"
        exit 1
    fi
    
    if [ ! -f "$SCRIPT_DIR/JackBridgeGUI" ]; then
         echo "WARNING: JackBridgeGUI binary not found in $SCRIPT_DIR - GUI will not be installed"
    fi
    
    echo "All files present"
}

# Install files
install_files() {
    echo ""
    echo "Installing JackBridge..."
    
    # Create directories if they don't exist
    mkdir -p "$LIB_PATH" /usr/local/bin /etc/jackbridge
    chmod 755 /etc/jackbridge
    
    # Copy library
    echo "Installing libjackbridge.so to $LIB_PATH..."
    cp "$SCRIPT_DIR/libjackbridge.so" "$LIB_PATH/"
    chmod 755 "$LIB_PATH/libjackbridge.so"
    
    # Copy binary
    echo "Installing JackBridge to /usr/local/bin..."
    cp "$SCRIPT_DIR/JackBridge" /usr/local/bin/
    chmod 755 /usr/local/bin/JackBridge

    if [ -f "$SCRIPT_DIR/JackBridgeGUI" ]; then
        echo "Installing JackBridgeGUI to /usr/local/bin..."
        cp "$SCRIPT_DIR/JackBridgeGUI" /usr/local/bin/
        chmod 755 /usr/local/bin/JackBridgeGUI 
    fi
    
    echo "Files installed"
}

# Update library cache
update_ldconfig() {
    echo ""
    echo "Updating library cache..."
    
    # Add /usr/local/lib to ld.so.conf if not already there
    if [ -d /etc/ld.so.conf.d ]; then
        if ! grep -q "^/usr/local/lib" /etc/ld.so.conf.d/* 2>/dev/null; then
            echo "/usr/local/lib" > /etc/ld.so.conf.d/jackbridge.conf
            if [ "$LIB_PATH" = "/usr/local/lib64" ]; then
                echo "/usr/local/lib64" >> /etc/ld.so.conf.d/jackbridge.conf
            fi
        fi
    fi
    
    # Run ldconfig
    if command -v ldconfig &> /dev/null; then
        ldconfig 2>/dev/null || true
        # Also ensure cache is regenerated
        ldconfig -v 2>/dev/null | grep -q jackbridge || true
        echo "Library cache updated"
    else
        echo "WARNING: ldconfig not found. You may need to reboot."
    fi
}

# Verify installation
verify_installation() {
    echo ""
    echo "Verifying installation..."
    
    # Check if binary is in PATH
    if command -v JackBridge &> /dev/null; then
        echo "JackBridge binary found in PATH"
    else
        echo "JackBridge binary not found in PATH"
        echo "  You may need to add /usr/local/bin to your PATH"
    fi
    
    # Check if library is loadable
    if ldd /usr/local/bin/JackBridge 2>/dev/null | grep -q "libjackbridge.so"; then
        if ldd /usr/local/bin/JackBridge 2>/dev/null | grep "libjackbridge.so" | grep -q "not found"; then
            echo "libjackbridge.so not loadable"
        else
            echo "libjackbridge.so is loadable"
        fi
    fi
    
    # Final test - try to run --help
    if /usr/local/bin/JackBridge --help &>/dev/null; then
        echo "JackBridge executable is working"
    else
        echo "JackBridge may have issues - try: sudo ldconfig"
    fi
}

# Main deployment
main() {
    detect_distro
    check_files
    install_dependencies
    detect_lib_path
    install_files
    update_ldconfig
    verify_installation
    
    echo ""
    echo "==================================="
    echo "Installation Complete!"
    echo "==================================="
    echo ""
    echo "You can now run JackBridge from anywhere:"
    echo "  sudo JackBridge --help"
    if [ -f /usr/local/bin/JackBridgeGUI ]; then
       echo "  sudo JackBridgeGUI      (Graphical Interface)"
    fi
    echo "  sudo JackBridge --proxy socks5://IP:PORT --rule \"app:*:*:TCP:PROXY\""
    echo ""
    echo "For cleanup after crash:"
    echo "  sudo JackBridge --cleanup"
    echo ""
}

main
