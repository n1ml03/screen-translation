using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ScreenTranslation
{
    public class OcrServerManager
    {
        private static OcrServerManager? _instance;
        private Process? _currentServerProcess;
        public bool serverStarted = false;

        public bool timeoutStartServer = false;
        
        // Singleton pattern
        public static OcrServerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OcrServerManager();
                }
                return _instance;
            }
        }
        
        private OcrServerManager()
        {
            // Private constructor for singleton
        }
        
        /// <summary>
        /// Start OCR server
        /// </summary>
        public async Task<bool> StartOcrServerAsync(string ocrMethod)
        {
            string flagFile = "";
            try
            {
                // Stop the current OCR server if it's running
                StopOcrServer();

                // Get the base directory of the application
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string webserverPath = Path.Combine(baseDirectory, "webserver");

                // Choose the appropriate batch file and working directory based on the OCR method
                string batchFileName;
                string workingDirectory;

                if (ocrMethod == "PaddleOCR")
                {
                    batchFileName = "RunServerPaddleOCR.bat";
                    workingDirectory = Path.Combine(webserverPath, "PaddleOCR");
                    flagFile = Path.Combine(Path.GetTempPath(), "paddleocr_ready.txt");
                    try
                    {
                        File.Delete(flagFile);
                        Console.WriteLine("Delete temp file success");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Delete temp file fail {e.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"OCR method not supported: {ocrMethod}");
                    return false;
                }

                // Check if batch file exists
                string batchFilePath = Path.Combine(workingDirectory, batchFileName);
                if (!File.Exists(batchFilePath))
                {
                    Console.WriteLine($"File not found: {batchFilePath}");
                    return false;
                }

                // Initialize process start info
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {batchFileName}",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };


                // Starting process
                _currentServerProcess = Process.Start(startInfo);
                timeoutStartServer = false;
                // Wait for flag file
                Console.WriteLine("⏳ Waiting for ready flag...");
                for (int i = 0; i < 90; i++) // 1 minute 30 seconds
                {
                    if (File.Exists(flagFile))
                    {
                        Console.WriteLine($"✅ {ocrMethod} READY!");
                        serverStarted = true;
                        break;
                    }

                    await Task.Delay(1000);

                    if (i % 1 == 0)
                        Console.WriteLine($"Still waiting... {i}s");
                }

                if (serverStarted == false)
                {
                    Console.WriteLine("Cannot start OCR server");
                    timeoutStartServer = true;
                    return false;
                }


                Console.WriteLine($"{ocrMethod} server has been started");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting OCR server: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Stop the OCR server if it's running
        /// </summary>
        public void StopOcrServer()
        {
            try
            {
                if (_currentServerProcess != null && !_currentServerProcess.HasExited)
                {
                    KillProcessesByPort(SocketManager.Instance.get_PaddleOcrPort());
                    MainWindow.Instance.UpdateServerButtonStatus(OcrServerManager.Instance.serverStarted);
                    // Get the process ID of the current server process
                    int processId = _currentServerProcess.Id;
                    // Try to close the process gracefully
                    _currentServerProcess.CloseMainWindow();

                    // Wait for the process to exit gracefully for a short period of time
                    if (!_currentServerProcess.WaitForExit(1000))
                    {
                        // Failed to close gracefully, so kill the process forcefully
                        _currentServerProcess.Kill();
                    }

                    _currentServerProcess = null;
                    Console.WriteLine("OCR server has been stopped");
                    serverStarted = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping OCR server: {ex.Message}");
            }
        }
        

        public void KillProcessesByPort(int port)
        {
            try
            {
                Console.WriteLine($"Looking for processes using port {port}...");
                

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c netstat -ano | findstr LISTENING | findstr :{port}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process? process = Process.Start(psi))
                {
                    if (process == null)
                    {
                        Console.WriteLine("Failed to start netstat command");
                        return;
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine($"No processes found using port {port}");
                        return;
                    }

                    // Find PIDs from netstat
                    foreach (string line in output.Split('\n'))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        

                        string[] parts = line.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 4)
                        {
                            if (int.TryParse(parts[parts.Length - 1], out int pid))
                            {
                                try
                                {
                                    Process processToKill = Process.GetProcessById(pid);
                                    Console.WriteLine($"Killing process {pid} using port {port}");
                                    processToKill.Kill();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Failed to kill process {pid}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error killing processes by port: {ex.Message}");
            }
        }


        

        /// <summary>
        /// Install Python dependencies for OCR using pip
        /// </summary>
        /// <param name="ocrMethod">OCR method ("PaddleOCR")</param>
        public bool SetupOcrEnvironment(string ocrMethod)
        {
            try
            {
                if (ocrMethod != "PaddleOCR")
                {
                    Console.WriteLine($"This OCR method is not supported: {ocrMethod}");
                    return false;
                }

                // Get the working directory
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string workingDirectory = Path.Combine(baseDirectory, "webserver", "PaddleOCR");

                // Check if requirements.txt exists
                string requirementsPath = Path.Combine(workingDirectory, "requirements.txt");
                if (!File.Exists(requirementsPath))
                {
                    Console.WriteLine($"Requirements file not found: {requirementsPath}");
                    return false;
                }

                // Initialize process start info for pip install
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"-m pip install -r requirements.txt",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                // Start the process
                using (Process? setupProcess = Process.Start(startInfo))
                {
                    if (setupProcess == null)
                    {
                        Console.WriteLine("Unable to start pip install process");
                        return false;
                    }

                    // Read output and error streams
                    string output = setupProcess.StandardOutput.ReadToEnd();
                    string error = setupProcess.StandardError.ReadToEnd();

                    // Wait for the process to finish
                    setupProcess.WaitForExit();

                    if (setupProcess.ExitCode == 0)
                    {
                        Console.WriteLine($"The {ocrMethod} dependencies installation has been completed successfully");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Pip install failed with exit code {setupProcess.ExitCode}");
                        Console.WriteLine($"Output: {output}");
                        Console.WriteLine($"Error: {error}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when installing OCR dependencies: {ex.Message}");
                return false;
            }
        }
    }
}