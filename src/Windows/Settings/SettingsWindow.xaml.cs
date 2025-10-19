using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using Button = System.Windows.Controls.Button;
using Brushes = System.Windows.Media.Brushes;
using Application = System.Windows.Application;

#pragma warning disable CA1416 // Validate platform compatibility

namespace ScreenTranslation
{
    // Class to represent an ignore phrase
    public class IgnorePhrase
    {
        public string Phrase { get; set; } = string.Empty;
        public bool ExactMatch { get; set; } = true;

        public IgnorePhrase(string phrase, bool exactMatch)
        {
            Phrase = phrase;
            ExactMatch = exactMatch;
        }
    }

    public partial class SettingsWindow : Window
    {
        private static SettingsWindow? _instance;

        public static bool _isLanguagePackInstall = false;


        public static SettingsWindow Instance
        {
            get
            {
                if (_instance == null || !IsWindowValid(_instance))
                {
                    _instance = new SettingsWindow();
                }
                return _instance;
            }
        }

        public SettingsWindow()
        {
            // Make sure the initialization flag is set before anything else
            _isInitializing = true;
            Console.WriteLine("SettingsWindow constructor: Setting _isInitializing to true");

            InitializeComponent();
            _instance = this;
            LoadAvailableScreens();
            LoadAvailableWindowTTSVoice();

            // Add Loaded event handler to ensure controls are initialized
            this.Loaded += SettingsWindow_Loaded;

            // Set up closing behavior (hide instead of close)
            this.Closing += (s, e) =>
            {
                e.Cancel = true;  // Cancel the close
                this.Hide();      // Just hide the window
                MainWindow.Instance.settingsButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(108, 117, 125));
            };
        }

        // Reload setting for setting windows
        public void ReloadSetting()
        {
            _instance = this;
            LoadAvailableScreens();
            LoadAvailableWindowTTSVoice();
            SettingsWindow_Loaded(null, null);
        }

        // Show message for multi selection are
        private bool isNeedShowMessage = false;

        // Flag to prevent saving during initialization
        private static bool _isInitializing = true;

        // Collection to hold the ignore phrases
        private ObservableCollection<IgnorePhrase> _ignorePhrases = new ObservableCollection<IgnorePhrase>();

        private void SettingsWindow_Loaded(object? sender, RoutedEventArgs? e)
        {
            try
            {
                Console.WriteLine("SettingsWindow_Loaded: Starting initialization");

                // Set initialization flag to prevent saving during setup
                _isInitializing = true;

                // Make sure keyboard shortcuts work from this window too
                PreviewKeyDown -= Application_KeyDown;
                PreviewKeyDown += Application_KeyDown;

                // Set initial values only after the window is fully loaded
                LoadSettingsFromMainWindow();

                // Make sure service-specific settings are properly initialized
                string currentService = ConfigManager.Instance.GetCurrentTranslationService();
                UpdateServiceSpecificSettings(currentService);

                // Make sure button check language package are properly initialize
                string currentOcr = ConfigManager.Instance.GetOcrMethod();
                if (currentOcr != "OneOCR")
                {
                    checkLanguagePack.Visibility = Visibility.Collapsed;
                    checkLanguagePackButton.Visibility = Visibility.Collapsed;
                }
                // Set default values
                hotKeyFunctionComboBox.SelectedIndex = 0;
                combineKey2.SelectedIndex = 0;
                combineKey1.SelectedIndex = 0;

                // Set selected screen from config
                int selectedScreenIndex = ConfigManager.Instance.GetSelectedScreenIndex();
                if (selectedScreenIndex >= 0 && selectedScreenIndex < screenComboBox.Items.Count)
                {
                    screenComboBox.SelectedIndex = selectedScreenIndex;
                }
                else if (screenComboBox.Items.Count > 0)
                {
                    // Default to first screen if saved index is invalid
                    screenComboBox.SelectedIndex = 0;
                }

                // Now that initialization is complete, allow saving changes
                _isInitializing = false;

                // Force the OCR method and translation service to match the config again
                // This ensures the config values are preserved and not overwritten
                string configOcrMethod = ConfigManager.Instance.GetOcrMethod();
                string configTransService = ConfigManager.Instance.GetCurrentTranslationService();
                Console.WriteLine($"Ensuring config values are preserved: OCR={configOcrMethod}, Translation={configTransService}");

                ConfigManager.Instance.SetOcrMethod(configOcrMethod);
                ConfigManager.Instance.SetTranslationService(configTransService);


                Console.WriteLine("Settings window fully loaded and initialized. Changes will now be saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Settings window: {ex.Message}");
                _isInitializing = false; // Ensure we don't get stuck in initialization mode
            }
        }
        // Load list of available window TTS voice
        private void LoadAvailableWindowTTSVoice()
        {
            try
            {
                // Clear any existing items
                windowTTSVoiceComboBox.Items.Clear();

                // Get all available voices from WindowsTTSService
                var availableVoices = WindowsTTSService.GetInstalledVoiceNames();

                if (availableVoices.Count == 0)
                {
                    // Add a placeholder item if no voices are available
                    ComboBoxItem noVoicesItem = new ComboBoxItem
                    {
                        Content = "No voices available",
                        IsEnabled = false
                    };
                    windowTTSVoiceComboBox.Items.Add(noVoicesItem);
                    Console.WriteLine("No Windows TTS voices found");
                }
                else
                {
                    // Group voices by language
                    var groupedVoices = new Dictionary<string, List<string>>();

                    foreach (string voiceName in availableVoices)
                    {
                        // Extract language information from voice
                        string languageCode = "Other";


                        int startIndex = voiceName.IndexOf('(');
                        if (startIndex > 0)
                        {
                            int endIndex = voiceName.IndexOf(',', startIndex);
                            if (endIndex > startIndex)
                            {
                                languageCode = voiceName.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
                            }
                        }

                        // Add to group
                        if (!groupedVoices.ContainsKey(languageCode))
                        {
                            groupedVoices[languageCode] = new List<string>();
                        }
                        groupedVoices[languageCode].Add(voiceName);
                    }

                    // Prefer show VN language
                    List<string> languagePriority = new List<string> { "vi-VN", "Vietnamese" };

                    foreach (string priorityLang in languagePriority)
                    {
                        if (groupedVoices.ContainsKey(priorityLang))
                        {
                            foreach (string voiceName in groupedVoices[priorityLang])
                            {
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = voiceName
                                };
                                windowTTSVoiceComboBox.Items.Add(item);
                            }
                            groupedVoices.Remove(priorityLang);
                        }
                    }

                    foreach (var group in groupedVoices)
                    {
                        foreach (string voiceName in group.Value)
                        {
                            ComboBoxItem item = new ComboBoxItem
                            {
                                Content = voiceName
                            };
                            windowTTSVoiceComboBox.Items.Add(item);
                        }
                    }

                    // Try to select the current voice from config
                    string currentVoice = ConfigManager.Instance.GetWindowsTtsVoice();
                    bool foundVoice = false;

                    // First try to find an exact match
                    foreach (ComboBoxItem item in windowTTSVoiceComboBox.Items)
                    {
                        if (string.Equals(item.Content?.ToString(), currentVoice, StringComparison.OrdinalIgnoreCase))
                        {
                            windowTTSVoiceComboBox.SelectedItem = item;
                            foundVoice = true;
                            Console.WriteLine($"Selected voice from config: {currentVoice}");
                            break;
                        }
                    }

                    // If the configured voice wasn't found, try to find a Vietnamese voice
                    if (!foundVoice)
                    {
                        foreach (ComboBoxItem item in windowTTSVoiceComboBox.Items)
                        {
                            string? itemContent = item.Content?.ToString();
                            if (itemContent != null &&
                                (itemContent.Contains("Vietnamese") ||
                                itemContent.Contains("vi-VN") ||
                                itemContent.Contains("An")))
                            {
                                windowTTSVoiceComboBox.SelectedItem = item;
                                foundVoice = true;
                                Console.WriteLine($"Selected Vietnamese voice: {itemContent}");
                                break;
                            }
                        }
                    }

                    // If still no voice selected, try to get the default system voice
                    if (!foundVoice)
                    {
                        string? defaultVoice = WindowsTTSService.GetDefaultSystemVoice();

                        if (!string.IsNullOrEmpty(defaultVoice))
                        {
                            foreach (ComboBoxItem item in windowTTSVoiceComboBox.Items)
                            {
                                if (string.Equals(item.Content?.ToString(), defaultVoice, StringComparison.OrdinalIgnoreCase))
                                {
                                    windowTTSVoiceComboBox.SelectedItem = item;
                                    foundVoice = true;
                                    Console.WriteLine($"Selected default system voice: {defaultVoice}");
                                    break;
                                }
                            }
                        }

                        // If still no voice selected, select the first one
                        if (!foundVoice && windowTTSVoiceComboBox.Items.Count > 0)
                        {
                            windowTTSVoiceComboBox.SelectedIndex = 0;
                            ComboBoxItem? firstItem = windowTTSVoiceComboBox.SelectedItem as ComboBoxItem;
                            Console.WriteLine($"Selected first available voice: {firstItem?.Content}");
                        }
                    }
                }

                Console.WriteLine($"Loaded {availableVoices.Count} Windows TTS voices");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Windows TTS voices: {ex.Message}");
                MessageBox.Show($"Error loading TTS voices: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load list of available screens
        private void LoadAvailableScreens()
        {
            try
            {
                // Clear existing items
                screenComboBox.Items.Clear();

                // Get all screens
                var screens = System.Windows.Forms.Screen.AllScreens;

                // Add each screen to the combo box
                for (int i = 0; i < screens.Length; i++)
                {
                    var screen = screens[i];

                    // Get native resolution
                    int width = screen.Bounds.Width;
                    int height = screen.Bounds.Height;

                    // Create display name
                    string displayName = $"{width} x {height}";
                    if (screen.Primary)
                    {
                        displayName += " (Primary)";
                    }

                    // Create combo box item
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = displayName,
                        Tag = i  // Store screen index as Tag
                    };

                    screenComboBox.Items.Add(item);
                }

                // Select the primary screen by default
                for (int i = 0; i < screenComboBox.Items.Count; i++)
                {
                    if (screenComboBox.Items[i] is ComboBoxItem item &&
                        item.Content.ToString()?.Contains("Primary") == true)
                    {
                        screenComboBox.SelectedIndex = i;
                        break;
                    }
                }

                // If no primary screen was found, select the first item
                if (screenComboBox.SelectedIndex == -1 && screenComboBox.Items.Count > 0)
                {
                    screenComboBox.SelectedIndex = 0;
                }

                Console.WriteLine($"Loaded {screenComboBox.Items.Count} screens");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading available screens: {ex.Message}");
            }
        }

        // Select screen - placeholder for now
        private void ScreenComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                if (screenComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    // Get the screen index from the Tag
                    if (selectedItem.Tag is int screenIndex)
                    {
                        // Save to config
                        ConfigManager.Instance.SetSelectedScreenIndex(screenIndex);
                        Console.WriteLine($"Selected screen index set to: {screenIndex}");

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling screen selection change: {ex.Message}");
            }
        }

        private void ApiKeyPasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (sender is PasswordBox passwordBox)
                {
                    string apiKey = passwordBox.Password.Trim();
                    string serviceType = ConfigManager.Instance.GetCurrentTranslationService();

                    if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(serviceType))
                    {
                        // Add Api key to list
                        ConfigManager.Instance.AddApiKey(serviceType, apiKey);

                        // Clear textbox content
                        passwordBox.Password = "";

                        Console.WriteLine($"Added new API key for {serviceType}");


                        MessageBox.Show($"API key added for {serviceType}.", "API Key Added",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void ViewApiKeysButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button)
            {
                string serviceType = ConfigManager.Instance.GetCurrentTranslationService();

                if (!string.IsNullOrEmpty(serviceType))
                {
                    // Get list api key
                    List<string> apiKeys = ConfigManager.Instance.GetApiKeysList(serviceType);

                    // Show API keys management window
                    ApiKeysWindow apiKeysWindow = new ApiKeysWindow(serviceType, apiKeys);
                    apiKeysWindow.Owner = this;
                    apiKeysWindow.ShowDialog();
                }
            }
        }
        // Handler for application-level keyboard shortcuts
        private void Application_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Forward to the central keyboard shortcuts handler
            KeyboardShortcuts.HandleKeyDown(e);
        }




        // Helper method to check if a window instance is still valid
        private static bool IsWindowValid(Window window)
        {
            // Check if the window still exists in the application's window collection
            var windowCollection = System.Windows.Application.Current.Windows;
            for (int i = 0; i < windowCollection.Count; i++)
            {
                if (windowCollection[i] == window)
                {
                    return true;
                }
            }
            return false;
        }

        private void LoadSettingsFromMainWindow()
        {
            // Temporarily remove event handlers to prevent triggering changes during initialization
            sourceLanguageComboBox.SelectionChanged -= SourceLanguageComboBox_SelectionChanged;
            targetLanguageComboBox.SelectionChanged -= TargetLanguageComboBox_SelectionChanged;

            // Remove focus event handlers
            maxContextPiecesTextBox.LostFocus -= MaxContextPiecesTextBox_LostFocus;
            minContextSizeTextBox.LostFocus -= MinContextSizeTextBox_LostFocus;
            minChatBoxTextSizeTextBox.LostFocus -= MinChatBoxTextSizeTextBox_LostFocus;
            gameInfoTextBox.TextChanged -= GameInfoTextBox_TextChanged;
            minTextFragmentSizeTextBox.LostFocus -= MinTextFragmentSizeTextBox_LostFocus;
            minLetterConfidenceTextBox.LostFocus -= MinLetterConfidenceTextBox_LostFocus;
            minLineConfidenceTextBox.LostFocus -= MinLineConfidenceTextBox_LostFocus;
            blockDetectionPowerTextBox.LostFocus -= BlockDetectionPowerTextBox_LostFocus;
            settleTimeTextBox.LostFocus -= SettleTimeTextBox_LostFocus;

            // Set context settings
            maxContextPiecesTextBox.Text = ConfigManager.Instance.GetMaxContextPieces().ToString();
            minContextSizeTextBox.Text = ConfigManager.Instance.GetMinContextSize().ToString();
            minChatBoxTextSizeTextBox.Text = ConfigManager.Instance.GetChatBoxMinTextSize().ToString();
            gameInfoTextBox.Text = ConfigManager.Instance.GetGameInfo();
            minTextFragmentSizeTextBox.Text = ConfigManager.Instance.GetMinTextFragmentSize().ToString();
            minLetterConfidenceTextBox.Text = ConfigManager.Instance.GetMinLetterConfidence().ToString();
            minLineConfidenceTextBox.Text = ConfigManager.Instance.GetMinLineConfidence().ToString();

            // Reattach focus event handlers
            maxContextPiecesTextBox.LostFocus += MaxContextPiecesTextBox_LostFocus;
            minContextSizeTextBox.LostFocus += MinContextSizeTextBox_LostFocus;
            minChatBoxTextSizeTextBox.LostFocus += MinChatBoxTextSizeTextBox_LostFocus;
            gameInfoTextBox.TextChanged += GameInfoTextBox_TextChanged;
            minTextFragmentSizeTextBox.LostFocus += MinTextFragmentSizeTextBox_LostFocus;
            minLetterConfidenceTextBox.LostFocus += MinLetterConfidenceTextBox_LostFocus;
            minLineConfidenceTextBox.LostFocus += MinLineConfidenceTextBox_LostFocus;

            textSimilarThresholdTextBox.LostFocus += TextSimilarThresholdTextBox_LostFocus;
            // Load source language either from config or MainWindow as fallback
            string configSourceLanguage = ConfigManager.Instance.GetSourceLanguage();
            if (!string.IsNullOrEmpty(configSourceLanguage))
            {
                // First try to load from config
                foreach (ComboBoxItem item in sourceLanguageComboBox.Items)
                {
                    if (string.Equals(item.Content.ToString(), configSourceLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        sourceLanguageComboBox.SelectedItem = item;
                        Console.WriteLine($"Settings window: Set source language from config to {configSourceLanguage}");
                        break;
                    }
                }
            }
            else if (MainWindow.Instance.sourceLanguageComboBox != null &&
                     MainWindow.Instance.sourceLanguageComboBox.SelectedIndex >= 0)
            {
                // Fallback to MainWindow if config doesn't have a value
                sourceLanguageComboBox.SelectedIndex = MainWindow.Instance.sourceLanguageComboBox.SelectedIndex;
            }
            ListHotKey_TextChanged();

            // Load target language either from config or MainWindow as fallback
            string configTargetLanguage = ConfigManager.Instance.GetTargetLanguage();
            if (!string.IsNullOrEmpty(configTargetLanguage))
            {
                // First try to load from config
                foreach (ComboBoxItem item in targetLanguageComboBox.Items)
                {
                    if (string.Equals(item.Content.ToString(), configTargetLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        targetLanguageComboBox.SelectedItem = item;
                        Console.WriteLine($"Settings window: Set target language from config to {configTargetLanguage}");
                        break;
                    }
                }
            }
            else if (MainWindow.Instance.targetLanguageComboBox != null &&
                     MainWindow.Instance.targetLanguageComboBox.SelectedIndex >= 0)
            {
                // Fallback to MainWindow if config doesn't have a value
                targetLanguageComboBox.SelectedIndex = MainWindow.Instance.targetLanguageComboBox.SelectedIndex;
            }

            // Reattach event handlers
            sourceLanguageComboBox.SelectionChanged += SourceLanguageComboBox_SelectionChanged;
            targetLanguageComboBox.SelectionChanged += TargetLanguageComboBox_SelectionChanged;

            // Set text similar threshold from config
            textSimilarThresholdTextBox.Text = Convert.ToString(ConfigManager.Instance.GetTextSimilarThreshold());

            // Set char level from config
            charLevelCheckBox.IsChecked = ConfigManager.Instance.IsCharLevelEnabled();

            // Set show icon signal
            showIconSignalCheckBox.IsChecked = ConfigManager.Instance.IsShowIconSignalEnabled();

            // Set auto OCR
            AutoOCRCheckBox.IsChecked = ConfigManager.Instance.IsAutoOCREnabled();

            // Set OneOCR integration
            oneOCRIntegrationCheckBox.IsChecked = ConfigManager.Instance.IsOneOCRIntegrationEnabled();

            // Set multi selection area from config
            multiSelectionAreaCheckBox.IsChecked = ConfigManager.Instance.IsMultiSelectionAreaEnabled();
            if (!ConfigManager.Instance.IsMultiSelectionAreaEnabled())
            {
                isNeedShowMessage = true;
            }

            // Set OCR settings from config
            string savedOcrMethod = ConfigManager.Instance.GetOcrMethod();
            Console.WriteLine($"SettingsWindow: Loading OCR method '{savedOcrMethod}'");

            // Temporarily remove event handler to prevent triggering during initialization
            ocrMethodComboBox.SelectionChanged -= OcrMethodComboBox_SelectionChanged;

            // Find matching ComboBoxItem
            foreach (ComboBoxItem item in ocrMethodComboBox.Items)
            {
                string itemText = item.Content.ToString() ?? "";
                if (string.Equals(itemText, savedOcrMethod, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Found matching OCR method: '{itemText}'");
                    ocrMethodComboBox.SelectedItem = item;
                    break;
                }
            }

            // Re-attach event handler
            ocrMethodComboBox.SelectionChanged += OcrMethodComboBox_SelectionChanged;

            // Get auto-translate setting from config instead of MainWindow
            // This ensures the setting persists across application restarts
            autoTranslateCheckBox.IsChecked = ConfigManager.Instance.IsAutoTranslateEnabled();
            Console.WriteLine($"Settings window: Loading auto-translate from config: {ConfigManager.Instance.IsAutoTranslateEnabled()}");

            // Set leave translation onscreen setting
            leaveTranslationOnscreenCheckBox.IsChecked = ConfigManager.Instance.IsLeaveTranslationOnscreenEnabled();

            // Set block detection settings directly from BlockDetectionManager
            // Temporarily remove event handlers to prevent triggering changes
            blockDetectionPowerTextBox.LostFocus -= BlockDetectionPowerTextBox_LostFocus;
            settleTimeTextBox.LostFocus -= SettleTimeTextBox_LostFocus;


            blockDetectionPowerTextBox.Text = BlockDetectionManager.Instance.GetBlockDetectionScale().ToString("F2");
            settleTimeTextBox.Text = ConfigManager.Instance.GetBlockDetectionSettleTime().ToString("F2");

            Console.WriteLine($"SettingsWindow: Loaded block detection power: {blockDetectionPowerTextBox.Text}");
            Console.WriteLine($"SettingsWindow: Loaded settle time: {settleTimeTextBox.Text}");

            // Reattach event handlers
            blockDetectionPowerTextBox.LostFocus += BlockDetectionPowerTextBox_LostFocus;
            settleTimeTextBox.LostFocus += SettleTimeTextBox_LostFocus;

            // Set translation service from config
            string currentService = ConfigManager.Instance.GetCurrentTranslationService();

            // Temporarily remove event handler
            translationServiceComboBox.SelectionChanged -= TranslationServiceComboBox_SelectionChanged;

            foreach (ComboBoxItem item in translationServiceComboBox.Items)
            {
                if (string.Equals(item.Content.ToString(), currentService, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Found matching translation service: '{item.Content}'");
                    translationServiceComboBox.SelectedItem = item;
                    break;
                }
            }

            // Re-attach event handler
            translationServiceComboBox.SelectionChanged += TranslationServiceComboBox_SelectionChanged;

            // Update service-specific settings visibility based on selected service
            UpdateServiceSpecificSettings(currentService);

            // Load the current service's prompt
            LoadCurrentServicePrompt();

            // Load TTS settings

            // Temporarily remove TTS event handlers
            ttsEnabledCheckBox.Checked -= TtsEnabledCheckBox_CheckedChanged;
            ttsEnabledCheckBox.Unchecked -= TtsEnabledCheckBox_CheckedChanged;
            ttsServiceComboBox.SelectionChanged -= TtsServiceComboBox_SelectionChanged;
            windowTTSVoiceComboBox.SelectionChanged -= WindowTTSVoiceComboBox_SelectionChanged;

            // Set TTS enabled state
            ttsEnabledCheckBox.IsChecked = ConfigManager.Instance.IsTtsEnabled();

            // Set Exclude character name
            excludeCharacterNameCheckBox.IsChecked = ConfigManager.Instance.IsExcludeCharacterNameEnabled();

            // Set TTS service
            string ttsService = ConfigManager.Instance.GetTtsService();
            foreach (ComboBoxItem item in ttsServiceComboBox.Items)
            {
                if (string.Equals(item.Content.ToString(), ttsService, StringComparison.OrdinalIgnoreCase))
                {
                    ttsServiceComboBox.SelectedItem = item;
                    break;
                }
            }

            // Update service-specific settings visibility
            UpdateTtsServiceSpecificSettings(ttsService);

            // Set Window TTS voice
            string windowTTSvoiceId = ConfigManager.Instance.GetWindowsTtsVoice();
            foreach (ComboBoxItem item in windowTTSVoiceComboBox.Items)
            {
                if (string.Equals(item.Tag?.ToString(), windowTTSvoiceId, StringComparison.OrdinalIgnoreCase))
                {
                    windowTTSVoiceComboBox.SelectedItem = item;
                    break;
                }
            }

            // Re-attach TTS event handlers
            ttsEnabledCheckBox.Checked += TtsEnabledCheckBox_CheckedChanged;
            ttsEnabledCheckBox.Unchecked += TtsEnabledCheckBox_CheckedChanged;
            ttsServiceComboBox.SelectionChanged += TtsServiceComboBox_SelectionChanged;
            windowTTSVoiceComboBox.SelectionChanged += WindowTTSVoiceComboBox_SelectionChanged;

            // Load ignore phrases
            LoadIgnorePhrases();

            // Audio Processing settings
            audioProcessingProviderComboBox.SelectedIndex = 0; // Only one for now
            openAiRealtimeApiKeyPasswordBox.Password = ConfigManager.Instance.GetOpenAiRealtimeApiKey();
            // Load Auto-translate for audio service
            audioServiceAutoTranslateCheckBox.IsChecked = ConfigManager.Instance.IsAudioServiceAutoTranslateEnabled();
        }


        private void SetHotKeyButton_Click(object sender, RoutedEventArgs e)
        {
            // Skip event if we're initializing
            if (_isInitializing)
            {
                return;
            }

            string functionName;
            string? key1 = "";
            string? key2 = "";
            string combineKey;

            // Check if selected item is a ComboBoxItem
            if (hotKeyFunctionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                functionName = selectedItem.Content.ToString() ?? "Start/Stop";
                if (combineKey1.SelectedItem is ComboBoxItem selectedItem1)
                {
                    key1 = selectedItem1.Content.ToString();
                }
                if (combineKey2.SelectedItem is ComboBoxItem selectedItem2)
                {
                    key2 = selectedItem2.Content.ToString();
                }
                if (key1 == "" || key2 == "" || key1 == "----------- Select -----------" || key2 == "----------- Select -----------")
                {
                    MessageBox.Show("Hot key is not valid, please try again", "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                }
                else
                {
                    combineKey = key1 + "+" + key2;
                    // Save HotKey
                    ConfigManager.Instance.SetHotKey(functionName, combineKey);
                    statusUpdateHotKey.Visibility = Visibility.Visible;
                    ListHotKey_TextChanged();
                    // Init keyboard hook
                    KeyboardShortcuts.InitializeGlobalHook();
                    IntPtr handle = new WindowInteropHelper(this).Handle;
                    KeyboardShortcuts.SetMainWindowHandle(handle);
                    HwndSource source = HwndSource.FromHwnd(handle);
                    source.AddHook(WndProc);
                    // Auto close notification after 1.5 second
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(1.3)
                    };

                    timer.Tick += (s, e) =>
                    {
                        statusUpdateHotKey.Visibility = Visibility.Collapsed;
                        timer.Stop();
                    };

                    timer.Start();

                }
            }

        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312) // WM_HOTKEY
            {
                handled = KeyboardShortcuts.ProcessHotKey(wParam);
            }

            return IntPtr.Zero;
        }

        private void ListHotKey_TextChanged()
        {
            // Setting windows
            hotKeyStartStop.Text = "Start/Stop: " + ConfigManager.Instance.GetHotKey("Start/Stop");
            hotKeyOverlay.Text = "Overlay: " + ConfigManager.Instance.GetHotKey("Overlay");
            hotKeySetting.Text = "Setting: " + ConfigManager.Instance.GetHotKey("Setting");
            hotKeyLog.Text = "Log: " + ConfigManager.Instance.GetHotKey("Log");
            hotKeySelectArea.Text = "Select Area: " + ConfigManager.Instance.GetHotKey("Select Area");
            hotKeyClearAreas.Text = "Clear Areas: " + ConfigManager.Instance.GetHotKey("Clear Areas");
            hotKeyClearPreviousArea.Text = "Clear Selected Area: " + ConfigManager.Instance.GetHotKey("Clear Selected Area");
            hotKeyShowArea.Text = "Show Area: " + ConfigManager.Instance.GetHotKey("Show Area");
            hotKeyChatBox.Text = "ChatBox: " + ConfigManager.Instance.GetHotKey("ChatBox");
            hotKeyArea1.Text = "Area 1: " + ConfigManager.Instance.GetHotKey("Area 1");
            hotKeyArea2.Text = "Area 2: " + ConfigManager.Instance.GetHotKey("Area 2");
            hotKeyArea3.Text = "Area 3: " + ConfigManager.Instance.GetHotKey("Area 3");
            hotKeyArea4.Text = "Area 4: " + ConfigManager.Instance.GetHotKey("Area 4");
            hotKeyArea5.Text = "Area 5: " + ConfigManager.Instance.GetHotKey("Area 5");
            // Mainwindows
            MainWindow.Instance.hotKeyStartStop.Text = "Start/Stop: " + ConfigManager.Instance.GetHotKey("Start/Stop");
            MainWindow.Instance.hotKeyOverlay.Text = "Overlay: " + ConfigManager.Instance.GetHotKey("Overlay");
            MainWindow.Instance.hotKeySetting.Text = "Setting: " + ConfigManager.Instance.GetHotKey("Setting");
            MainWindow.Instance.hotKeyLog.Text = "Log: " + ConfigManager.Instance.GetHotKey("Log");
            MainWindow.Instance.hotKeySelectArea.Text = "Select Area: " + ConfigManager.Instance.GetHotKey("Select Area");
            MainWindow.Instance.hotKeyClearPreviousArea.Text = "Clear Selected Area: " + ConfigManager.Instance.GetHotKey("Clear Selected Area");
            MainWindow.Instance.hotKeyClearAreas.Text = "Clear Areas: " + ConfigManager.Instance.GetHotKey("Clear Areas");
            MainWindow.Instance.hotKeyShowArea.Text = "Show Area: " + ConfigManager.Instance.GetHotKey("Show Area");
            MainWindow.Instance.hotKeyChatBox.Text = "ChatBox: " + ConfigManager.Instance.GetHotKey("ChatBox");
            MainWindow.Instance.hotKeyArea1.Text = "Area 1: " + ConfigManager.Instance.GetHotKey("Area 1");
            MainWindow.Instance.hotKeyArea2.Text = "Area 2: " + ConfigManager.Instance.GetHotKey("Area 2");
            MainWindow.Instance.hotKeyArea3.Text = "Area 3: " + ConfigManager.Instance.GetHotKey("Area 3");
            MainWindow.Instance.hotKeyArea4.Text = "Area 4: " + ConfigManager.Instance.GetHotKey("Area 4");
            MainWindow.Instance.hotKeyArea5.Text = "Area 5: " + ConfigManager.Instance.GetHotKey("Area 5");
        }

        private void HotKeyFunctionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Skip event if we're initializing
            if (_isInitializing)
            {
                return;
            }

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string functionName = selectedItem.Content.ToString() ?? "Start/Stop";

                // Get HotKey from config
                string hotKey = ConfigManager.Instance.GetHotKey(functionName);
                string[] keyParts = hotKey.Split('+');

                if (keyParts.Length >= 2)
                {
                    string key1 = keyParts[0].ToUpper();
                    string key2 = keyParts[1].ToUpper();


                    foreach (ComboBoxItem item in combineKey1.Items)
                    {
                        if (item.Content.ToString() == key1)
                        {
                            combineKey1.SelectedItem = item;
                            break;
                        }
                    }


                    foreach (ComboBoxItem item in combineKey2.Items)
                    {
                        if (item.Content.ToString() == key2)
                        {
                            combineKey2.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        // Language settings
        private void SourceLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Skip event if we're initializing
            if (_isInitializing)
            {
                return;
            }

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string language = selectedItem.Content.ToString() ?? "ja";
                Console.WriteLine($"Settings: Source language changed to: {language}");

                // Save to config
                ConfigManager.Instance.SetSourceLanguage(language);

                // Update MainWindow source language
                if (MainWindow.Instance.sourceLanguageComboBox != null)
                {
                    // Find and select matching ComboBoxItem by content
                    foreach (ComboBoxItem item in MainWindow.Instance.sourceLanguageComboBox.Items)
                    {
                        if (string.Equals(item.Content.ToString(), language, StringComparison.OrdinalIgnoreCase))
                        {
                            MainWindow.Instance.sourceLanguageComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Reset the OCR hash to force a fresh comparison after changing source language
                Logic.Instance.ClearAllTextObjects();
            }
        }

        private void TargetLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Skip event if we're initializing
            if (_isInitializing)
            {
                return;
            }

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string language = selectedItem.Content.ToString() ?? "en";
                Console.WriteLine($"Settings: Target language changed to: {language}");

                // Save to config
                ConfigManager.Instance.SetTargetLanguage(language);

                // Update MainWindow target language
                if (MainWindow.Instance.targetLanguageComboBox != null)
                {
                    // Find and select matching ComboBoxItem by content
                    foreach (ComboBoxItem item in MainWindow.Instance.targetLanguageComboBox.Items)
                    {
                        if (string.Equals(item.Content.ToString(), language, StringComparison.OrdinalIgnoreCase))
                        {
                            MainWindow.Instance.targetLanguageComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Reset the OCR hash to force a fresh comparison after changing target language
                Logic.Instance.ClearAllTextObjects();
            }
        }

        private void OcrMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Skip event if we're initializing
            if (_isInitializing)
            {
                Console.WriteLine("Skipping OCR method change during initialization");
                return;
            }

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string? ocrMethod = selectedItem.Content?.ToString();

                if (!string.IsNullOrEmpty(ocrMethod))
                {
                    Console.WriteLine($"Setting OCR method to: {ocrMethod}");

                    // Save to config
                    ConfigManager.Instance.SetOcrMethod(ocrMethod);

                    // Update UI
                    MainWindow.Instance.SetOcrMethod(ocrMethod);
                    UpdateMonitorWindowOcrMethod(ocrMethod);
                    SocketManager.Instance.Disconnect();

                    // await SocketManager.Instance.SwitchOcrMethod(ocrMethod);
                    if (ocrMethod != "OneOCR")
                    {
                        checkLanguagePack.Visibility = Visibility.Collapsed;
                        checkLanguagePackButton.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        checkLanguagePack.Visibility = Visibility.Visible;
                        checkLanguagePackButton.Visibility = Visibility.Visible;
                    }


                }
            }
        }

        private void UpdateMonitorWindowOcrMethod(string ocrMethod)
        {
            // Update MonitorWindow OCR method selection
            if (MonitorWindow.Instance.ocrMethodComboBox != null)
            {
                foreach (ComboBoxItem item in MonitorWindow.Instance.ocrMethodComboBox.Items)
                {
                    if (string.Equals(item.Content.ToString(), ocrMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        MonitorWindow.Instance.ocrMethodComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void AutoTranslateCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Skip if initializing to prevent overriding values from config
            if (_isInitializing)
            {
                return;
            }

            bool isEnabled = autoTranslateCheckBox.IsChecked ?? false;
            Console.WriteLine($"Settings window: Auto-translate changed to {isEnabled}");

            // Update auto translate setting in MainWindow
            // This will also save to config and update the UI
            MainWindow.Instance.SetAutoTranslateEnabled(isEnabled);

            // Update MonitorWindow CheckBox if needed
            if (MonitorWindow.Instance.autoTranslateCheckBox != null)
            {
                MonitorWindow.Instance.autoTranslateCheckBox.IsChecked = autoTranslateCheckBox.IsChecked;
            }
        }

        private void CharLevelCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Skip if initializing to prevent overriding values from config
            if (_isInitializing)
            {
                return;
            }
            bool isEnabled = charLevelCheckBox.IsChecked ?? false;
            ConfigManager.Instance.SetCharLevelEnabled(isEnabled);
            // Clear text objects
            Logic.Instance.ClearAllTextObjects();
            Logic.Instance.ResetHash();
            // Force OCR to run again
            MainWindow.Instance.SetOCRCheckIsWanted(true);
            Console.WriteLine($"Settings window: Character level mode changed to {isEnabled}");
        }

        private void LeaveTranslationOnscreenCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Skip if initializing
            if (_isInitializing)
                return;

            bool isEnabled = leaveTranslationOnscreenCheckBox.IsChecked ?? false;
            ConfigManager.Instance.SetLeaveTranslationOnscreenEnabled(isEnabled);
            Console.WriteLine($"Leave translation onscreen enabled: {isEnabled}");
        }

        // Language swap button handler
        private void SwapLanguagesButton_Click(object sender, RoutedEventArgs e)
        {
            // Store the current selections
            int sourceIndex = sourceLanguageComboBox.SelectedIndex;
            int targetIndex = targetLanguageComboBox.SelectedIndex;

            // Swap the selections
            sourceLanguageComboBox.SelectedIndex = targetIndex;
            targetLanguageComboBox.SelectedIndex = sourceIndex;

            // The SelectionChanged events will handle updating the MainWindow
            Console.WriteLine($"Languages swapped: {GetLanguageCode(sourceLanguageComboBox)} ⇄ {GetLanguageCode(targetLanguageComboBox)}");
        }

        // Helper method to get language code from ComboBox
        private string GetLanguageCode(ComboBox comboBox)
        {
            try
            {
                if (comboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    return selectedItem.Content?.ToString() ?? "";
                }
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting language code: {ex.Message}");
                return "";
            }
        }

        // Block detection settings
        private void BlockDetectionPowerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Skip if initializing to prevent overriding values from config
            if (_isInitializing)
            {
                return;
            }

            // Update block detection power in MonitorWindow
            if (MonitorWindow.Instance.blockDetectionPowerTextBox != null)
            {
                MonitorWindow.Instance.blockDetectionPowerTextBox.Text = blockDetectionPowerTextBox.Text;
            }

            // Update BlockDetectionManager if applicable
            if (float.TryParse(blockDetectionPowerTextBox.Text, out float power))
            {
                // Note: SetBlockDetectionScale will save to config
                BlockDetectionManager.Instance.SetBlockDetectionScale(power);
                // Reset hash to force recalculation of text blocks
                Logic.Instance.ResetHash();
            }
            else
            {
                // If text is invalid, reset to the current value from BlockDetectionManager
                blockDetectionPowerTextBox.Text = BlockDetectionManager.Instance.GetBlockDetectionScale().ToString("F2");
            }
        }

        private void SettleTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Skip if initializing to prevent overriding values from config
            if (_isInitializing)
            {
                return;
            }

            // Update settle time in ConfigManager
            if (float.TryParse(settleTimeTextBox.Text, out float settleTime) && settleTime >= 0)
            {
                ConfigManager.Instance.SetBlockDetectionSettleTime(settleTime);
                Console.WriteLine($"Block detection settle time set to: {settleTime:F2} seconds");

                // Reset hash to force recalculation of text blocks
                Logic.Instance.ResetHash();
            }
            else
            {
                // If text is invalid, reset to the current value from ConfigManager
                settleTimeTextBox.Text = ConfigManager.Instance.GetBlockDetectionSettleTime().ToString("F2");
            }
        }

        // Translation service changed
        private void TranslationServiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Skip event if we're initializing
            Console.WriteLine($"SettingsWindow.TranslationServiceComboBox_SelectionChanged called (isInitializing: {_isInitializing})");
            if (_isInitializing)
            {
                Console.WriteLine("Skipping translation service change during initialization");
                return;
            }

            try
            {
                if (translationServiceComboBox == null)
                {
                    Console.WriteLine("Translation service combo box not initialized yet");
                    return;
                }

                if (translationServiceComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string selectedService = selectedItem.Content.ToString() ?? "ChatGPT";

                    Console.WriteLine($"SettingsWindow translation service changed to: '{selectedService}'");

                    // Save the selected service to config
                    ConfigManager.Instance.SetTranslationService(selectedService);

                    // Update service-specific settings visibility
                    UpdateServiceSpecificSettings(selectedService);

                    // Load the prompt for the selected service
                    LoadCurrentServicePrompt();

                    // Only trigger retranslation if not initializing (i.e., user changed it manually)
                    if (!_isInitializing)
                    {
                        Console.WriteLine("Translation service changed. Triggering retranslation...");

                        // Reset the hash to force a retranslation
                        Logic.Instance.ResetHash();

                        // Clear any existing text objects to refresh the display
                        Logic.Instance.ClearAllTextObjects();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling translation service change: {ex.Message}");
            }
        }

        // Load prompt for the currently selected translation service
        private void LoadCurrentServicePrompt()
        {
            try
            {
                if (translationServiceComboBox == null || promptTemplateTextBox == null)
                {
                    Console.WriteLine("Translation service controls not initialized yet. Skipping prompt loading.");
                    return;
                }

                if (translationServiceComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string selectedService = selectedItem.Content.ToString() ?? "ChatGPT";
                    string prompt = ConfigManager.Instance.GetServicePrompt(selectedService);

                    // Update the text box
                    promptTemplateTextBox.Text = prompt;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading prompt template: {ex.Message}");
            }
        }

        // Save prompt button clicked
        private void SavePromptButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentPrompt();
        }

        // Text box lost focus - save prompt
        private void PromptTemplateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveCurrentPrompt();
        }

        // Save the current prompt to the selected service
        private void SaveCurrentPrompt()
        {
            if (translationServiceComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedService = selectedItem.Content.ToString() ?? "ChatGPT";
                string prompt = promptTemplateTextBox.Text;

                if (!string.IsNullOrWhiteSpace(prompt))
                {
                    // Save to config
                    bool success = ConfigManager.Instance.SaveServicePrompt(selectedService, prompt);

                    if (success)
                    {
                        Console.WriteLine($"Prompt saved for {selectedService}");
                    }
                }
            }
        }

        private void RestoreDefaultPromptButton_Click(object sender, RoutedEventArgs e)
        {
            // Restore the default prompt for the selected service
            if (translationServiceComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedService = selectedItem.Content.ToString() ?? "ChatGPT";
                string defaultPrompt = ConfigManager.Instance.GetDefaultServicePrompt(selectedService);

                if (!string.IsNullOrWhiteSpace(defaultPrompt))
                {
                    promptTemplateTextBox.Text = defaultPrompt;
                }
            }
        }

        // Update service-specific settings visibility
        private void UpdateServiceSpecificSettings(string selectedService)
        {
            try
            {
                bool isChatGptSelected = selectedService == "ChatGPT";

                // Make sure the window is fully loaded and controls are initialized
                if (chatGptUsernameLabel == null || chatGptUsernameTextBox == null ||
                    chatGptEndpointLabel == null || chatGptEndpointTextBox == null ||
                    chatGptPasswordLabel == null || chatGptPasswordBox == null ||
                    chatGptModelLabel == null || chatGptModelGrid == null)
                {
                    Console.WriteLine("UI elements not initialized yet. Skipping visibility update.");
                    return;
                }

                // Show/hide ChatGPT-specific settings
                chatGptUsernameLabel.Visibility = isChatGptSelected ? Visibility.Visible : Visibility.Collapsed;
                chatGptUsernameTextBox.Visibility = isChatGptSelected ? Visibility.Visible : Visibility.Collapsed;
                chatGptEndpointLabel.Visibility = isChatGptSelected ? Visibility.Visible : Visibility.Collapsed;
                chatGptEndpointTextBox.Visibility = isChatGptSelected ? Visibility.Visible : Visibility.Collapsed;
                chatGptPasswordLabel.Visibility = isChatGptSelected ? Visibility.Visible : Visibility.Collapsed;
                chatGptPasswordBox.Visibility = isChatGptSelected ? Visibility.Visible : Visibility.Collapsed;
                chatGptModelLabel.Visibility = isChatGptSelected ? Visibility.Visible : Visibility.Collapsed;
                chatGptModelGrid.Visibility = isChatGptSelected ? Visibility.Visible : Visibility.Collapsed;

                // Always show prompt template for ChatGPT
                promptLabel.Visibility = Visibility.Visible;
                promptTemplateTextBox.Visibility = Visibility.Visible;
                savePromptButton.Visibility = Visibility.Visible;
                restoreDefaultPromptButton.Visibility = Visibility.Visible;

                // Load service-specific settings if they're being shown
                if (isChatGptSelected)
                {
                    chatGptUsernameTextBox.Text = ConfigManager.Instance.GetChatGptUsername();
                    chatGptEndpointTextBox.Text = ConfigManager.Instance.GetChatGptEndpoint();
                    chatGptPasswordBox.Password = ConfigManager.Instance.GetChatGptPassword();

                    // Set selected model
                    string model = ConfigManager.Instance.GetChatGptModel();
                    foreach (ComboBoxItem item in chatGptModelComboBox.Items)
                    {
                        if (string.Equals(item.Tag?.ToString(), model, StringComparison.OrdinalIgnoreCase))
                        {
                            chatGptModelComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating service-specific settings: {ex.Message}");
            }
        }

        private void AdjustOverlayConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create options window
                var optionsWindow = new OverlayOptionsWindow();

                // Set the owner to ensure it appears on top of this window
                optionsWindow.Owner = this;

                // Make this window appear in front
                this.Topmost = false;
                this.Topmost = true;

                // Show the dialog
                var result = optionsWindow.ShowDialog();

                // If user clicked OK, styling will already be applied by the options window
                if (result == true)
                {
                    Console.WriteLine("Chat box options updated");

                    // Create and start the flash animation for visual feedback
                    CreateFlashAnimation(overlayConfig);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing options dialog: {ex.Message}");
            }
        }

        private void CreateFlashAnimation(System.Windows.Controls.Button button)
        {
            try
            {
                // Get the current background brush
                SolidColorBrush? currentBrush = button.Background as SolidColorBrush;

                if (currentBrush != null)
                {
                    // Need to freeze the original brush to animate its clone
                    currentBrush = currentBrush.Clone();
                    System.Windows.Media.Color originalColor = currentBrush.Color;

                    // Create a new brush for animation
                    SolidColorBrush animBrush = new SolidColorBrush(originalColor);
                    button.Background = animBrush;

                    // Create color animation for the brush's Color property
                    var animation = new ColorAnimation
                    {
                        From = originalColor,
                        To = Colors.LightGreen,
                        Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                        AutoReverse = true,
                        FillBehavior = FillBehavior.Stop // Stop the animation when complete
                    };

                    // Apply the animation to the brush's Color property
                    animBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating flash animation: {ex.Message}");
            }
        }

        private void UpdateTtsServiceSpecificSettings(string selectedService)
        {
            try
            {
                // Make sure the window is fully loaded and controls are initialized
                if (windowTTSVoiceLabel == null || windowTTSVoiceComboBox == null)
                {
                    Console.WriteLine("TTS UI elements not initialized yet. Skipping visibility update.");
                    return;
                }

                // Show Windows TTS-specific settings (always visible since it's the only option)
                windowTTSVoiceLabel.Visibility = Visibility.Visible;
                windowTTSVoiceComboBox.Visibility = Visibility.Visible;
                windowsTTSGuide.Visibility = Visibility.Visible;

                // Set selected voice
                string voiceId = ConfigManager.Instance.GetWindowsTtsVoice();
                foreach (ComboBoxItem item in windowTTSVoiceComboBox.Items)
                {
                    if (string.Equals(item.Content?.ToString(), voiceId, StringComparison.OrdinalIgnoreCase))
                    {
                        windowTTSVoiceComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating TTS service-specific settings: {ex.Message}");
            }
        }


        private void ChatGptApiLink_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://platform.openai.com/api-keys");
        }

        private void ViewChatGptModelsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://platform.openai.com/docs/models");
        }

        // ChatGPT API Key changed
        private void ChatGptApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                // string apiKey = chatGptApiKeyPasswordBox.Password.Trim();

                // // Update the config
                // ConfigManager.Instance.SetChatGptApiKey(apiKey);
                Console.WriteLine("ChatGPT API key updated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating ChatGPT API key: {ex.Message}");
            }
        }

        // ChatGPT Model changed
        private void ChatGptModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                if (chatGptModelComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string model = selectedItem.Tag?.ToString() ?? "41-nano-ktv";

                    // Save to config
                    ConfigManager.Instance.SetChatGptModel(model);
                    Console.WriteLine($"ChatGPT model set to: {model}");

                    // Trigger retranslation if the current service is ChatGPT
                    if (ConfigManager.Instance.GetCurrentTranslationService() == "ChatGPT")
                    {
                        Console.WriteLine("ChatGPT model changed. Triggering retranslation...");

                        // Reset the hash to force a retranslation
                        Logic.Instance.ResetHash();

                        // Clear any existing text objects to refresh the display
                        Logic.Instance.ClearAllTextObjects();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating ChatGPT model: {ex.Message}");
            }
        }

        private void NaturalVoiceSAPIAdapterLink_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/gexgd0419/NaturalVoiceSAPIAdapter");
        }

        private void OpenUrl(string url)
        {
            try
            {
                var processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening URL: {ex.Message}");
                MessageBox.Show($"Unable to open URL: {url}\n\nError: {ex.Message}",
                    "Error Opening URL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Text-to-Speech settings handlers

        private void TtsEnabledCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                bool isEnabled = ttsEnabledCheckBox.IsChecked ?? false;
                ConfigManager.Instance.SetTtsEnabled(isEnabled);
                Console.WriteLine($"TTS enabled: {isEnabled}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating TTS enabled state: {ex.Message}");
            }
        }

        private void TtsServiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                if (ttsServiceComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string service = selectedItem.Content.ToString() ?? "Windows TTS";
                    ConfigManager.Instance.SetTtsService(service);
                    Console.WriteLine($"TTS service set to: {service}");

                    // Update UI for the selected service
                    UpdateTtsServiceSpecificSettings(service);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating TTS service: {ex.Message}");
            }
        }




        private void WindowTTSVoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                if (windowTTSVoiceComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string voiceId = selectedItem.Content?.ToString() ?? "Microsoft David (en-US, Male)";
                    ConfigManager.Instance.SetWindowsTtsVoice(voiceId);
                    Console.WriteLine($"Windows TTS voice set to: {selectedItem.Content} (ID: {voiceId})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Windows TTS voice: {ex.Message}");
            }
        }

        // Context settings handlers
        private void MaxContextPiecesTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                if (int.TryParse(maxContextPiecesTextBox.Text, out int maxContextPieces) && maxContextPieces >= 0)
                {
                    ConfigManager.Instance.SetMaxContextPieces(maxContextPieces);
                    Console.WriteLine($"Max context pieces set to: {maxContextPieces}");
                }
                else
                {
                    // Reset to current value from config if invalid
                    maxContextPiecesTextBox.Text = ConfigManager.Instance.GetMaxContextPieces().ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating max context pieces: {ex.Message}");
            }
        }

        private void MinContextSizeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                if (int.TryParse(minContextSizeTextBox.Text, out int minContextSize) && minContextSize >= 0)
                {
                    ConfigManager.Instance.SetMinContextSize(minContextSize);
                    Console.WriteLine($"Min context size set to: {minContextSize}");
                }
                else
                {
                    // Reset to current value from config if invalid
                    minContextSizeTextBox.Text = ConfigManager.Instance.GetMinContextSize().ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating min context size: {ex.Message}");
            }
        }

        private void MinChatBoxTextSizeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                if (int.TryParse(minChatBoxTextSizeTextBox.Text, out int minChatBoxTextSize) && minChatBoxTextSize >= 0)
                {
                    ConfigManager.Instance.SetChatBoxMinTextSize(minChatBoxTextSize);
                    Console.WriteLine($"Min ChatBox text size set to: {minChatBoxTextSize}");
                }
                else
                {
                    // Reset to current value from config if invalid
                    minChatBoxTextSizeTextBox.Text = ConfigManager.Instance.GetChatBoxMinTextSize().ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating min ChatBox text size: {ex.Message}");
            }
        }

        private void GameInfoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                string gameInfo = gameInfoTextBox.Text.Trim();
                ConfigManager.Instance.SetGameInfo(gameInfo);
                Console.WriteLine($"Game info updated: {gameInfo}");

                // Reset the hash to force a retranslation when game info changes
                Logic.Instance.ResetHash();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating game info: {ex.Message}");
            }
        }

        private void MinTextFragmentSizeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                if (int.TryParse(minTextFragmentSizeTextBox.Text, out int minSize) && minSize >= 0)
                {
                    ConfigManager.Instance.SetMinTextFragmentSize(minSize);
                    Console.WriteLine($"Minimum text fragment size set to: {minSize}");

                    // Reset the hash to force new OCR processing
                    Logic.Instance.ResetHash();
                }
                else
                {
                    // Reset to current value from config if invalid
                    minTextFragmentSizeTextBox.Text = ConfigManager.Instance.GetMinTextFragmentSize().ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating minimum text fragment size: {ex.Message}");
            }
        }

        private void TextSimilarThresholdTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                // Get last threshold value from config
                string lastThreshold = Convert.ToString(ConfigManager.Instance.GetTextSimilarThreshold());

                // Validate input is a valid number
                if (!double.TryParse(textSimilarThresholdTextBox.Text, System.Globalization.CultureInfo.InvariantCulture, out double similarThreshold))
                {
                    MessageBox.Show("Please enter a valid number for the threshold.",
                                "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Reset textbox to last valid value
                    textSimilarThresholdTextBox.Text = lastThreshold;
                    return;
                }

                // Check range
                if (similarThreshold > 1.0 || similarThreshold < 0.5)
                {
                    // Show warning message
                    MessageBox.Show("Please enter a value between 0.5 and 1.0",
                                "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Reset to current value from config
                    textSimilarThresholdTextBox.Text = lastThreshold;
                    Console.WriteLine($"Text similar threshold reset to default value: {lastThreshold}");
                    return;
                }

                // If we get here, the value is valid, so save it
                ConfigManager.Instance.SetTextSimilarThreshold(similarThreshold.ToString());
                Console.WriteLine($"Text similar threshold updated to: {similarThreshold}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating text similar threshold: {ex}");

                // Restore last known good value in case of any error
                textSimilarThresholdTextBox.Text = Convert.ToString(ConfigManager.Instance.GetTextSimilarThreshold());
            }
        }

        private void MinLetterConfidenceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                if (double.TryParse(minLetterConfidenceTextBox.Text, out double confidence) && confidence >= 0 && confidence <= 1)
                {
                    ConfigManager.Instance.SetMinLetterConfidence(confidence);
                    Console.WriteLine($"Minimum letter confidence set to: {confidence}");

                    // Reset the hash to force new OCR processing
                    Logic.Instance.ResetHash();
                }
                else
                {
                    // Reset to current value from config if invalid
                    minLetterConfidenceTextBox.Text = ConfigManager.Instance.GetMinLetterConfidence().ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating minimum letter confidence: {ex.Message}");
            }
        }

        private void MinLineConfidenceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Skip if initializing
                if (_isInitializing)
                    return;

                if (double.TryParse(minLineConfidenceTextBox.Text, out double confidence) && confidence >= 0 && confidence <= 1)
                {
                    ConfigManager.Instance.SetMinLineConfidence(confidence);
                    Console.WriteLine($"Minimum line confidence set to: {confidence}");

                    // Reset the hash to force new OCR processing
                    Logic.Instance.ResetHash();
                }
                else
                {
                    // Reset to current value from config if invalid
                    minLineConfidenceTextBox.Text = ConfigManager.Instance.GetMinLineConfidence().ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating minimum line confidence: {ex.Message}");
            }
        }

        private void CheckLanguagePackButton_Click(object sender, RoutedEventArgs e)
        {
            string? sourceLanguage = null;

            if (sourceLanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                sourceLanguage = selectedItem.Content?.ToString();
            }
            if (string.IsNullOrEmpty(sourceLanguage))
            {
                _isLanguagePackInstall = false;

                MessageBox.Show("No language selected.", "Language Pack Check", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                Console.WriteLine($"Checking language pack for: {sourceLanguage}");

                _isLanguagePackInstall = OneOCRManager.Instance.CheckLanguagePackInstall(sourceLanguage);

                if (_isLanguagePackInstall)
                {
                    MessageBox.Show("Language pack is installed.", "Language Pack Check", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (!string.IsNullOrEmpty(OneOCRManager.Instance._currentLanguageCode))
                    {
                        string message = "Language pack is not installed. \n\n" +
                                     "To install the corresponding language pack, please follow these steps:\n\n" +
                                     "Step 1: Press \"Windows + S\" button, type \"language settings\" and press Enter button.\n\n" +
                                     "Step 2: Click on \"Add a language\" button.\n\n" +
                                     $"Step 3: Type \"{OneOCRManager.Instance._currentLanguageCode}\" to search.\n\n" +
                                     "Step 4:  Click \"Next\" button, uncheck all option and click \"install\".\n\n" +
                                     "Wait for language package install complete and retry";

                        MessageBox.Show(message, "Language Pack Check", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("This language is not supported for OneOCR", "Language Pack Check", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

        }






        // Handle Clear Context button click
        private void ClearContextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("Clearing translation context and history");

                // Clear translation history in MainWindow
                MainWindow.Instance.ClearTranslationHistory();

                // Reset hash to force new translation on next capture
                Logic.Instance.ResetHash();

                // Clear any existing text objects
                Logic.Instance.ClearAllTextObjects();

                // Show success message
                MessageBox.Show("Translation context and history have been cleared.",
                    "Context Cleared", MessageBoxButton.OK, MessageBoxImage.Information);

                Console.WriteLine("Translation context cleared successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing translation context: {ex.Message}");
                MessageBox.Show($"Error clearing context: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Ignore Phrases methods

        // Load ignore phrases from ConfigManager
        private void LoadIgnorePhrases()
        {
            try
            {
                _ignorePhrases.Clear();

                // Get phrases from ConfigManager
                var phrases = ConfigManager.Instance.GetIgnorePhrases();

                // Add each phrase to the collection
                foreach (var (phrase, exactMatch) in phrases)
                {
                    if (!string.IsNullOrEmpty(phrase))
                    {
                        _ignorePhrases.Add(new IgnorePhrase(phrase, exactMatch));
                    }
                }

                // Set the ListView's ItemsSource
                ignorePhraseListView.ItemsSource = _ignorePhrases;

                Console.WriteLine($"Loaded {_ignorePhrases.Count} ignore phrases");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ignore phrases: {ex.Message}");
            }
        }

        // Save all ignore phrases to ConfigManager
        private void SaveIgnorePhrases()
        {
            try
            {
                if (_isInitializing)
                    return;

                // Convert collection to list of tuples
                var phrases = _ignorePhrases.Select(p => (p.Phrase, p.ExactMatch)).ToList();

                // Save to ConfigManager
                ConfigManager.Instance.SaveIgnorePhrases(phrases);

                // Force the Logic to refresh
                Logic.Instance.ResetHash();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving ignore phrases: {ex.Message}");
            }
        }

        // Add a new ignore phrase
        private void AddIgnorePhraseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string phrase = newIgnorePhraseTextBox.Text.Trim();

                if (string.IsNullOrEmpty(phrase))
                {
                    MessageBox.Show("Please enter a phrase to ignore.",
                        "Missing Phrase", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if the phrase already exists
                if (_ignorePhrases.Any(p => p.Phrase == phrase))
                {
                    MessageBox.Show($"The phrase '{phrase}' is already in the list.",
                        "Duplicate Phrase", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool exactMatch = newExactMatchCheckBox.IsChecked ?? true;

                // Add to the collection
                _ignorePhrases.Add(new IgnorePhrase(phrase, exactMatch));

                // Save to ConfigManager
                SaveIgnorePhrases();

                // Clear the input
                newIgnorePhraseTextBox.Text = "";

                Console.WriteLine($"Added ignore phrase: '{phrase}' (Exact Match: {exactMatch})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding ignore phrase: {ex.Message}");
                MessageBox.Show($"Error adding phrase: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Remove a selected ignore phrase
        private void RemoveIgnorePhraseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ignorePhraseListView.SelectedItem is IgnorePhrase selectedPhrase)
                {
                    string phrase = selectedPhrase.Phrase;

                    // Ask for confirmation
                    MessageBoxResult result = MessageBox.Show($"Are you sure you want to remove the phrase '{phrase}'?",
                        "Confirm Remove", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Remove from the collection
                        _ignorePhrases.Remove(selectedPhrase);

                        // Save to ConfigManager
                        SaveIgnorePhrases();

                        Console.WriteLine($"Removed ignore phrase: '{phrase}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing ignore phrase: {ex.Message}");
                MessageBox.Show($"Error removing phrase: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handle selection changed event
        private void IgnorePhraseListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Enable or disable the Remove button based on selection
            removeIgnorePhraseButton.IsEnabled = ignorePhraseListView.SelectedItem != null;
        }

        // Handle checkbox changed event
        private void IgnorePhrase_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isInitializing)
                    return;

                if (sender is System.Windows.Controls.CheckBox checkbox && checkbox.Tag is string phrase)
                {
                    bool exactMatch = checkbox.IsChecked ?? false;

                    // Find and update the phrase in the collection
                    foreach (var ignorePhrase in _ignorePhrases)
                    {
                        if (ignorePhrase.Phrase == phrase)
                        {
                            ignorePhrase.ExactMatch = exactMatch;

                            // Save to ConfigManager
                            SaveIgnorePhrases();

                            Console.WriteLine($"Updated ignore phrase: '{phrase}' (Exact Match: {exactMatch})");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating ignore phrase: {ex.Message}");
            }
        }

        private void ShowIconSignal_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool enabled = showIconSignalCheckBox.IsChecked ?? false;
            ConfigManager.Instance.SetShowIconSignal(enabled);
            Console.WriteLine($"Show icon signal set to {enabled}");
        }

        private void AudioProcessingProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            if (audioProcessingProviderComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                ConfigManager.Instance.SetAudioProcessingProvider(selectedItem.Content.ToString() ?? "OpenAI Realtime API");
            }
        }

        private void OpenAiRealtimeApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ConfigManager.Instance.SetOpenAiRealtimeApiKey(openAiRealtimeApiKeyPasswordBox.Password.Trim());
        }

        // Handle Auto-translate checkbox change for audio service
        private void AudioServiceAutoTranslateCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool enabled = audioServiceAutoTranslateCheckBox.IsChecked ?? false;
            ConfigManager.Instance.SetAudioServiceAutoTranslateEnabled(enabled);
            Console.WriteLine($"Settings window: Audio service auto-translate set to {enabled}");
        }

        // Handle Multi selection area checkbox change for multi selection area
        private void MultiSelectionAreaCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool enabled = multiSelectionAreaCheckBox.IsChecked ?? false;
            ConfigManager.Instance.SetUseMultiSelectionArea(enabled);
            Console.WriteLine($"Settings window: Multi selection area set to {enabled}");
            if (isNeedShowMessage)
            {
                // Show notification
                MessageBox.Show("When this feature is enabled, you can select multiple areas to translate by clicking the SelectArea button \n\n" +
                "Each selection corresponds to one translation area \n\n" +
                "To switch between translation areas, press ALT+number (number from 1 to 5) \n\n" +
                "The numbers correspond to the areas you have created; the first selected area is 1, and it increases up to 5 \n\n",
                            "Multi selection area guide",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
            }
            isNeedShowMessage = !enabled;
        }









        private void OneOCRIntegrationCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool enabled = oneOCRIntegrationCheckBox.IsChecked ?? false;
            ConfigManager.Instance.SetOneOCRIntegration(enabled);
            Console.WriteLine($"OneOCR integration set to {enabled}");
        }

        private void AutoOCRCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool enabled = AutoOCRCheckBox.IsChecked ?? true;
            ConfigManager.Instance.SetAutoOCR(enabled);
            Console.WriteLine($"Auto OCR set to {enabled}");
        }

        private void ExcludeCharacterNameCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool enabled = excludeCharacterNameCheckBox.IsChecked ?? false;
            ConfigManager.Instance.SetExcludeCharacterName(enabled);
            Console.WriteLine($"Exclude character name set to {enabled}");
        }

        // ChatGPT credential event handlers
        private void ChatGptUsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.Instance.SetChatGptUsername(chatGptUsernameTextBox.Text);
        }

        private void ChatGptEndpointTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigManager.Instance.SetChatGptEndpoint(chatGptEndpointTextBox.Text);
        }

        private void ChatGptPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ConfigManager.Instance.SetChatGptPassword(chatGptPasswordBox.Password);
        }
    }
}