import os
import json
import time
import tempfile
import math
import sys
from PIL import Image


# Global variables to manage OCR engine
OCR_ENGINE = None
CURRENT_LANG = None

def sanitize_float(value, default=0.0):
    """
    Sanitize a float value to ensure it's valid for JSON serialization.
    Converts Infinity and NaN to a default value.
    
    Args:
        value: The value to sanitize
        default: Default value to use if value is invalid (default: 0.0)
    
    Returns:
        float: A valid float value safe for JSON serialization
    """
    try:
        float_value = float(value)
        if math.isinf(float_value) or math.isnan(float_value):
            return default
        return float_value
    except (ValueError, TypeError):
        return default

def initialize_ocr_engine(lang='english'):
    """
    Initialize or reinitialize the OCR engine with the specified language.

    Args:
        lang (str): Language to use for OCR (default: 'english')

    Returns:
        OneOCR OcrEngine: Initialized OCR engine
    """
    global OCR_ENGINE, CURRENT_LANG

    # OneOCR uses Windows Snipping Tool OCR engine and doesn't need language mapping
    # The language is handled automatically by the underlying Windows OCR engine

    # Only reinitialize if language has changed (though OneOCR doesn't use language parameter)
    if OCR_ENGINE is None or CURRENT_LANG != lang:
        print(f"Initializing OneOCR engine...", file=sys.stderr)
        start_time = time.time()

        try:
            # Import OneOCR here to avoid import errors if not installed
            import oneocr
            OCR_ENGINE = oneocr.OcrEngine()  # OneOCR constructor doesn't take language parameter
            CURRENT_LANG = lang  # Store for tracking, though not used by OneOCR
            initialization_time = time.time() - start_time
            print(f"OneOCR initialization completed in {initialization_time:.2f} seconds", file=sys.stderr)
        except ImportError:
            print("OneOCR not installed. Please install it with: pip install oneocr", file=sys.stderr)
            print("Note: OneOCR requires Windows Snipping Tool DLL files to be placed in C:/Users/your_user/.config/oneocr/", file=sys.stderr)
            raise

        flag_file = os.path.join(tempfile.gettempdir(), "oneocr_ready.txt")
        with open(flag_file, "w") as f:
            f.write("READY")
        print("Ready flag created!", file=sys.stderr)
    else:
        print(f"Using existing OneOCR engine", file=sys.stderr)

    return OCR_ENGINE


# Initialize with default language at module load time
try:
    initialize_ocr_engine('english')
except:
    print("OneOCR not available during initialization - will initialize when needed", file=sys.stderr)

def process_image(image_path, lang='english', preprocess_images=False, upscale_if_needed=False, char_level="True"):
    """
    Process an image using OneOCR and return the OCR text.

    Args:
        image_path (str): Path to the image to process.
        lang (str): Language to use for OCR (default: 'english').
        preprocess_images (bool): Not used, kept for compatibility.
        upscale_if_needed (bool): Not used, kept for compatibility.
        char_level (str): Not used, kept for compatibility.

    Returns:
        dict: JSON-serializable dictionary with OCR text result.
    """
    # Check if image exists
    if not os.path.exists(image_path):
        return {"status": "error", "message": f"Image file not found: {image_path}"}

    try:
        # Start timing the OCR process
        start_time = time.time()

        # Open the image using PIL
        image = Image.open(image_path)

        # Ensure OCR engine is initialized with the correct language
        ocr_engine = initialize_ocr_engine(lang)

        # Use the initialized OCR engine with PIL image
        # OneOCR API: model.recognize_pil(pil_image) returns {'text': 'full text', 'text_angle': angle, 'lines': [...]}
        # We only need the text
        result_text = ocr_engine.recognize_pil(image)['text']

        # Calculate processing time
        processing_time = time.time() - start_time

        # Return simple text result
        return {
            "status": "success",
            "text": result_text if result_text else "",
            "processing_time_seconds": sanitize_float(processing_time)
        }

    except Exception as e:
        import traceback
        traceback.print_exc()
        return {
            "status": "error",
            "message": str(e)
        }

if __name__ == '__main__':
    import sys
    
    # Check if the correct number of arguments is provided
    if len(sys.argv) < 3:
        print(json.dumps({
            "status": "error",
            "message": "Usage: python process_image_oneocr.py <image_path> <language> [preprocess] [upscale] [char_level]"
        }))
        sys.exit(1)
    
    # Parse command line arguments
    image_path = sys.argv[1]
    language = sys.argv[2] if len(sys.argv) > 2 else 'english'
    preprocess = sys.argv[3].lower() == 'true' if len(sys.argv) > 3 else False
    upscale = sys.argv[4].lower() == 'true' if len(sys.argv) > 4 else False
    char_level = sys.argv[5] if len(sys.argv) > 5 else 'True'
    
    # Process the image
    result = process_image(image_path, language, preprocess, upscale, char_level)
    
    # Print the result as JSON (allow_nan=False to catch any invalid float values)
    print(json.dumps(result, ensure_ascii=False, allow_nan=False))
