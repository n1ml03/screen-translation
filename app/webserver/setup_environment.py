#!/usr/bin/env python3
"""
Environment setup script for OCR servers
Replaces conda environment setup with pure Python solution
"""

import sys
import subprocess
import importlib.util
import os
import platform

def check_python_version():
    """Check if Python version is compatible (3.8+)"""
    if sys.version_info < (3, 8):
        print("❌ Python 3.8+ is required")
        return False
    print(f"✅ Python {sys.version}")
    return True

def check_and_install_package(package_name, import_name=None):
    """Check if package is installed, install if not"""
    if import_name is None:
        import_name = package_name

    try:
        if import_name == "PIL":
            import_name = "PIL.Image"
        elif import_name == "cv2":
            import_name = "cv2"

        # Try to import the package
        if import_name in ["torch", "torchvision"]:
            # Special handling for torch
            import torch
            print(f"✅ {package_name} is already installed")
            return True
        else:
            __import__(import_name)
            print(f"✅ {package_name} is already installed")
            return True
    except ImportError:
        print(f"📦 Installing {package_name}...")
        try:
            subprocess.check_call([sys.executable, "-m", "pip", "install", package_name])
            print(f"✅ {package_name} installed successfully")
            return True
        except subprocess.CalledProcessError as e:
            print(f"❌ Failed to install {package_name}: {e}")
            return False

def setup_easyocr():
    """Setup EasyOCR environment"""
    print("🚀 Setting up EasyOCR environment...")
    print("Installing required packages...")

    required_packages = [
        ("torch", "torch"),
        ("torchvision", "torchvision"),
        ("torchaudio", "torchaudio"),
        ("pillow", "PIL"),
        ("numpy", "numpy"),
        ("scipy", "scipy"),
        ("tqdm", "tqdm"),
        ("pyyaml", "yaml"),
        ("requests", "requests"),
        ("easyocr", "easyocr"),
        ("opencv-python", "cv2"),
        ("matplotlib", "matplotlib"),
    ]

    success = True
    for package_name, import_name in required_packages:
        if not check_and_install_package(package_name, import_name):
            success = False

    if success:
        print("✅ EasyOCR setup completed successfully!")
        print("Testing EasyOCR import...")
        try:
            import easyocr
            print("✅ EasyOCR imported successfully!")
        except ImportError as e:
            print(f"❌ EasyOCR import failed: {e}")
            success = False
    else:
        print("❌ EasyOCR setup failed!")

    return success

def setup_paddleocr():
    """Setup PaddleOCR environment"""
    print("🚀 Setting up PaddleOCR environment...")
    print("Installing required packages...")

    required_packages = [
        ("paddlepaddle-gpu", "paddle"),
        ("paddleocr", "paddleocr"),
        ("opencv-python", "cv2"),
        ("numpy", "numpy"),
        ("pillow", "PIL"),
        ("matplotlib", "matplotlib"),
        ("scipy", "scipy"),
        ("tqdm", "tqdm"),
        ("pyyaml", "yaml"),
        ("requests", "requests"),
    ]

    success = True
    for package_name, import_name in required_packages:
        if not check_and_install_package(package_name, import_name):
            success = False

    if success:
        print("✅ PaddleOCR setup completed successfully!")
        print("Testing PaddleOCR import...")
        try:
            import paddleocr
            print("✅ PaddleOCR imported successfully!")
        except ImportError as e:
            print(f"❌ PaddleOCR import failed: {e}")
            success = False
    else:
        print("❌ PaddleOCR setup failed!")

    return success

def download_models():
    """Download OCR models"""
    print("📥 Downloading OCR models...")

    try:
        import easyocr
        print("Downloading EasyOCR models...")
        # This will download models if they don't exist
        reader = easyocr.Reader(['ja', 'en'])
        print("✅ EasyOCR models downloaded successfully!")
    except ImportError:
        print("⚠️ EasyOCR not available, skipping model download")

    try:
        import paddleocr
        print("Downloading PaddleOCR models...")
        # PaddleOCR downloads models automatically when needed
        print("✅ PaddleOCR models ready!")
    except ImportError:
        print("⚠️ PaddleOCR not available, skipping model download")

def main():
    """Main setup function"""
    print("🔧 OCR Environment Setup")
    print("=" * 50)

    if not check_python_version():
        return False

    # Check if we want to setup EasyOCR or PaddleOCR
    if len(sys.argv) > 1:
        ocr_method = sys.argv[1].lower()
    else:
        print("Usage: python setup_environment.py [easyocr|paddleocr]")
        return False

    print(f"Setting up {ocr_method}...")

    if ocr_method == "easyocr":
        success = setup_easyocr()
    elif ocr_method == "paddleocr":
        success = setup_paddleocr()
    else:
        print(f"❌ Unknown OCR method: {ocr_method}")
        return False

    if success:
        download_models()
        print("🎉 Setup completed! You can now run the OCR server.")
    else:
        print("❌ Setup failed! Please check the errors above.")

    return success

if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)
