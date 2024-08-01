using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using NAudio.Wave;

namespace VoiceMood_Trainer
{
    public partial class MainWindow : Window
    {
        private JObject ravdessData = new JObject();
        private List<JObject> selectedAudioFiles = new List<JObject>();
        private int currentFileIndex;
        private int correctAnswers;
        private int incorrectAnswers;
        private Random random = new Random();
        private bool isTestRunning;
        private string? currentCorrectEmotion;
        private int totalFiles;
        private int loadedFiles = 0;
        private int numberOfActors;
        private int numberOfEmotions;
        private string? selectedPresetKey;
        private float speedUpFactor = 1.0f;

        public MainWindow()
        {
            InitializeComponent();
            LoadRavdessData();
            foreach (var lang in LocalizationManager.Instance.GetAvailableLanguages())
            {
                LanguageComboBox.Items.Add(lang);
            }
            LanguageComboBox.SelectedItem = "en";
            UpdateUITexts();
            if (SpeedSlider != null)
            {
                SpeedSlider.Value = 10;
            }
            if (SpeedValueText != null)
            {
                SpeedValueText.Text = "1.0x";
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is string selectedLang)
            {
                LocalizationManager.Instance.SetLanguage(selectedLang);
                UpdateUITexts();
            }
        }

        private void UpdateUITexts()
        {
            Title = LocalizationManager.Instance.GetString("WindowTitle");
            LoadedFilesText.Text = string.Format(LocalizationManager.Instance.GetString("LoadedFiles"), loadedFiles);
            ActorsCountText.Text = string.Format(LocalizationManager.Instance.GetString("ActorsCount"), numberOfActors);
            EmotionsCountText.Text = string.Format(LocalizationManager.Instance.GetString("EmotionsCount"), numberOfEmotions);
            TotalFilesText.Text = string.Format(LocalizationManager.Instance.GetString("TotalFiles"), totalFiles);
            StartButton.Content = LocalizationManager.Instance.GetString("Start");
            StopButton.Content = LocalizationManager.Instance.GetString("Stop");
            NextButton.Content = LocalizationManager.Instance.GetString("Next");
            RepeatButton.Content = LocalizationManager.Instance.GetString("Repeat");

            var settingsTextBlock = FindName("SettingsText") as TextBlock;
            if (settingsTextBlock != null)
            {
                settingsTextBlock.Text = LocalizationManager.Instance.GetString("Settings");
            }

            var volumeTextBlock = FindName("VolumeText") as TextBlock;
            if (volumeTextBlock != null)
            {
                volumeTextBlock.Text = LocalizationManager.Instance.GetString("Volume");
            }

            var playbackSpeedTextBlock = FindName("PlaybackSpeedText") as TextBlock;
            if (playbackSpeedTextBlock != null)
            {
                playbackSpeedTextBlock.Text = LocalizationManager.Instance.GetString("PlaybackSpeed");
            }

            StatusText.Text = LocalizationManager.Instance.GetString("ReadyToStart");
            ScoreText.Text = LocalizationManager.Instance.GetString("Score");

            var correctTextBlock = FindName("CorrectText") as TextBlock;
            if (correctTextBlock != null)
            {
                correctTextBlock.Text = LocalizationManager.Instance.GetString("Correct");
            }

            var incorrectTextBlock = FindName("IncorrectText") as TextBlock;
            if (incorrectTextBlock != null)
            {
                incorrectTextBlock.Text = LocalizationManager.Instance.GetString("Incorrect");
            }

            foreach (ComboBoxItem item in PresetComboBox.Items)
            {
                item.Content = LocalizationManager.Instance.GetString(item.Tag.ToString());
            }

            var statisticsHeaderTextBlock = FindName("StatisticsHeader") as TextBlock;
            if (statisticsHeaderTextBlock != null)
            {
                statisticsHeaderTextBlock.Text = LocalizationManager.Instance.GetString("Statistics");
            }

            if (selectedAudioFiles != null && selectedAudioFiles.Any())
            {
                UpdateEmotionButtons(selectedAudioFiles
                                     .Select(f => f["emotion"]?.ToString() ?? "")
                                     .Distinct()
                                     .ToList());
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speedUpFactor = (float)e.NewValue / 10f;
            if (SpeedValueText != null)
            {
                SpeedValueText.Text = $"{speedUpFactor:F1}x";
            }
        }

        private void LoadRavdessData()
        {
            string jsonText = File.ReadAllText("ravdess_data.json");
            ravdessData = JObject.Parse(jsonText);

            totalFiles = ravdessData["actors"]?.Children()
                         .Sum(actor => actor.First?.Children<JObject>().Count() ?? 0) ?? 0;
            numberOfActors = ravdessData["actors"]?.Count() ?? 0;
            numberOfEmotions = ravdessData["presets"]?["all_emotions"]?.Count() ?? 0;

            loadedFiles = totalFiles;

            ravdessData["presets"] = new JObject
            {
                {"all_emotions", new JArray("neutral", "calm", "happy", "sad", "angry", "fearful", "disgust", "surprised")},
                {"negative_emotions", new JArray("sad", "angry", "fearful", "disgust")},
                {"positive_emotions", new JArray("happy", "calm")},
                {"basic_emotions", new JArray("happy", "sad", "angry", "fearful")},
                {"neutral_and_extreme", new JArray("neutral", "happy", "angry", "fearful")},
                {"calm_and_tension", new JArray("calm", "fearful", "angry")},
                {"surprise_and_disgust", new JArray("surprised", "disgust")},
                {"happy_and_sad", new JArray("happy", "sad")},
                {"denial_of_involvement", new JArray("fearful", "angry", "disgust", "neutral")},
                {"covering_for_an_accomplice", new JArray("calm", "neutral", "fearful", "surprised")},
                {"justification_of_actions", new JArray("sad", "angry", "calm", "neutral")},
                {"distraction", new JArray("calm", "neutral", "surprised", "happy")},
                {"disagreement_with_facts", new JArray("angry", "fearful", "calm", "neutral")},
                {"shifting_blame", new JArray("angry", "fearful", "sad", "neutral")},
                {"pretending_illness", new JArray("sad", "fearful", "neutral")},
                {"playing_on_sympathy", new JArray("sad", "fearful", "calm")}
            };

            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            LoadedFilesText.Text = string.Format(LocalizationManager.Instance.GetString("LoadedFiles"), loadedFiles);
            ActorsCountText.Text = string.Format(LocalizationManager.Instance.GetString("ActorsCount"), numberOfActors);
            EmotionsCountText.Text = string.Format(LocalizationManager.Instance.GetString("EmotionsCount"), numberOfEmotions);
            TotalFilesText.Text = string.Format(LocalizationManager.Instance.GetString("TotalFiles"), totalFiles);
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem selectedItem)
            {
                selectedPresetKey = selectedItem.Tag as string;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPresetKey != null)
            {
                SetupTest(selectedPresetKey);
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                NextButton.IsEnabled = false;
                RepeatButton.IsEnabled = false;
                StatusText.Text = LocalizationManager.Instance.GetString("TestStarted");
            }
            else
            {
                MessageBox.Show(LocalizationManager.Instance.GetString("PleaseSelectPreset"));
            }

            isTestRunning = true;
            PlayNextAudio();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            isTestRunning = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            NextButton.IsEnabled = false;
            RepeatButton.IsEnabled = false;
            StatusText.Text = LocalizationManager.Instance.GetString("TestStopped");
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextButton.IsEnabled = false;
            EmotionOptions.IsEnabled = true;
            FeedbackText.Text = "";
            currentFileIndex++;
            PlayNextAudio();
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (isTestRunning && currentFileIndex < selectedAudioFiles.Count)
            {
                PlayAudioFile(selectedAudioFiles[currentFileIndex]["path"]?.ToString() ?? "");
                EmotionOptions.IsEnabled = true;
            }
        }

        private void SetupTest(string presetKey)
        {
            var emotionsToTest = ravdessData["presets"]?[presetKey]?.ToObject<List<string>>() ?? new List<string>();
            selectedAudioFiles = new List<JObject>();

            foreach (var actor in ravdessData["actors"]?.Children() ?? Enumerable.Empty<JToken>())
            {
                var actorFiles = actor.First?.Children<JObject>()
                                 .Where(file => file["emotion"] != null && emotionsToTest.Contains(file["emotion"].ToString()))
                                 .ToList() ?? new List<JObject>();
                selectedAudioFiles.AddRange(actorFiles);
            }

            currentFileIndex = 0;
            correctAnswers = 0;
            CorrectAnswersText.Text = "0";
            IncorrectAnswersText.Text = "0";

            selectedAudioFiles = selectedAudioFiles.OrderBy(x => random.Next()).Take(100).ToList();

            UpdateEmotionButtons(emotionsToTest);

            PlayNextAudio();
        }

        private void UpdateEmotionButtons(List<string> emotions)
        {
            EmotionOptions.Children.Clear();

            foreach (var emotion in emotions)
            {
                try
                {
                    var(text, svgPath, color) = EmotionResourcesManager.GetEmotionTranslation(emotion);

                    var button = new Button
                    {
                        Height = 50,
                        Width = 150,
                        Margin = new Thickness(5),
                        Tag = emotion
                    };

                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    var path = new System.Windows.Shapes.Path
                    {
                        Width = 24,
                        Height = 24,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(0, 0, 5, 0)
                    };

                    try
                    {
                        path.Data = Geometry.Parse(svgPath);
                    }
                    catch (FormatException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error parsing SVG path for emotion {emotion}: {ex.Message}");
                        continue;
                    }

                    System.Windows.Media.Color buttonColor;
                    if (!TryGetColor(color, out buttonColor))
                    {
                        System.Diagnostics.Debug.WriteLine($"Error converting color for emotion {emotion}: {color}");
                        continue;
                    }

                    path.Fill = new SolidColorBrush(buttonColor);

                    var textBlock = new TextBlock
                    {
                        Text = LocalizationManager.Instance.GetString(emotion),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    stackPanel.Children.Add(path);
                    stackPanel.Children.Add(textBlock);

                    button.Content = stackPanel;
                    button.Click += EmotionButton_Click;
                    EmotionOptions.Children.Add(button);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating button for emotion {emotion}: {ex.Message}");
                }
            }
        }

        private bool TryGetColor(System.Drawing.Color color, out System.Windows.Media.Color windowsColor)
        {
            try
            {
                windowsColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                return true;
            }
            catch (Exception)
            {
                windowsColor = System.Windows.Media.Colors.Transparent;
                return false;
            }
        }

        private async void PlayNextAudio()
        {
            if (!isTestRunning) { return; }

            if (currentFileIndex < selectedAudioFiles.Count)
            {
                var currentFile = selectedAudioFiles[currentFileIndex];
                string filePath = currentFile["path"]?.ToString() ?? "";
                currentCorrectEmotion = currentFile["emotion"]?.ToString();

                await Task.Run(() => PlayAudioFile(filePath));

                var currentEmotions = selectedAudioFiles
                                      .Select(f => f["emotion"]?.ToString() ?? "")
                                      .Distinct()
                                      .ToList();
                UpdateEmotionButtons(currentEmotions);

                RepeatButton.IsEnabled = true;
                EmotionOptions.IsEnabled = true;
            }
            else
            {
                isTestRunning = false;
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                NextButton.IsEnabled = false;
                RepeatButton.IsEnabled = false;
                StatusText.Text = LocalizationManager.Instance.GetString("TestCompleted");
                MessageBox.Show(string.Format(LocalizationManager.Instance.GetString("TestCompletedMessage"), correctAnswers,
                                              selectedAudioFiles.Count));
            }
        }

        private async void PlayAudioFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) { return; }

            using (var audioFile = new AudioFileReader(filePath))
                using (var speedUpProvider = new SpeedUpWaveProvider(audioFile, speedUpFactor))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        outputDevice.Init(speedUpProvider);
                        outputDevice.Play();

                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            await Task.Delay(100);
                        }
                    }
        }

        private string GetTranslatedEmotion(string emotion)
        {
            return LocalizationManager.Instance.GetString(emotion);
        }

        private void EmotionButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmotion = ((Button)sender).Tag?.ToString();
            var correctEmotion = selectedAudioFiles[currentFileIndex]["emotion"]?.ToString();

            if (selectedEmotion == correctEmotion)
            {
                correctAnswers++;
                CorrectAnswersText.Text = correctAnswers.ToString();
                FeedbackText.Text = LocalizationManager.Instance.GetString("CorrectAnswer");
                FeedbackText.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                incorrectAnswers++;
                IncorrectAnswersText.Text = incorrectAnswers.ToString();
                FeedbackText.Text = string.Format(LocalizationManager.Instance.GetString("IncorrectAnswerMessage"),
                                                  GetTranslatedEmotion(correctEmotion ?? ""));
                FeedbackText.Foreground = System.Windows.Media.Brushes.Red;
            }

            ScoreText.Text = string.Format(LocalizationManager.Instance.GetString("ScoreFormat"), correctAnswers,
                                           selectedAudioFiles.Count);
            ProgressBar.Value = (double)(currentFileIndex + 1) / selectedAudioFiles.Count * 100;

            EmotionOptions.IsEnabled = false;
            NextButton.IsEnabled = true;
        }

        private void ProgressionModeButton_Click(object sender, RoutedEventArgs e)
        {
            var progressionModeWindow = new ProgressionModeWindow(this);
            progressionModeWindow.ShowDialog();
        }

        public List<string> GetAllEmotions()
        {
            return ravdessData["presets"]?["all_emotions"]?.ToObject<List<string>>() ?? new List<string>();
        }

        public void SetupProgressionTest(List<string> emotions)
        {
            selectedAudioFiles = new List<JObject>();

            foreach (var actor in ravdessData["actors"]?.Children() ?? Enumerable.Empty<JToken>())
            {
                var actorFiles = actor.First?.Children<JObject>()
                                 .Where(file => file["emotion"] != null && emotions.Contains(file["emotion"].ToString()))
                                 .ToList() ?? new List<JObject>();
                selectedAudioFiles.AddRange(actorFiles);
            }

            currentFileIndex = 0;
            correctAnswers = 0;
            incorrectAnswers = 0;
            CorrectAnswersText.Text = "0";
            IncorrectAnswersText.Text = "0";

            selectedAudioFiles = selectedAudioFiles.OrderBy(x => random.Next()).Take(100).ToList();

            UpdateEmotionButtons(emotions);

            isTestRunning = true;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            NextButton.IsEnabled = false;
            RepeatButton.IsEnabled = false;
            StatusText.Text = LocalizationManager.Instance.GetString("TestStarted");

            PlayNextAudio();
        }
    }
}