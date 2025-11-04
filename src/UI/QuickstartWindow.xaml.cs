using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Color = System.Windows.Media.Color;
using Button = System.Windows.Controls.Button;

namespace ScreenTranslation
{
    /// <summary>
    /// Interaction logic for QuickstartWindow.xaml
    /// </summary>
    public partial class QuickstartWindow : Window
    {
        private int currentStep = 1;
        private const int TotalSteps = 5;
        private ConfigManager configManager;
        private bool LoadedLanguageSettings = false;
        private bool LoadedOcrSettings = false;
        private bool LoadedTranslationSettings = false;

        public QuickstartWindow()
        {
            InitializeComponent();
            configManager = ConfigManager.Instance;

            // Set initial step
            NavigateToStep(1);

            // Center window on screen
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void NavigateToStep(int step)
        {
            currentStep = step;

            // Update progress indicator
            UpdateProgressIndicator();

            // Hide all content panels
            WelcomePanel.Visibility = Visibility.Collapsed;
            LanguagePanel.Visibility = Visibility.Collapsed;
            OcrPanel.Visibility = Visibility.Collapsed;
            TranslationPanel.Visibility = Visibility.Collapsed;
            CompletePanel.Visibility = Visibility.Collapsed;

            // Show the appropriate panel based on the current step
            switch (step)
            {
                case 1:
                    WelcomePanel.Visibility = Visibility.Visible;
                    PrevButton.IsEnabled = false;
                    break;
                case 2:
                    LanguagePanel.Visibility = Visibility.Visible;
                    PrevButton.IsEnabled = true;
                    if (!LoadedLanguageSettings)
                    {
                        LoadLanguageSettings();
                    }
                    break;
                case 3:
                    OcrPanel.Visibility = Visibility.Visible;
                    PrevButton.IsEnabled = true;
                    if (!LoadedOcrSettings)
                    {
                        LoadOcrSettings();
                    }
                    break;
                case 4:
                    TranslationPanel.Visibility = Visibility.Visible;
                    PrevButton.IsEnabled = true;
                    if (!LoadedTranslationSettings)
                    {
                        LoadTranslationSettings();
                    }
                    break;
                case 5:
                    CompletePanel.Visibility = Visibility.Visible;
                    PrevButton.IsEnabled = true;
                    NextButton.Visibility = Visibility.Collapsed;
                    FinishButton.Visibility = Visibility.Visible;
                    LoadSummarySettings();
                    break;
            }
        }

        private void UpdateProgressIndicator()
        {
            // Reset all step indicators
            Step1Indicator.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            Step2Indicator.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            Step3Indicator.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            Step4Indicator.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            Step5Indicator.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));

            // Highlight current step
            switch (currentStep)
            {
                case 1:
                    Step1Indicator.Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Blue
                    break;
                case 2:
                    Step2Indicator.Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Blue
                    break;
                case 3:
                    Step3Indicator.Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Blue
                    break;
                case 4:
                    Step4Indicator.Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Blue
                    break;
                case 5:
                    Step5Indicator.Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Blue
                    break;
            }
        }

        #region Language Settings

        private void LoadLanguageSettings()
        {
            // Load source and target languages from config
            string sourceLanguage = configManager.GetSourceLanguage();
            string targetLanguage = configManager.GetTargetLanguage();

            // Populate language dropdowns
            PopulateLanguageComboBox(SourceLanguageComboBox);
            PopulateLanguageComboBox(TargetLanguageComboBox);

            // Set current selections
            SourceLanguageComboBox.SelectedItem = sourceLanguage;
            TargetLanguageComboBox.SelectedItem = targetLanguage;
            LoadedLanguageSettings = true;
        }

        private void PopulateLanguageComboBox(System.Windows.Controls.ComboBox comboBox)
        {
            comboBox.Items.Clear();

            // List of supported languages
            List<string> languages = new List<string>
            {
                "ja",
                "en",
                "ch_sim",
                "ch_tra",
                "ko",
                "vi",
                "fr",
                "ru",
                "de",
                "es",
                "it",
                "hi",
                "pt",
                "ar",
                "nl",
                "pl",
                "ro",
                "fa",
                "cs",
                "id",
                "th"
            };

            // Sort languages alphabetically
            languages.Sort();

            // Add to ComboBox
            foreach (string language in languages)
            {
                comboBox.Items.Add(language);
            }
        }


        private void CommonPair_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            if (button != null && button.Tag != null)
            {
                string? tagString = button.Tag.ToString();
                if (tagString != null)
                {
                    string[] languages = tagString.Split(',');
                    if (languages.Length == 2)
                    {
                        // Find and select the languages in the comboboxes
                        SourceLanguageComboBox.SelectedItem = languages[0];
                        TargetLanguageComboBox.SelectedItem = languages[1];
                    }
                    else
                    {
                        // Default language select
                        SourceLanguageComboBox.SelectedItem = "en";
                        TargetLanguageComboBox.SelectedItem = "vi";
                    }
                }
            }
        }

        #endregion

        #region OCR Settings

        private void LoadOcrSettings()
        {
            // Load OCR method from config
            string ocrMethod = configManager.GetOcrMethod();

            // Populate OCR method dropdown
            OcrMethodComboBox.Items.Clear();
            OcrMethodComboBox.Items.Add("Windows OCR");
            OcrMethodComboBox.Items.Add("PaddleOCR");
            OcrMethodComboBox.Items.Add("OneOCR");

            // Set current selection
            switch (ocrMethod.ToLower())
            {
                case "windows ocr":
                    OcrMethodComboBox.SelectedItem = "Windows OCR";
                    break;
                case "oneocr":
                    OcrMethodComboBox.SelectedItem = "OneOCR";
                    break;
                case "paddleocr":
                    OcrMethodComboBox.SelectedItem = "PaddleOCR";
                    break;
                default:
                    OcrMethodComboBox.SelectedItem = "Windows OCR";
                    break;
            }
            LoadedOcrSettings = true;
        }




        #endregion

        #region Translation Settings

        private void LoadTranslationSettings()
        {
            // Load translation service from config
            string translationService = configManager.GetCurrentTranslationService();

            // Populate translation service dropdown
            TranslationServiceComboBox.Items.Clear();
            TranslationServiceComboBox.Items.Add("ChatGPT");

            // Set current selection
            switch (translationService.ToLower())
            {
                case "chatgpt":
                    TranslationServiceComboBox.SelectedItem = "ChatGPT";
                    break;
                default:
                    TranslationServiceComboBox.SelectedItem = "ChatGPT";
                    break;
            }

            // Load endpoint
            ChatGptEndpointTextBox.Text = configManager.GetChatGptEndpoint();
            ChatGptUsernameTextBox.Text = configManager.GetChatGptUsername();
            ChatGptPasswordBox.Password = configManager.GetChatGptPassword();

            // Update visibility of endpoint fields based on selected service
            UpdateEndpointFieldsVisibility();
            LoadedTranslationSettings = true;
        }

        private void TranslationServiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TranslationServiceComboBox.SelectedItem != null)
            {

                // Update visibility of endpoint fields
                UpdateEndpointFieldsVisibility();
            }
        }

        private void UpdateEndpointFieldsVisibility()
        {
            // Hide all endpoint fields
            ChatGptEndpointPanel.Visibility = Visibility.Collapsed;

            // Show the appropriate endpoint field based on the selected service
            if (TranslationServiceComboBox.SelectedItem != null)
            {
                string? selectedService = TranslationServiceComboBox.SelectedItem.ToString();

                switch (selectedService)
                {
                    case "ChatGPT":
                        ChatGptEndpointPanel.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private void ChatGptEndpointTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            configManager.SetChatGptEndpoint(ChatGptEndpointTextBox.Text);
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (sender is System.Windows.Controls.TextBox textBox)
                {
                    string endpoint = textBox.Text.Trim();
                    string? serviceType = TranslationServiceComboBox.SelectedItem.ToString();

                    if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(serviceType))
                    {
                        // Validate endpoint format
                        if (endpoint.StartsWith("http://") || endpoint.StartsWith("https://"))
                        {
                            configManager.SetChatGptEndpoint(endpoint);
                            Console.WriteLine($"Endpoint saved for {serviceType}");

                            System.Windows.MessageBox.Show($"Endpoint saved for {serviceType}.", "Endpoint Saved",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Please enter a valid HTTP/HTTPS URL.", "Invalid Endpoint",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void ChatGptUsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ChatGptUsernameTextBox != null)
            {
                configManager.SetChatGptUsername(ChatGptUsernameTextBox.Text);
            }
        }

        private void ChatGptPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ChatGptPasswordBox != null)
            {
                configManager.SetChatGptPassword(ChatGptPasswordBox.Password);
            }
        }

        private void GetApiKey_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            if (button != null && button.Tag != null)
            {
                string? url = button.Tag.ToString();
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Could not open URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Complete Settings

        private void LoadSummarySettings()
        {
            // Get settings from config
            string sourceLanguage = SourceLanguageComboBox.SelectedItem.ToString() ?? "en";
            string targetLanguage = TargetLanguageComboBox.SelectedItem.ToString() ?? "vi";
            string ocrMethod = OcrMethodComboBox.SelectedItem.ToString() ?? "Windows OCR";
            string translationService = TranslationServiceComboBox.SelectedItem.ToString() ?? "ChatGPT";

            // Format language display
            string sourceFormatted = char.ToUpper(sourceLanguage[0]) + sourceLanguage.Substring(1);
            string targetFormatted = char.ToUpper(targetLanguage[0]) + targetLanguage.Substring(1);
            LanguagesSummaryText.Text = $"{sourceFormatted} → {targetFormatted}";

            // Format OCR method display
            switch (ocrMethod.ToLower())
            {
                case "windowsocr":
                    OcrMethodSummaryText.Text = "Windows OCR";
                    break;
                case "paddleocr":
                    OcrMethodSummaryText.Text = "PaddleOCR";
                    break;
                case "oneocr":
                    OcrMethodSummaryText.Text = "OneOCR";
                    break;
                default:
                    OcrMethodSummaryText.Text = "Windows OCR";
                    break;
            }

            // Format translation service display
            switch (translationService.ToLower())
            {
                case "chatgpt":
                    TranslationServiceSummaryText.Text = "ChatGPT";
                    break;
                default:
                    TranslationServiceSummaryText.Text = "ChatGPT";
                    break;
            }
        }


        #endregion

        #region Navigation

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep < TotalSteps)
            {
                NavigateToStep(currentStep + 1);
            }
            else
            {
                // Final step - show summary
                NavigateToStep(TotalSteps + 1);
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (FinishButton.Visibility == Visibility.Visible)
            {
                FinishButton.Visibility = Visibility.Collapsed;
                NextButton.Visibility = Visibility.Visible;
            }
            if (currentStep > 1)
            {
                NavigateToStep(currentStep - 1);
            }
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            // Save setting to not show quickstart again if checked
            if (DontShowAgainCheckBox.IsChecked == true)
            {
                configManager.SetNeedShowQuickStart(false);
            }

            this.Close();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            // Save setting to not show quickstart again if checked
            if (DontShowAgainCheckBox.IsChecked == true)
            {
                configManager.SetNeedShowQuickStart(false);
            }

            // Save setting language
            configManager.SetSourceLanguage(SourceLanguageComboBox.SelectedItem.ToString() ?? "en");
            configManager.SetTargetLanguage(TargetLanguageComboBox.SelectedItem.ToString() ?? "vi");

            // Save setting OCR method
            configManager.SetOcrMethod(OcrMethodComboBox.SelectedItem.ToString() ?? "Windows OCR");
            MainWindow.Instance.SetOcrMethod(OcrMethodComboBox.SelectedItem.ToString() ?? "Windows OCR");

            // Save setting translation services
            configManager.SetTranslationService(TranslationServiceComboBox.SelectedItem.ToString() ?? "ChatGPT");

            // No model selection needed for endpoint-based services

            this.Close();
        }

        private void DontShowAgainCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Save setting to not show quickstart again if checked
            configManager.SetNeedShowQuickStart(false);
            Console.WriteLine($"Popup quickstart will not show again");
        }


        #endregion
    }
}