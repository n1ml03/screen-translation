# Screen Translation OCR Servers

This directory contains Python-based OCR servers for the Screen Translation application.

## Installation

1. Install Python dependencies:
```bash
pip install -r requirements.txt
```

## Available OCR Methods

### OneOCR (Built-in)
- **Port:** None (built-in to Windows)
- **Requirements:** Windows 11 Snipping Tool files
- **Languages:** Windows system languages (automatic detection)
- **Start:** No server needed - built into the main application

### PaddleOCR
- **Port:** 9998
- **Requirements:** PaddlePaddle (GPU optional)
- **Languages:** Chinese, English, Japanese, Korean, etc.
- **Start:** Run `PaddleOCR/RunServerPaddleOCR.bat`

## OneOCR Setup (Windows Only)

OneOCR uses Windows Snipping Tool OCR engine and requires specific DLL files from Windows 11.

### Installation Steps:

1. **Download Snipping Tool files:**
   - Go to: https://store.rg-adguard.net
   - Search for: `https://apps.microsoft.com/detail/9mz95kl8mr0l`
   - Download the latest "Microsoft.ScreenSketch" msixbundle file

2. **Extract the files:**
   - Rename the downloaded `.msixbundle` file to `.zip`
   - Extract the zip file
   - Find and extract the `SnippingToolApp` msix file (choose x64 for 64-bit Windows)
   - Rename the msix file to `.zip` and extract again

3. **Copy required files:**
   - Create folder: `C:\Users\%USERNAME%\.config\oneocr\`
   - Copy these 3 files from the extracted SnippingTool folder:
     - `oneocr.dll`
     - `oneocr.onemodel`
     - `onnxruntime.dll`

4. **Install OneOCR Python package:**
   ```bash
   pip install oneocr
   ```

### API Usage:
```python
import oneocr
model = oneocr.OcrEngine()
result = model.recognize_pil(pil_image)
# Returns: {'text': 'full text', 'lines': [...], 'text_angle': angle}
```

## Usage

Each server runs independently. Choose the OCR method in the application settings and start the corresponding server.

The servers communicate with the main application via socket connections on their respective ports.
