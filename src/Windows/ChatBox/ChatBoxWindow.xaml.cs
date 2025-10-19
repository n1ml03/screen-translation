using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using FontFamily = System.Windows.Media.FontFamily;

namespace ScreenTranslation
{
    /// <summary>
    /// Helper class for ChatBox styling and UI element creation
    /// </summary>
    internal class ChatBoxStyler
    {
        // Constants for consistent styling
        public const double HeaderFontSize = 16;
        public const double ContentFontSize = 16;
        public const double TimestampFontSize = 11;
        public const double LanguageIndicatorFontSize = 11;
        public const double IconFontSize = 12;

        public const double BaseMarginMultiplier = 0.2;
        public const double SectionMarginMultiplier = 0.15;
        public const double HeaderMarginMultiplier = 0.15;
        public const double ContentMarginMultiplier = 0.08;

        // Color constants
        public static readonly Color TimestampColor = Color.FromRgb(180, 180, 180);
        public static readonly Color LanguageIndicatorColor = Color.FromRgb(100, 180, 255);
        public static readonly Color OriginalTextColor = Color.FromRgb(200, 200, 200);
        public static readonly Color TranslatedTextColor = Color.FromRgb(255, 255, 255);
        public static readonly Color SeparatorColor = Color.FromRgb(120, 120, 120);
        public static readonly Color EntryCountColor = Color.FromRgb(200, 200, 200);
        public static readonly Color EntryCountTruncatedColor = Color.FromRgb(255, 200, 100);
        public static readonly Color EntryCountEmptyColor = Color.FromRgb(150, 150, 150);

        // Background colors for alternating entries
        public static readonly Color EvenEntryBgColor = Color.FromRgb(100, 150, 200);
        public static readonly Color OddEntryBgColor = Color.FromRgb(100, 150, 120);
        public static readonly Color BorderColor = Color.FromRgb(200, 200, 200);

        private readonly Dictionary<Color, SolidColorBrush> _brushCache;

        public ChatBoxStyler(Dictionary<Color, SolidColorBrush> brushCache)
        {
            _brushCache = brushCache;
        }

        public SolidColorBrush GetBrush(Color color)
        {
            if (!_brushCache.TryGetValue(color, out var brush))
            {
                brush = new SolidColorBrush(color);
                brush.Freeze();
                _brushCache[color] = brush;
            }
            return brush;
        }

        public double CalculateBaseMargin(double fontSize) =>
            Math.Max(fontSize * BaseMarginMultiplier, 4);

        public double CalculateSectionMargin(double fontSize) =>
            Math.Max(fontSize * SectionMarginMultiplier, 3);

        public double CalculateHeaderMargin(double fontSize) =>
            Math.Max(fontSize * HeaderMarginMultiplier, 2);

        public double CalculateContentMargin(double fontSize) =>
            Math.Max(fontSize * ContentMarginMultiplier, 1);

        public Section CreateEntrySection(int entryIndex, double fontSize, double bgOpacity)
        {
            var section = new Section();

            // Calculate spacing
            double baseMargin = CalculateBaseMargin(fontSize);
            double sectionMargin = CalculateSectionMargin(fontSize);

            // Set alternating background
            byte bgOpacityValue = (byte)(bgOpacity <= 0 ? 8 : Math.Max(3, bgOpacity * 8));
            Color bgColor = entryIndex % 2 == 0 ? EvenEntryBgColor : OddEntryBgColor;
            section.Background = GetBrush(Color.FromArgb(bgOpacityValue, bgColor.R, bgColor.G, bgColor.B));

            // Add separator border (except first entry)
            if (entryIndex > 0)
            {
                byte borderOpacity = (byte)Math.Min(40, bgOpacity * 30 + 10);
                section.BorderBrush = GetBrush(Color.FromArgb(borderOpacity, BorderColor.R, BorderColor.G, BorderColor.B));
                section.BorderThickness = new Thickness(0, 1, 0, 0);
            }

            // Set padding
            section.Padding = new Thickness(baseMargin, sectionMargin, baseMargin, sectionMargin);

            return section;
        }

        public Paragraph CreateHeaderParagraph(TranslationEntry entry, double fontSize, int displayMode)
        {
            var para = new Paragraph
            {
                Margin = new Thickness(0, CalculateHeaderMargin(fontSize), 0, CalculateContentMargin(fontSize)),
                TextIndent = 0
            };

            // Add timestamp
            string timestamp = entry.Timestamp.ToString("HH:mm:ss");
            var timestampRun = new Run($"🕐 {timestamp}")
            {
                Foreground = GetBrush(TimestampColor),
                FontSize = Math.Max(fontSize - 5, 9),
                FontWeight = FontWeights.Normal
            };
            para.Inlines.Add(timestampRun);

            // Add language indicator for dual language mode
            if (displayMode == 0) // Both languages
            {
                string sourceLang = ConfigManager.Instance.GetSourceLanguage().ToUpper();
                string targetLang = ConfigManager.Instance.GetTargetLanguage().ToUpper();

                var separator = new Run(" • ")
                {
                    Foreground = GetBrush(SeparatorColor),
                    FontSize = Math.Max(fontSize - 5, 9)
                };
                para.Inlines.Add(separator);

                var langIndicator = new Run($"{sourceLang} → {targetLang}")
                {
                    Foreground = GetBrush(LanguageIndicatorColor),
                    FontSize = Math.Max(fontSize - 5, 9),
                    FontWeight = FontWeights.SemiBold
                };
                para.Inlines.Add(langIndicator);
            }

            return para;
        }

        public Paragraph CreateContentParagraph(TranslationEntry entry, double fontSize, int displayMode,
                                               bool translationFailed, bool isSourceRtl, bool isTargetRtl)
        {
            var para = new Paragraph
            {
                Margin = new Thickness(0, CalculateContentMargin(fontSize), 0, CalculateSectionMargin(fontSize)),
                TextIndent = 0,
                LineHeight = 1.5
            };

            // Add original text
            bool showOriginal = (displayMode == 0 || displayMode == 2) && !string.IsNullOrEmpty(entry.OriginalText);
            if (showOriginal || (translationFailed && (displayMode == 0 || displayMode == 1)))
            {
                AddTextWithIcon(para, "🔤", entry.OriginalText!, fontSize, isSourceRtl,
                               translationFailed ? Color.FromRgb(255, 180, 180) : OriginalTextColor, FontWeights.Normal);
            }

            // Add line break if showing both texts
            if (displayMode == 0 && !string.IsNullOrEmpty(entry.TranslatedText) && !translationFailed)
            {
                para.Inlines.Add(new LineBreak());
            }

            // Add translated text
            if ((displayMode == 0 || displayMode == 1) && !string.IsNullOrEmpty(entry.TranslatedText) && !translationFailed)
            {
                if (displayMode == 0) // Add icon only when showing both
                {
                    var icon = new Run("🌐 ")
                    {
                        FontSize = Math.Max(fontSize, 12)
                    };
                    para.Inlines.Add(icon);
                }

                var translatedRun = new Run(entry.TranslatedText)
                {
                    Foreground = GetBrush(TranslatedTextColor),
                    FontSize = fontSize,
                    FontWeight = FontWeights.SemiBold,
                    FlowDirection = isTargetRtl ? System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight
                };
                para.Inlines.Add(translatedRun);
            }

            return para;
        }

        private void AddTextWithIcon(Paragraph para, string icon, string text, double fontSize,
                                   bool isRtl, Color textColor, FontWeight fontWeight)
        {
            // Add icon
            var iconRun = new Run($"{icon} ")
            {
                FontSize = Math.Max(fontSize - 1, 11)
            };
            para.Inlines.Add(iconRun);

            // Add text
            var textRun = new Run(text)
            {
                Foreground = GetBrush(textColor),
                FontSize = Math.Max(fontSize - 2, 10),
                FontWeight = fontWeight,
                FlowDirection = isRtl ? System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight
            };
            para.Inlines.Add(textRun);
        }
    }

    public partial class ChatBoxWindow : Window
    {
        // Cached brushes to avoid repeated allocation
        private static readonly SolidColorBrush BlackBrush = new SolidColorBrush(Colors.Black);
        private static readonly Dictionary<Color, SolidColorBrush> _globalBrushCache = new Dictionary<Color, SolidColorBrush>();
        private readonly Dictionary<Color, SolidColorBrush> _brushCache = new Dictionary<Color, SolidColorBrush>();

        // ChatBox styling helper
        private readonly ChatBoxStyler _styler;

        // Helper method to get or create cached brush with thread safety
        private SolidColorBrush GetCachedBrush(Color color)
        {
            // First check instance cache for frequently used colors
            if (_brushCache.TryGetValue(color, out var brush))
            {
                return brush;
            }

            // Then check global cache
            lock (_globalBrushCache)
            {
                if (_globalBrushCache.TryGetValue(color, out brush))
                {
                    // Also add to instance cache for faster future access
                    _brushCache[color] = brush;
                    return brush;
                }

                // Create new brush and cache it
                brush = new SolidColorBrush(color);
                brush.Freeze(); // Freeze for better performance
                _globalBrushCache[color] = brush;
                _brushCache[color] = brush;
                return brush;
            }
        }

        // Constants
        private const int MAX_CONTEXT_HISTORY_SIZE = 100; // Max entries to keep for context purposes

        // We'll use MainWindow.Instance.translationHistory instead of maintaining our own
        private int _maxHistorySize; // Display history size from config
        private int _displayMode = 1; // 0 = both, 1 = target only, 2 = source only

        public static ChatBoxWindow? Instance { get; private set; }

        // Animation timer for translation status
        private DispatcherTimer? _animationTimer;

        // Semaphore to ensure only one speech request is processed at a time
        private static readonly SemaphoreSlim _speechSemaphore = new SemaphoreSlim(1, 1);

        // Thread-safe queue for speech requests
        private static readonly ConcurrentQueue<string> _speechQueue = new ConcurrentQueue<string>();

        // Flag to track if we're currently processing speech
        private static bool _isProcessingSpeech = false;

        // Cancellation token source for speech processing
        private static CancellationTokenSource? _speechCancellationTokenSource;

        private int _animationStep = 0;

        public ChatBoxWindow()
        {
            Instance = this;
            InitializeComponent();

            // Initialize styler
            _styler = new ChatBoxStyler(_brushCache);

            // Register application-wide keyboard shortcut handler
            this.PreviewKeyDown += Application_KeyDown;

            // Get max history size from configuration for display purposes
            _maxHistorySize = ConfigManager.Instance.GetChatBoxHistorySize();

            // Initialize the RichTextBox with a properly configured document
            chatHistoryText.Document = new FlowDocument()
            {
                // Set basic document properties
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 16,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Left,

                // Make sure the document is visible
                IsEnabled = true,
                IsHyphenationEnabled = false,

                // Ensure text wraps properly
                PageWidth = 350,

                // Standard margins
                PagePadding = new Thickness(5)
            };

            // Ensure the document is visible
            chatHistoryText.IsDocumentEnabled = true;

            // Set up context menu
            SetupContextMenu();

            // Apply custom styling from configuration
            ApplyConfigurationStyling();

            // Set up animation timer
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _animationTimer.Tick += AnimationTimer_Tick;

            // Subscribe to events
            this.Loaded += ChatBoxWindow_Loaded;
            this.Closing += ChatBoxWindow_Closing;

            // Listen for Logic's translation in progress status
            Logic.Instance.TranslationCompleted += OnTranslationCompleted;
        }

        private void SetupContextMenu()
        {
            // Create a context menu
            ContextMenu contextMenu = new ContextMenu();

            // Add standard menu items
            MenuItem cutItem = new MenuItem() { Header = "Cut" };
            cutItem.Command = ApplicationCommands.Cut;
            contextMenu.Items.Add(cutItem);

            MenuItem copyItem = new MenuItem() { Header = "Copy Selected" };
            copyItem.Command = ApplicationCommands.Copy;
            contextMenu.Items.Add(copyItem);

            MenuItem pasteItem = new MenuItem() { Header = "Paste" };
            pasteItem.Command = ApplicationCommands.Paste;
            contextMenu.Items.Add(pasteItem);

            // Add a separator
            contextMenu.Items.Add(new Separator());

            // Copy submenu
            MenuItem copySubmenu = new MenuItem() { Header = "Copy Text" };
            MenuItem copySourceItem = new MenuItem() { Header = "Copy Source Text" };
            copySourceItem.Click += (s, e) => CopySourceMenuItem_Click(s, e);
            copySubmenu.Items.Add(copySourceItem);

            MenuItem copyTranslatedItem = new MenuItem() { Header = "Copy Translated Text" };
            copyTranslatedItem.Click += (s, e) => CopyTranslatedMenuItem_Click(s, e);
            copySubmenu.Items.Add(copyTranslatedItem);

            MenuItem copyBothItem = new MenuItem() { Header = "Copy Both (Source + Translation)" };
            copyBothItem.Click += CopyBothMenuItem_Click;
            copySubmenu.Items.Add(copyBothItem);

            contextMenu.Items.Add(copySubmenu);

            // Add another separator
            contextMenu.Items.Add(new Separator());

            // Text-to-Speech submenu
            MenuItem speakSubmenu = new MenuItem() { Header = "Text-to-Speech" };
            MenuItem speakSourceItem = new MenuItem() { Header = "Speak Source Text" };
            speakSourceItem.Click += (s, e) => SpeakSourceMenuItem_Click(s, e);
            speakSubmenu.Items.Add(speakSourceItem);

            MenuItem speakTranslatedItem = new MenuItem() { Header = "Speak Translated Text" };
            speakTranslatedItem.Click += SpeakTranslatedMenuItem_Click;
            speakSubmenu.Items.Add(speakTranslatedItem);

            MenuItem stopSpeechItem = new MenuItem() { Header = "Stop Speaking" };
            stopSpeechItem.Click += StopSpeechMenuItem_Click;
            speakSubmenu.Items.Add(stopSpeechItem);

            contextMenu.Items.Add(speakSubmenu);

            // Learn menu item
            MenuItem learnItem = new MenuItem() { Header = "Learn with AI" };
            learnItem.Click += LearnMenuItem_Click;
            contextMenu.Items.Add(learnItem);

            // Add another separator
            contextMenu.Items.Add(new Separator());

            // History management
            MenuItem historySubmenu = new MenuItem() { Header = "History" };
            MenuItem clearHistoryItem = new MenuItem() { Header = "Clear All History" };
            clearHistoryItem.Click += (s, e) => ClearButton_Click(s, e);
            historySubmenu.Items.Add(clearHistoryItem);

            MenuItem exportHistoryItem = new MenuItem() { Header = "Export History..." };
            exportHistoryItem.Click += ExportHistoryMenuItem_Click;
            historySubmenu.Items.Add(exportHistoryItem);

            contextMenu.Items.Add(historySubmenu);

            // Add another separator
            contextMenu.Items.Add(new Separator());

            // Settings submenu
            MenuItem settingsSubmenu = new MenuItem() { Header = "Display Settings" };
            MenuItem showBothItem = new MenuItem() { Header = "Show Both Languages" };
            showBothItem.Click += (s, e) => { _displayMode = 0; UpdateChatHistory(); modeButton.Content = "Source&Translated Text"; };
            settingsSubmenu.Items.Add(showBothItem);

            MenuItem showTranslatedItem = new MenuItem() { Header = "Show Translated Only" };
            showTranslatedItem.Click += (s, e) => { _displayMode = 1; UpdateChatHistory(); modeButton.Content = "Translated Text"; };
            settingsSubmenu.Items.Add(showTranslatedItem);

            MenuItem showSourceItem = new MenuItem() { Header = "Show Source Only" };
            showSourceItem.Click += (s, e) => { _displayMode = 2; UpdateChatHistory(); modeButton.Content = "Source Text"; };
            settingsSubmenu.Items.Add(showSourceItem);

            contextMenu.Items.Add(settingsSubmenu);

            // Update menu item states when context menu is opened
            contextMenu.Opened += (s, e) =>
            {
                // Get selected text to determine available options
                TextRange selectedText = new TextRange(
                    chatHistoryText.Selection.Start,
                    chatHistoryText.Selection.End);

                bool hasSelection = !string.IsNullOrWhiteSpace(selectedText.Text);

                // Enable/disable items based on selection
                copySourceItem.IsEnabled = hasSelection;
                copyTranslatedItem.IsEnabled = hasSelection;
                copyBothItem.IsEnabled = hasSelection;
                speakSourceItem.IsEnabled = hasSelection;
                speakTranslatedItem.IsEnabled = hasSelection;
                learnItem.IsEnabled = hasSelection;

                // Set checkmarks for display mode
                showBothItem.IsChecked = _displayMode == 0;
                showTranslatedItem.IsChecked = _displayMode == 1;
                showSourceItem.IsChecked = _displayMode == 2;
            };

            // Assign the context menu to the RichTextBox
            chatHistoryText.ContextMenu = contextMenu;
        }

        // Click handler for Copy Source menu item
        private void CopySourceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected text
                TextRange selectedText = new TextRange(
                    chatHistoryText.Selection.Start,
                    chatHistoryText.Selection.End);

                if (!string.IsNullOrWhiteSpace(selectedText.Text))
                {
                    // For source text, we try to extract it from the selection
                    // This is a simplified approach - copy the entire selection
                    System.Windows.Forms.Clipboard.SetText(selectedText.Text.Trim());
                    Console.WriteLine("Copied source text to clipboard");
                }
                else
                {
                    Console.WriteLine("No text selected for Copy Source function");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Copy Source function: {ex.Message}");
            }
        }

        // Click handler for Copy Translated menu item
        private void CopyTranslatedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected text
                TextRange selectedText = new TextRange(
                    chatHistoryText.Selection.Start,
                    chatHistoryText.Selection.End);

                if (!string.IsNullOrWhiteSpace(selectedText.Text))
                {
                    // For translated text, we try to extract it from the selection
                    // This is a simplified approach - copy the entire selection
                    System.Windows.Forms.Clipboard.SetText(selectedText.Text.Trim());
                    Console.WriteLine("Copied translated text to clipboard");
                }
                else
                {
                    Console.WriteLine("No text selected for Copy Translated function");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Copy Translated function: {ex.Message}");
            }
        }

        // Click handler for Speak Source menu item
        private void SpeakSourceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected text
                TextRange selectedText = new TextRange(
                    chatHistoryText.Selection.Start,
                    chatHistoryText.Selection.End);

                if (!string.IsNullOrWhiteSpace(selectedText.Text))
                {
                    // For source text, we try to extract it from the selection
                    // This is a simplified approach - speak the entire selection
                    string text = selectedText.Text.Trim();
                    EnqueueSpeechRequest(text);
                    Console.WriteLine("Speaking source text");
                }
                else
                {
                    Console.WriteLine("No text selected for Speak Source function");
                    System.Windows.MessageBox.Show("Please select some text to speak first.",
                        "No Text Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Speak Source function: {ex.Message}");
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Text-to-Speech Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LearnMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected text
                TextRange selectedText = new TextRange(
                    chatHistoryText.Selection.Start,
                    chatHistoryText.Selection.End);

                if (!string.IsNullOrWhiteSpace(selectedText.Text))
                {
                    // Construct the ChatGPT URL with the selected text and instructions
                    string chatGptPrompt = $"Create a lesson to help me learn about this text and its translation: {selectedText.Text}";
                    string encodedPrompt = HttpUtility.UrlEncode(chatGptPrompt);
                    string chatGptUrl = $"https://chat.openai.com/?q={encodedPrompt}";

                    // Open in default browser
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = chatGptUrl,
                        UseShellExecute = true
                    });

                    Console.WriteLine($"Opening ChatGPT with selected text: {selectedText.Text.Substring(0, Math.Min(50, selectedText.Text.Length))}...");
                }
                else
                {
                    Console.WriteLine("No text selected for Learn function");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Learn function: {ex.Message}");
            }
        }

        private void SpeakMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected text
                TextRange selectedText = new TextRange(
                    chatHistoryText.Selection.Start,
                    chatHistoryText.Selection.End);

                if (!string.IsNullOrWhiteSpace(selectedText.Text))
                {
                    string text = selectedText.Text.Trim();
                    EnqueueSpeechRequest(text);
                }
                else
                {
                    Console.WriteLine("No text selected for Speak function");
                    System.Windows.MessageBox.Show("Please select some text to speak first.",
                        "No Text Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Speak function: {ex.Message}");
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Text-to-Speech Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click handler for Copy Both menu item
        private void CopyBothMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected text
                TextRange selectedText = new TextRange(
                    chatHistoryText.Selection.Start,
                    chatHistoryText.Selection.End);

                if (!string.IsNullOrWhiteSpace(selectedText.Text))
                {
                    // Try to find both source and translated text in the same entry
                    // This is a simplified approach - copy the selected text as-is
                    System.Windows.Forms.Clipboard.SetText(selectedText.Text.Trim());
                    Console.WriteLine("Copied both source and translated text to clipboard");
                }
                else
                {
                    Console.WriteLine("No text selected for Copy Both function");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Copy Both function: {ex.Message}");
            }
        }

        // Click handler for Speak Translated menu item
        private void SpeakTranslatedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the selected text
                TextRange selectedText = new TextRange(
                    chatHistoryText.Selection.Start,
                    chatHistoryText.Selection.End);

                if (!string.IsNullOrWhiteSpace(selectedText.Text))
                {
                    // For translated text, we try to extract it from the selection
                    // This is a simplified approach - speak the entire selection
                    string text = selectedText.Text.Trim();
                    EnqueueSpeechRequest(text);
                    Console.WriteLine("Speaking translated text");
                }
                else
                {
                    Console.WriteLine("No text selected for Speak Translated function");
                    System.Windows.MessageBox.Show("Please select some text to speak first.",
                        "No Text Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Speak Translated function: {ex.Message}");
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Text-to-Speech Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click handler for Stop Speech menu item
        private void StopSpeechMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Cancel any ongoing speech
                if (_speechCancellationTokenSource != null)
                {
                    _speechCancellationTokenSource.Cancel();
                    _speechCancellationTokenSource.Dispose();
                    _speechCancellationTokenSource = null;
                }

                _isProcessingSpeech = false;
                _speechQueue.Clear();

                Console.WriteLine("Speech stopped by user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping speech: {ex.Message}");
            }
        }

        // Click handler for Export History menu item
        private void ExportHistoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a save file dialog
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv",
                    DefaultExt = "txt",
                    FileName = $"translation_history_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var history = MainWindow.Instance.GetTranslationHistory();
                    var exportContent = new StringBuilder();

                    if (saveFileDialog.FilterIndex == 1) // TXT format
                    {
                        exportContent.AppendLine("Translation History");
                        exportContent.AppendLine($"Exported on: {DateTime.Now}");
                        exportContent.AppendLine(new string('=', 50));
                        exportContent.AppendLine();

                        foreach (var entry in history)
                        {
                            exportContent.AppendLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}]");
                            if (!string.IsNullOrEmpty(entry.OriginalText))
                            {
                                exportContent.AppendLine($"Source: {entry.OriginalText}");
                            }
                            if (!string.IsNullOrEmpty(entry.TranslatedText))
                            {
                                exportContent.AppendLine($"Translation: {entry.TranslatedText}");
                            }
                            exportContent.AppendLine();
                        }
                    }
                    else // CSV format
                    {
                        exportContent.AppendLine("Timestamp,Source Text,Translated Text");
                        foreach (var entry in history)
                        {
                            string source = entry.OriginalText?.Replace("\"", "\"\"") ?? "";
                            string translated = entry.TranslatedText?.Replace("\"", "\"\"") ?? "";
                            exportContent.AppendLine($"\"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{source}\",\"{translated}\"");
                        }
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, exportContent.ToString());
                    System.Windows.MessageBox.Show($"History exported to {saveFileDialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting history: {ex.Message}");
                System.Windows.MessageBox.Show($"Error exporting history: {ex.Message}",
                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Process the speech queue
        private static async Task ProcessSpeechQueueAsync(CancellationToken cancellationToken)
        {
            if (_isProcessingSpeech)
                return;

            _isProcessingSpeech = true;
            Console.WriteLine("Starting speech queue processing");

            try
            {

                await Task.Delay(5, cancellationToken);

                while (!_speechQueue.IsEmpty && !cancellationToken.IsCancellationRequested)
                {
                    StringBuilder combinedText = new StringBuilder();
                    int queueSize = _speechQueue.Count;
                    Console.WriteLine($"Processing {queueSize} speech requests as one batch");

                    // Dequeue all items and combine them
                    while (_speechQueue.TryDequeue(out string? textToSpeak) && !cancellationToken.IsCancellationRequested)
                    {
                        if (!string.IsNullOrWhiteSpace(textToSpeak))
                        {
                            // Add a space between items if needed
                            if (combinedText.Length > 0)
                            {
                                combinedText.Append(" ");
                            }

                            combinedText.Append(textToSpeak);
                        }
                    }

                    // If we have text to speak, process it as one request
                    if (combinedText.Length > 0 && !cancellationToken.IsCancellationRequested)
                    {
                        string finalText = combinedText.ToString();
                        Console.WriteLine($"Speaking combined text ({finalText.Length} chars): {finalText.Substring(0, Math.Min(50, finalText.Length))}...");

                        // Process the combined speech request
                        await Speak_Item_InternalAsync(finalText, cancellationToken);
                    }

                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Speech processing was cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in speech queue processing: {ex.Message}");
            }
            finally
            {
                _isProcessingSpeech = false;
                Console.WriteLine("Speech queue processing completed");
            }
        }

        // Enqueue a speech request and start processing if needed
        public static void EnqueueSpeechRequest(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            try
            {
                // Process the text to remove line breaks and normalize spaces
                string processedText = ProcessTextForSpeech(text);

                // Add the processed text to the queue
                _speechQueue.Enqueue(processedText);
                Console.WriteLine($"Speech request enqueued. Queue size: {_speechQueue.Count}");

                // Start processing if not already doing so
                if (!_isProcessingSpeech)
                {
                    if (_speechCancellationTokenSource != null)
                    {
                        _speechCancellationTokenSource.Cancel();
                        _speechCancellationTokenSource.Dispose();
                    }

                    // Create a new cancellation token source
                    _speechCancellationTokenSource = new CancellationTokenSource();

                    // Start the processing task
                    Task.Run(() => ProcessSpeechQueueAsync(_speechCancellationTokenSource.Token));
                }
                else
                {

                    bool interruptCurrentSpeech = false;
                    if (interruptCurrentSpeech && _speechCancellationTokenSource != null)
                    {
                        _speechCancellationTokenSource.Cancel();
                        _speechCancellationTokenSource.Dispose();
                        _speechCancellationTokenSource = new CancellationTokenSource();

                        _isProcessingSpeech = false;

                        Task.Run(() => ProcessSpeechQueueAsync(_speechCancellationTokenSource.Token));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enqueueing speech request: {ex.Message}");
            }
        }

        // Process text to optimize for speech with minimal pauses
        private static string ProcessTextForSpeech(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Replace multiple newlines with a single space to reduce pauses
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n+", ".");

            // Replace multiple spaces with a single space
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            // text = System.Text.RegularExpressions.Regex.Replace(text, @"\.{2,}", ".");
            // text = System.Text.RegularExpressions.Regex.Replace(text, @"\s*([.,;:!?])\s*", "$1 ");

            return text.Trim();
        }

        private static async Task<bool> Speak_Item_InternalAsync(string text, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(text) || cancellationToken.IsCancellationRequested)
                return false;

            try
            {
                string trimmedText = text.Trim();
                Console.WriteLine($"Speaking text: {trimmedText.Substring(0, Math.Min(50, trimmedText.Length))}...");

                // Check if TTS is enabled in config
                if (ConfigManager.Instance.IsTtsEnabled())
                {
                    string ttsService = ConfigManager.Instance.GetTtsService();

                    // Wait to acquire the semaphore - this ensures only one speech request runs at a time
                    await _speechSemaphore.WaitAsync(cancellationToken);

                    try
                    {
                        bool success = await WindowsTTSService.Instance.SpeakText(trimmedText);

                        if (!success)
                        {
                            Console.WriteLine($"Failed to generate speech using {ttsService}");
                        }

                        return success;
                    }
                    finally
                    {
                        // Always release the semaphore when done
                        _speechSemaphore.Release();
                    }
                }
                else
                {
                    Console.WriteLine("Text-to-Speech is disabled in settings");
                    return true; // Consider this successful since TTS is disabled
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Speech operation was cancelled");
                throw; // Re-throw to propagate cancellation
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Speak function: {ex.Message}");
                return false;
            }
        }

        // For backward compatibility - now just enqueues the speech request
        private void Speak_Item(string text)
        {
            EnqueueSpeechRequest(text);
        }

        private void ChatBoxWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Don't actually close the window, just hide it
            Console.WriteLine("ChatBox window closing intercepted - hiding instead");

            // Cancel the closing operation
            e.Cancel = true;

            // Hide the window instead
            this.Hide();

            // Note: We maintain timer and event subscriptions since the window instance stays alive

            // Periodic cleanup of brush cache to prevent memory leaks
            if (_globalBrushCache.Count > 200) // Limit global cache size
            {
                lock (_globalBrushCache)
                {
                    // Keep only the most recently used brushes (simple LRU approximation)
                    var keysToRemove = _globalBrushCache.Keys.Take(_globalBrushCache.Count - 100).ToList();
                    foreach (var key in keysToRemove)
                    {
                        _globalBrushCache.Remove(key);
                    }
                    Console.WriteLine($"Cleaned up {_globalBrushCache.Count} brushes from global cache");
                }
            }
        }

        public void ApplyConfigurationStyling()
        {
            try
            {
                // Get styling from ConfigManager
                var fontFamily = ConfigManager.Instance.GetChatBoxFontFamily();
                var fontSize = ConfigManager.Instance.GetChatBoxFontSize();
                var fontColor = ConfigManager.Instance.GetChatBoxFontColor();
                var backgroundColor = ConfigManager.Instance.GetChatBoxBackgroundColor();
                var bgOpacity = ConfigManager.Instance.GetChatBoxBackgroundOpacity();
                var windowOpacity = ConfigManager.Instance.GetChatBoxWindowOpacity();

                // Apply background color with its opacity to window
                if (bgOpacity <= 0)
                {
                    // Make all backgrounds completely transparent when opacity is 0
                    this.Background = Brushes.Transparent;

                    // Also make the ScrollViewer, RichTextBox and resize grip transparent
                    if (chatScrollViewer != null)
                    {
                        chatScrollViewer.Background = Brushes.Transparent;
                    }
                    if (chatHistoryText != null)
                    {
                        chatHistoryText.Background = Brushes.Transparent;
                    }

                    if (resizeGrip != null)
                    {
                        resizeGrip.Fill = Brushes.Transparent;
                    }
                }
                else
                {
                    // Calculate opacity value that:
                    // - At 0%, stays 0%
                    // - At 50%, is more like true 50%
                    // - At 100%, is actually 100%
                    double scaledOpacity;
                    if (bgOpacity >= 0.95)
                    {
                        // Ensure full opacity when set to maximum
                        scaledOpacity = 1.0;
                    }
                    else
                    {
                        // Use a blend of linear and square-root for intermediate values
                        scaledOpacity = 0.7 * Math.Sqrt(bgOpacity) + 0.3 * bgOpacity;
                    }

                    // Set main window background
                    Color bgColorWithOpacity = Color.FromArgb(
                        (byte)(scaledOpacity * 255), // Full opacity when slider is at 100%
                        backgroundColor.R,
                        backgroundColor.G,
                        backgroundColor.B);
                    this.Background = GetCachedBrush(bgColorWithOpacity);

                    // Set the RichTextBox background directly to match
                    if (chatHistoryText != null)
                    {
                        chatHistoryText.Background = GetCachedBrush(Color.FromArgb(
                            (byte)(scaledOpacity * 255),
                            0, 0, 0)); // Black background
                    }

                    // Set resize grip
                    if (resizeGrip != null)
                    {
                        Color gripColor = Color.FromArgb(
                            (byte)(bgOpacity * 128), // Half opacity of background
                            128, 128, 128);          // Gray color
                        resizeGrip.Fill = GetCachedBrush(gripColor);
                    }
                }

                // Apply window opacity
                this.Opacity = windowOpacity;

                // Ensure header bar is always visible (at least 50% opacity)
                if (headerBar != null)
                {
                    // Calculate header opacity - always at least 50% opaque
                    byte headerOpacity = (byte)Math.Max(128, (int)(bgOpacity * 255));

                    // Make header always visible
                    Color headerColor = Color.FromArgb(
                        headerOpacity,  // At least 50% opaque
                        0x20, 0x20, 0x20);  // Dark gray
                    headerBar.Background = GetCachedBrush(headerColor);
                }

                // Store values for use when creating text entries
                this.FontFamily = new FontFamily(fontFamily);
                ChatFontSize = fontSize;  // Set the chat-specific font size
                this.Foreground = GetCachedBrush(fontColor);

                // Apply updated styling to existing entries
                UpdateChatHistory();

                Console.WriteLine($"Applied ChatBox styling: Font={fontFamily}, Size={fontSize}, Color={fontColor}, BG={backgroundColor}, Window Opacity={windowOpacity}, BG Opacity={bgOpacity}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying ChatBox styling: {ex.Message}");
            }
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create options window
                var optionsWindow = new ChatBoxOptionsWindow();

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
                    CreateFlashAnimation(optionsButton);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing options dialog: {ex.Message}");
            }
        }

        private void ChatBoxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Position the window based on screen bounds if not already positioned
            if (this.Left == 0 && this.Top == 0)
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                // Default to bottom right corner
                this.Left = screenWidth - this.Width - 20;
                this.Top = screenHeight - this.Height - 40;
            }

            // Add SizeChanged event handler for reflowing text when window is resized
            this.SizeChanged += ChatBoxWindow_SizeChanged;
        }

        // Handler for application-level keyboard shortcuts
        private void Application_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Forward to the central keyboard shortcuts handler
            KeyboardShortcuts.HandleKeyDown(e);
        }

        private void ChatBoxWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Reflow text when window size changes
            UpdateChatHistory();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Instead of closing, hide the window (to match behavior with Log window)
            this.Hide();
            MainWindow.Instance.chatBoxButton.Background = GetCachedBrush(System.Windows.Media.Color.FromRgb(69, 176, 105));

            // The MainWindow will handle setting isChatBoxVisible to false in its event handler
        }

        // Store chat font size separately from window fonts
        private double _chatFontSize = 16;  // Default chat font size

        public double ChatFontSize
        {
            get { return _chatFontSize; }
            set
            {
                _chatFontSize = Math.Max(8, Math.Min(48, value));  // Clamp between 8 and 48
            }
        }

        private void FontIncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Increase font size and update
                ChatFontSize += 1;
                UpdateChatHistory();

                // Save the new font size to config
                ConfigManager.Instance.SetValue(ConfigManager.CHATBOX_FONT_SIZE, ChatFontSize.ToString());
                ConfigManager.Instance.SaveConfig();

                // Create and start the flash animation for visual feedback
                CreateFlashAnimation(fontIncreaseButton);

                Console.WriteLine($"Chat font size increased to {ChatFontSize} and saved to config");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error increasing font size: {ex.Message}");
            }
        }

        private void FontDecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Decrease font size and update
                ChatFontSize -= 1;
                UpdateChatHistory();

                // Save the new font size to config
                ConfigManager.Instance.SetValue(ConfigManager.CHATBOX_FONT_SIZE, ChatFontSize.ToString());
                ConfigManager.Instance.SaveConfig();

                // Create and start the flash animation for visual feedback
                CreateFlashAnimation(fontDecreaseButton);

                Console.WriteLine($"Chat font size decreased to {ChatFontSize} and saved to config");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decreasing font size: {ex.Message}");
            }
        }


        // Create and apply a flash animation for the button
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
                    Color originalColor = currentBrush.Color;

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

        // Play the clipboard sound
        private void PlayClipboardSound()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string soundPath = System.IO.Path.Combine(appDirectory, "audio", "clipboard.wav") ?? "";

                if (System.IO.File.Exists(soundPath))
                {
                    var player = new System.Media.SoundPlayer(soundPath);
                    player.Play();
                }
                else
                {
                    Console.WriteLine($"Clipboard sound file not found: {soundPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing clipboard sound: {ex.Message}");
            }
        }

        public void OnTranslationWasAdded(string originalText, string translatedText)
        {
            // Hide translation status indicator if it was visible
            HideTranslationStatus();

            // Update UI with existing history
            UpdateChatHistory();

            if (!string.IsNullOrEmpty(translatedText) && ConfigManager.Instance.IsTtsEnabled())
            {
                if (ConfigManager.Instance.IsExcludeCharacterNameEnabled())
                {
                    string[] text = translatedText.Split(':', 2);
                    if (text.Length > 1)
                    {
                        EnqueueSpeechRequest(text[1]);
                    }
                    else
                    {
                        EnqueueSpeechRequest(text[0]);
                    }
                }
                else
                {
                    EnqueueSpeechRequest(translatedText);
                }
            }
        }

        // Handle animation timer tick - now used for progress bar animation
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            // The progress bar is indeterminate, so no manual animation needed
            // This timer is kept for backward compatibility and potential future use
            _animationStep = (_animationStep + 1) % 4;
        }

        // Show translation status indicator with animation
        public void ShowTranslationStatus(bool bSettling)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ShowTranslationStatus(bSettling));
                return;
            }

            if (translationStatusPanel != null && translationStatusText != null && serviceIcon != null)
            {
                // Show the translation status panel
                translationStatusPanel.Visibility = Visibility.Visible;

                // Update content based on settling state
                if (bSettling)
                {
                    // If settling, show settling message
                    translationStatusText.Text = "Processing captured text...";
                    serviceIcon.Text = "⚡";
                    serviceIcon.Foreground = GetCachedBrush(Color.FromRgb(255, 215, 0)); // Gold
                }
                else
                {
                    // If translating, show translation notification with service name
                    string service = ConfigManager.Instance.GetCurrentTranslationService();

                    // Set appropriate icon and text based on service
                    switch (service.ToLower())
                    {
                        case "openai":
                        case "chatgpt":
                            serviceIcon.Text = "🤖";
                            translationStatusText.Text = "Translating with OpenAI...";
                            break;
                        default:
                            serviceIcon.Text = "🔄";
                            translationStatusText.Text = $"Translating with {service}...";
                            break;
                    }

                    serviceIcon.Foreground = GetCachedBrush(Color.FromRgb(255, 215, 0)); // Gold for active translation
                }

                // Start animation timer in all cases (for backward compatibility with old animation)
                if (_animationTimer != null && !_animationTimer.IsEnabled)
                {
                    _animationStep = 0;
                    _animationTimer.Start();
                }
            }
        }

        // Hide translation status indicator
        public void HideTranslationStatus()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => HideTranslationStatus());
                return;
            }

            if (translationStatusPanel != null)
            {
                translationStatusPanel.Visibility = Visibility.Collapsed;

                // Stop the animation timer
                if (_animationTimer != null && _animationTimer.IsEnabled)
                {
                    _animationTimer.Stop();
                }
            }
        }

        // Handle translation completed event
        private void OnTranslationCompleted(object? sender, TranslationEventArgs e)
        {
            // Hide the translation status indicator
            HideTranslationStatus();
        }

        // Get recent original texts for context
        public List<string> GetRecentOriginalTexts(int maxCount, int minContextSize)
        {
            var result = new List<string>();

            // Get access to MainWindow's translation history
            var mainWindowHistory = MainWindow.Instance.GetTranslationHistory();

            // If no history or count is zero, return empty list
            if (mainWindowHistory.Count == 0 || maxCount <= 0)
            {
                return result;
            }

            // Copy the queue to a list so we can access by index, most recent first
            var historyList = mainWindowHistory.Reverse().ToList();
            int collected = 0;

            // Collect entries until we have the requested number
            for (int i = 0; i < historyList.Count && collected < maxCount; i++)
            {
                if (!string.IsNullOrEmpty(historyList[i].OriginalText))
                {
                    if (historyList[i].OriginalText.Length >= minContextSize)
                    {
                        result.Add(historyList[i].OriginalText);
                        collected++;
                    }
                }
            }

            // Reverse the list so older entries come first (chronological order)
            result.Reverse();

            return result;
        }

        private void ModeButton_Click(object? sender, RoutedEventArgs? e)
        {
            try
            {
                // Cycle through display modes: 0 (both) -> 1 (target only) -> 2 (source only) -> 0 (both)
                _displayMode = (_displayMode + 1) % 3;
                if (_displayMode == 0)
                {
                    // Both
                    modeButton.Content = "Both";
                }
                else if (_displayMode == 1)
                {
                    // Target only
                    modeButton.Content = "Translation";
                }
                else if (_displayMode == 2)
                {
                    // Source only
                    modeButton.Content = "Source";
                }

                // Update the UI
                UpdateChatHistory();

                // Create and start the flash animation for visual feedback
                CreateFlashAnimation(modeButton);

                // string[] modes = { "both languages", "target language only", "source language only" };
                Console.WriteLine($"Display mode changed to: {modeButton.Content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing display mode: {ex.Message}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear the translation history queue in MainWindow
                MainWindow.Instance.ClearTranslationHistory();

                // Update the UI to show empty history
                UpdateChatHistory();

                // Create and start the flash animation for visual feedback
                CreateFlashAnimation(clearButton);

                Console.WriteLine("Translation history cleared");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing translation history: {ex.Message}");
            }
        }

        private void CancelTranslationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hide the translation status panel
                HideTranslationStatus();

                // Cancel any ongoing translation process
                // TODO: Implement cancellation in Logic class when needed
                // For now, just hide the status panel

                Console.WriteLine("Translation cancelled by user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling translation: {ex.Message}");
            }
        }

        public void UpdateChatHistory()
        {
            // Only update if window is visible
            if (!this.IsVisible)
                return;

            // Run on UI thread
            this.Dispatcher.Invoke(() =>
            {
                // Performance optimization: Skip update if window size hasn't changed significantly
                // and content hasn't changed (basic check)
                var translationHistory = MainWindow.Instance.GetTranslationHistory();
                if (translationHistory.Count == 0 && chatHistoryText.Document.Blocks.Count == 0)
                {
                    // Update entry count even when empty
                    UpdateEntryCountDisplay(0);
                    return; // No content to display
                }
                // Get styling from window's properties and config
                var fontFamily = this.FontFamily;
                var fontSize = this.ChatFontSize;
                var originalTextColor = ConfigManager.Instance.GetOriginalTextColor();
                var translatedTextColor = ConfigManager.Instance.GetTranslatedTextColor();
                var bgOpacity = ConfigManager.Instance.GetChatBoxBackgroundOpacity();

                // Get current target language
                string targetLanguage = ConfigManager.Instance.GetTargetLanguage().ToLower();
                string sourceLanguage = ConfigManager.Instance.GetSourceLanguage().ToLower();

                // Define RTL (Right-to-Left) languages
                HashSet<string> rtlLanguages = new HashSet<string> {
                    "ar", "arabic", "fa", "farsi", "persian", "he", "hebrew", "ur", "urdu"
                };

                // Check if languages are RTL
                bool isTargetRtl = rtlLanguages.Contains(targetLanguage);
                bool isSourceRtl = rtlLanguages.Contains(sourceLanguage);

                if (isTargetRtl)
                {
                    Console.WriteLine($"ChatBox: Detected RTL target language: {targetLanguage}");
                }

                // Performance optimization: Only clear and rebuild if necessary
                // Check if we need to rebuild (simplified check - rebuild if count differs significantly)
                int currentBlockCount = chatHistoryText.Document.Blocks.Count;
                int expectedBlockCount = Math.Min(translationHistory.Count * 2, _maxHistorySize * 2); // Rough estimate: 2 blocks per entry

                if (Math.Abs(currentBlockCount - expectedBlockCount) < 3 && currentBlockCount > 0)
                {
                    // Minor changes, might not need full rebuild - but for now, we'll keep it simple and rebuild
                    // TODO: Implement incremental updates for better performance
                }

                // Clear existing content
                chatHistoryText.Document.Blocks.Clear();

                // Set up document properties to enable text wrapping
                double viewportWidth = CalculateOptimalPageWidth();

                // Only update page width if it changed significantly
                if (Math.Abs(chatHistoryText.Document.PageWidth - viewportWidth) > 10)
                {
                    chatHistoryText.Document.PageWidth = viewportWidth;
                }

                // Set the background opacity
                if (bgOpacity <= 0)
                {
                    chatHistoryText.Background = Brushes.Transparent;
                }
                else
                {
                    // Calculate opacity value with our scaling formula
                    double scaledOpacity;
                    if (bgOpacity >= 0.95)
                    {
                        scaledOpacity = 1.0;
                    }
                    else
                    {
                        scaledOpacity = 0.7 * Math.Sqrt(bgOpacity) + 0.3 * bgOpacity;
                    }

                    // Apply the background color to the ScrollViewer instead of RichTextBox
                    chatScrollViewer.Background = GetCachedBrush(Color.FromArgb(
                        (byte)(scaledOpacity * 255),
                        0, 0, 0));
                    chatHistoryText.Background = Brushes.Transparent;
                }

                // Get the history from MainWindow (already retrieved above as translationHistory)
                // Get only the most recent entries for display (based on _maxHistorySize)
                var displayHistory = translationHistory.Reverse().Take(_maxHistorySize).Reverse();

                // Update entry count display
                int totalEntries = translationHistory.Count;
                int displayEntries = displayHistory.Count();
                UpdateEntryCountDisplay(totalEntries, displayEntries);

                // Get Min ChatBox Text Size setting
                int minChatBoxTextSize = ConfigManager.Instance.GetChatBoxMinTextSize();

                // Track entry number for alternating backgrounds
                int entryIndex = 0;

                // Create a paragraph for each entry to display
                foreach (var entry in displayHistory)
                {
                    // Skip entries with source text smaller than minimum size
                    if (!string.IsNullOrEmpty(entry.OriginalText) && entry.OriginalText.Length < minChatBoxTextSize)
                    {
                        continue;
                    }

                    // Create entry section using styler
                    Section entrySection = _styler.CreateEntrySection(entryIndex++, fontSize, bgOpacity);

                    // Create header paragraph using styler
                    Paragraph headerPara = _styler.CreateHeaderParagraph(entry, fontSize, _displayMode);
                    entrySection.Blocks.Add(headerPara);

                    // Check if translation failed
                    bool translationFailed = !string.IsNullOrEmpty(entry.OriginalText) &&
                                             !string.IsNullOrEmpty(entry.TranslatedText) &&
                                             entry.OriginalText == entry.TranslatedText;

                    // Create content paragraph using styler
                    Paragraph contentPara = _styler.CreateContentParagraph(entry, fontSize, _displayMode,
                                                                          translationFailed, isSourceRtl, isTargetRtl);

                    // Set flow direction for the entire paragraph
                    if ((_displayMode == 1 && isTargetRtl) ||
                        (_displayMode == 2 && isSourceRtl) ||
                        (_displayMode == 0 && isTargetRtl))
                    {
                        contentPara.FlowDirection = System.Windows.FlowDirection.RightToLeft;
                    }
                    else
                    {
                        contentPara.FlowDirection = System.Windows.FlowDirection.LeftToRight;
                    }

                    entrySection.Blocks.Add(contentPara);

                    // Add the section to the document
                    chatHistoryText.Document.Blocks.Add(entrySection);
                }

                // Scroll to the bottom to see newest entries
                chatScrollViewer.ScrollToEnd();
            });
        }

        // Update the entry count display in the header
        private void UpdateEntryCountDisplay(int totalEntries, int displayEntries = -1)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateEntryCountDisplay(totalEntries, displayEntries));
                return;
            }

            if (entryCountText != null)
            {
                if (displayEntries == -1)
                {
                    displayEntries = totalEntries;
                }

                if (totalEntries == 0)
                {
                    entryCountText.Text = "No entries";
                    entryCountText.Foreground = _styler.GetBrush(ChatBoxStyler.EntryCountEmptyColor);
                }
                else if (totalEntries <= displayEntries)
                {
                    entryCountText.Text = $"{totalEntries} entr{(totalEntries == 1 ? "y" : "ies")}";
                    entryCountText.Foreground = _styler.GetBrush(ChatBoxStyler.EntryCountColor);
                }
                else
                {
                    entryCountText.Text = $"{displayEntries} of {totalEntries} entries";
                    entryCountText.Foreground = _styler.GetBrush(ChatBoxStyler.EntryCountTruncatedColor);
                }
            }
        }

        // Overload for simple updates
        private void UpdateEntryCountDisplay(int totalEntries)
        {
            UpdateEntryCountDisplay(totalEntries, totalEntries);
        }

        // Calculate optimal page width for text wrapping
        private double CalculateOptimalPageWidth()
        {
            // PageWidth should match the viewport width of the ScrollViewer (minus padding)
            // Subtract extra pixels to ensure text doesn't get too close to the scrollbar
            double viewportWidth = chatScrollViewer.ActualWidth > 0 ? chatScrollViewer.ActualWidth - 30 : 320;

            // Ensure minimum width for readability
            return Math.Max(viewportWidth, 200);
        }
    }

    public class TranslationEntry
    {
        public string OriginalText { get; set; } = string.Empty;
        public string TranslatedText { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}