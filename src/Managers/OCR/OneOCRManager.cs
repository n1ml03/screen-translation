using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace ScreenTranslation
{
    public class OneOCRManager
    {
        private static OneOCRManager? _instance;
        public string _currentLanguageCode = string.Empty;

        // Singleton pattern
        public static OneOCRManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OneOCRManager();
                }
                return _instance;
            }
        }

        private OneOCRManager()
        {
            // Private constructor for singleton
        }

        /// <summary>
        /// Get OCR text from image file using OneOCR directly
        /// </summary>
        public async Task<string> GetOcrTextFromFileAsync(string imagePath, string sourceLanguage)
        {
            string ocrText = string.Empty;

            try
            {
                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"[OneOCR ERROR] Image file not found: {imagePath}");
                    return ocrText;
                }

                // Call OneOCR process directly
                ocrText = await ProcessImageWithOneOCRAsync(imagePath, sourceLanguage);

                if (!string.IsNullOrWhiteSpace(ocrText))
                {
                    Console.WriteLine($"[OneOCR] ✓ Recognized text ({ocrText.Length} chars)");
                }
                else
                {
                    Console.WriteLine($"[OneOCR] No text detected in image");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OneOCR ERROR] {ex.Message}");
            }

            return ocrText;
        }

        /// <summary>
        /// Process OCR text and display on overlay
        /// </summary>
        public async Task ProcessOneOcrText(string ocrText, string sourceLanguage)
        {
            try
            {
                // Get the monitor window instance
                var monitorWindow = MonitorWindow.Instance;
                if (monitorWindow == null)
                {
                    Console.WriteLine("[OneOCR ERROR] MonitorWindow instance is null");
                    return;
                }

                // Clear existing text objects
                Logic.Instance.ClearAllTextObjects();

                // Add text object to overlay (centered position)
                if (!string.IsNullOrWhiteSpace(ocrText))
                {
                    Logic.Instance.AddTextObject(ocrText, 10, 10, 0, 0);
                    
                    // Start translation if enabled
                    if (ConfigManager.Instance.IsAutoTranslateEnabled())
                    {
                        Console.WriteLine("[OneOCR] → Starting translation...");
                        await Logic.Instance.TranslateTextObjectsAsync();
                    }
                    else
                    {
                        // If translation is disabled, add text directly to ChatBox
                        Console.WriteLine("[OneOCR] → Adding to ChatBox (translation disabled)");
                        MainWindow.Instance.AddTranslationToHistory(ocrText, "");
                        MonitorWindow.Instance.RefreshOverlays();
                    }
                }
                else
                {
                    Console.WriteLine("[OneOCR] Empty text, skipping");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OneOCR ERROR] Failed to process text: {ex.Message}");
            }
            finally
            {
                // Always re-enable OCR check for next frame
                MainWindow.Instance.SetOCRCheckIsWanted(true);
            }
        }

        /// <summary>
        /// Call OneOCR Python script directly
        /// </summary>
        private async Task<string> ProcessImageWithOneOCRAsync(string imagePath, string language)
        {
            string ocrText = string.Empty;

            try
            {
                // Get the path to Python executable
                string pythonPath = GetPythonExecutablePath();

                // Get the path to OneOCR script
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string oneocrScriptPath = Path.Combine(baseDirectory, "webserver", "OneOCR", "process_image_oneocr.py");

                if (!File.Exists(oneocrScriptPath))
                {
                    throw new FileNotFoundException($"Script not found: {oneocrScriptPath}");
                }

                // Prepare arguments (simplified - no need for char_level since we only get text)
                string arguments = $"\"{oneocrScriptPath}\" \"{imagePath}\" \"{language}\"";

                // Create process start info
                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = baseDirectory
                };

                // Start the process
                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        throw new Exception("Failed to start Python process");
                    }

                    // Read output and error asynchronously
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    // Wait for process to complete
                    bool exited = await Task.Run(() => process.WaitForExit(30000)); // 30 second timeout

                    if (!exited)
                    {
                        process.Kill();
                        throw new Exception("Python process timed out (30s)");
                    }

                    string output = await outputTask;
                    string error = await errorTask;

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"[OneOCR WARNING] Python stderr: {error.Trim()}");
                    }

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Python exit code {process.ExitCode}: {error}");
                    }

                    // Parse JSON output
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        ocrText = ParseOcrJsonOutput(output);
                    }
                    else
                    {
                        throw new Exception("Empty Python output");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OneOCR ERROR] Python call failed: {ex.Message}");
                throw;
            }

            return ocrText;
        }

        /// <summary>
        /// Get Python executable path
        /// </summary>
        private string GetPythonExecutablePath()
        {
            // Try to find python in the virtual environment first
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            
            // Check Windows path in base directory (venv\Scripts\python.exe)
            string venvPythonPathWindows = Path.Combine(baseDirectory, "venv", "Scripts", "python.exe");
            if (File.Exists(venvPythonPathWindows))
            {
                return venvPythonPathWindows;
            }

            // Check Windows path one level up (..\venv\Scripts\python.exe) - for when app runs from app folder
            string venvPythonPathWindowsParent = Path.Combine(baseDirectory, "..", "venv", "Scripts", "python.exe");
            if (File.Exists(venvPythonPathWindowsParent))
            {
                return Path.GetFullPath(venvPythonPathWindowsParent);
            }

            // Check Unix/Linux path (venv/bin/python)
            string venvPythonPathUnix = Path.Combine(baseDirectory, "venv", "bin", "python");
            if (File.Exists(venvPythonPathUnix))
            {
                return venvPythonPathUnix;
            }

            // Check Unix/Linux path one level up (../venv/bin/python)
            string venvPythonPathUnixParent = Path.Combine(baseDirectory, "..", "venv", "bin", "python");
            if (File.Exists(venvPythonPathUnixParent))
            {
                return Path.GetFullPath(venvPythonPathUnixParent);
            }

            // Fallback to system python
            return "python";
        }

        /// <summary>
        /// Parse JSON output from OneOCR
        /// </summary>
        private string ParseOcrJsonOutput(string jsonOutput)
        {
            try
            {
                // Try to parse as JSON
                var jsonResult = JsonConvert.DeserializeObject<dynamic>(jsonOutput);

                if (jsonResult?.status == "success")
                {
                    var text = jsonResult?.text;
                    if (text != null)
                    {
                        return text.ToString();
                    }
                    else
                    {
                        throw new Exception("OCR returned success but no text content");
                    }
                }
                else if (jsonResult?.status == "error")
                {
                    var message = jsonResult?.message;
                    throw new Exception($"Python error: {message ?? "Unknown error"}");
                }
                else
                {
                    throw new Exception("Invalid JSON response");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OneOCR ERROR] JSON parse failed: {ex.Message}");
                Console.WriteLine($"[OneOCR ERROR] Raw output: {jsonOutput}");
                throw;
            }
        }

        /// <summary>
        /// Check if language pack is installed (OneOCR doesn't require language packs)
        /// </summary>
        public bool CheckLanguagePackInstall(string sourceLanguage)
        {
            // OneOCR uses Windows Snipping Tool OCR and doesn't require language packs
            // It works automatically with the system's installed languages
            _currentLanguageCode = sourceLanguage;
            return true;
        }
    }
}
