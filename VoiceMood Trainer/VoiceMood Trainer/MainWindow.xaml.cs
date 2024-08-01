using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using NAudio.Wave;


namespace VoiceMood_Trainer {

public partial class MainWindow : Window {
    private JObject ravdessData = new JObject();
    private List<JObject> selectedAudioFiles = new List<JObject>();
    private int currentFileIndex;
    private int correctAnswers;
    private int incorrectAnswers;
    private Random random = new Random();
    private bool isTestRunning;
    private string? currentCorrectEmotion;
    // New fields for statistics
    private int totalFiles;
    private int loadedFiles = 0;
    private int numberOfActors;
    private int numberOfEmotions;
    private string? selectedPresetKey;
    private float speedUpFactor = 1.0f;

    public MainWindow() {
        InitializeComponent();
        LoadRavdessData();

        // Инициализация слайдера скорости и текстового поля
        if (SpeedSlider != null) {
            SpeedSlider.Value = 10; // Начальное значение 1.0x
        }
        if (SpeedValueText != null) {
            SpeedValueText.Text = "1.0x";
        }
    }
    private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
        speedUpFactor = (float)e.NewValue / 10f;
        if (SpeedValueText != null) {
            SpeedValueText.Text = $"{speedUpFactor:F1}x";
        }
    }

    private void LoadRavdessData() {
        string jsonText = File.ReadAllText("ravdess_data.json");
        ravdessData = JObject.Parse(jsonText);

        // Update statistics
        totalFiles = ravdessData["actors"]?.Children()
                     .Sum(actor => actor.First?.Children<JObject>().Count() ?? 0) ?? 0;
        numberOfActors = ravdessData["actors"]?.Count() ?? 0;
        numberOfEmotions = ravdessData["presets"]?["all_emotions"]?.Count() ?? 0;

        loadedFiles = totalFiles; // Assuming all files are loaded

        // Add presets to the code
        ravdessData["presets"] = new JObject {
            // Existing presets
            {
                "all_emotions", new JArray("neutral", "calm", "happy", "sad", "angry",
                                           "fearful", "disgust", "surprised")
            },
            {"negative_emotions", new JArray("sad", "angry", "fearful", "disgust")},
            {"positive_emotions", new JArray("happy", "calm")},
            {"basic_emotions", new JArray("happy", "sad", "angry", "fearful")},
            {
                "neutral_and_extreme",
                new JArray("neutral", "happy", "angry", "fearful")
            },
            // New presets
            {"calm_and_tension", new JArray("calm", "fearful", "angry")},
            {"surprise_and_disgust", new JArray("surprised", "disgust")},
            {"happy_and_sad", new JArray("happy", "sad")},

            // New presets for lie detection
            {
                "denial_of_involvement",
                new JArray("fearful", "angry", "disgust", "neutral")
            },
            {
                "covering_for_an_accomplice",
                new JArray("calm", "neutral", "fearful", "surprised")
            },
            {
                "justification_of_actions",
                new JArray("sad", "angry", "calm", "neutral")
            },
            {"distraction", new JArray("calm", "neutral", "surprised", "happy")},
            {
                "disagreement_with_facts",
                new JArray("angry", "fearful", "calm", "neutral")
            },
            {"shifting_blame", new JArray("angry", "fearful", "sad", "neutral")},
            {"pretending_illness", new JArray("sad", "fearful", "neutral")},
            {"playing_on_sympathy", new JArray("sad", "fearful", "calm")}
        };

        UpdateStatistics();
    }

    private void UpdateStatistics() {
        LoadedFilesText.Text = $"Загружено файлов: {loadedFiles}";
        ActorsCountText.Text = $"Количество актеров: {numberOfActors}";
        EmotionsCountText.Text = $"Количество эмоций: {numberOfEmotions}";
        TotalFilesText.Text = $"Общее количество файлов: {totalFiles}";
    }

    // Event handler for ComboBox
    private void PresetComboBox_SelectionChanged(object sender,
            SelectionChangedEventArgs e) {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem selectedItem) {
            selectedPresetKey = selectedItem.Tag as string;
        }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e) {
        if (selectedPresetKey != null) {
            SetupTest(selectedPresetKey);
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            NextButton.IsEnabled = false;
            RepeatButton.IsEnabled = false;
            StatusText.Text = "Тест начался";
        } else {
            MessageBox.Show("Пожалуйста, выберите пресет.");
        }

        isTestRunning = true;
        PlayNextAudio();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e) {
        isTestRunning = false;
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
        NextButton.IsEnabled = false;
        RepeatButton.IsEnabled = false;
        StatusText.Text = "Тест остановлен";
    }

    private void NextButton_Click(object sender, RoutedEventArgs e) {
        NextButton.IsEnabled = false;
        EmotionOptions.IsEnabled = true;
        FeedbackText.Text = "";
        currentFileIndex++;
        PlayNextAudio();
    }

    private void RepeatButton_Click(object sender, RoutedEventArgs e) {
        if (isTestRunning && currentFileIndex < selectedAudioFiles.Count) {
            PlayAudioFile(selectedAudioFiles[currentFileIndex]
                          ["path"]?.ToString() ?? "");
            EmotionOptions.IsEnabled = true;
        }
    }

    private void SetupTest(string presetKey) {
        var emotionsToTest = ravdessData["presets"]?[presetKey]?.ToObject<List<string>>() ?? new List<string>();
        selectedAudioFiles = new List<JObject>();

        foreach (var actor in ravdessData["actors"]?.Children() ?? Enumerable.Empty<JToken>()) {
            var actorFiles = actor.First?.Children<JObject>()
                             .Where(file => file["emotion"] != null && emotionsToTest.Contains(file["emotion"].ToString()))
                             .ToList() ?? new List<JObject>();
            selectedAudioFiles.AddRange(actorFiles);
        }

        currentFileIndex = 0;
        correctAnswers = 0;
        CorrectAnswersText.Text = "0";
        IncorrectAnswersText.Text = "0";

        // Shuffle files
        selectedAudioFiles = selectedAudioFiles.OrderBy(x => random.Next()).Take(100).ToList();

        UpdateEmotionButtons(emotionsToTest);

        PlayNextAudio();
    }

    private void UpdateEmotionButtons(List<string> emotions) {
        EmotionOptions.Children.Clear();

        foreach (var emotion in emotions) {
            try {
                var (text, svgPath, color) = EmotionResourcesManager.GetEmotionTranslation(emotion);

                var button = new Button {
                    Height = 50,
                    Width = 150,
                    Margin = new Thickness(5),
                    Tag = emotion
                };

                var stackPanel = new StackPanel {
                    Orientation = Orientation.Horizontal
                };

                var path = new System.Windows.Shapes.Path {
                    Width = 24,
                    Height = 24,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 5, 0)
                };

                try {
                    path.Data = Geometry.Parse(svgPath);
                } catch (System.FormatException ex) {
                    // Логгирование или вывод ошибки в интерфейс
                    System.Diagnostics.Debug.WriteLine($"Ошибка парсинга SVG-пути для эмоции {emotion}: {ex.Message}");
                    continue; // Пропустить текущую итерацию
                }

                // Проверяем, что цвет корректно преобразуется в System.Windows.Media.Color
                System.Windows.Media.Color buttonColor;
                if (!TryGetColor(color, out buttonColor)) {
                    // Логгирование или вывод ошибки в интерфейс
                    System.Diagnostics.Debug.WriteLine($"Ошибка преобразования цвета для эмоции {emotion}: {color}");
                    continue; // Пропустить текущую итерацию
                }

                path.Fill = new SolidColorBrush(buttonColor);

                var textBlock = new TextBlock {
                    Text = text,
                    VerticalAlignment = VerticalAlignment.Center
                };

                stackPanel.Children.Add(path);
                stackPanel.Children.Add(textBlock);

                button.Content = stackPanel;
                button.Click += EmotionButton_Click;
                EmotionOptions.Children.Add(button);
            } catch (Exception ex) {
                // Логгирование или вывод ошибки в интерфейс
                System.Diagnostics.Debug.WriteLine($"Ошибка создания кнопки для эмоции {emotion}: {ex.Message}");
            }
        }
    }

    private bool TryGetColor(System.Drawing.Color color, out System.Windows.Media.Color windowsColor) {
        try {
            windowsColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            return true;
        } catch (Exception) {
            windowsColor = System.Windows.Media.Colors.Transparent;
            return false;
        }
    }


    private async void PlayNextAudio() {
        if (!isTestRunning) return;

        if (currentFileIndex < selectedAudioFiles.Count) {
            var currentFile = selectedAudioFiles[currentFileIndex];
            string filePath = currentFile["path"]?.ToString() ?? "";
            currentCorrectEmotion = currentFile["emotion"]?.ToString();

            // Play audio asynchronously
            await Task.Run(() => PlayAudioFile(filePath));

            // Update emotion buttons
            var currentEmotions = selectedAudioFiles
                                  .Select(f => f["emotion"]?.ToString() ?? "")
                                  .Distinct()
                                  .ToList();
            UpdateEmotionButtons(currentEmotions);

            RepeatButton.IsEnabled = true;  // Enable the repeat button
            EmotionOptions.IsEnabled = true;  // Activate emotion selection buttons
        } else {
            // Test completed
            isTestRunning = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            NextButton.IsEnabled = false;
            RepeatButton.IsEnabled = false;  // Disable the repeat button
            StatusText.Text = "Тест завершен";
            MessageBox.Show($"Тест завершен! Правильных ответов: {correctAnswers} из {selectedAudioFiles.Count}");
        }
    }

    private async void PlayAudioFile(string filePath) {
        if (string.IsNullOrEmpty(filePath)) return;

        using (var audioFile = new AudioFileReader(filePath))
            using (var speedUpProvider = new SpeedUpWaveProvider(audioFile, speedUpFactor))
                using (var outputDevice = new WaveOutEvent()) {
                    outputDevice.Init(speedUpProvider);
                    outputDevice.Play();

                    while (outputDevice.PlaybackState == PlaybackState.Playing) {
                        await Task.Delay(100);
                    }
                }
    }


    private string GetTranslatedEmotion(string emotion) {
        var emotionTranslations = new Dictionary<string, string> {
            {"neutral", "Нейтральная"}, {"calm", "Спокойная"},
            {"happy", "Радостная"},     {"sad", "Грустная"},
            {"angry", "Злая"},          {"fearful", "Испуганная"},
            {"disgust", "Отвращение"},  {"surprised", "Удивленная"}
        };

        return emotionTranslations.TryGetValue(emotion, out var translatedEmotion) ? translatedEmotion : emotion;
    }

    private void EmotionButton_Click(object sender, RoutedEventArgs e) {
        var selectedEmotion = ((Button)sender).Tag?.ToString();
        var correctEmotion = selectedAudioFiles[currentFileIndex]["emotion"]?.ToString();

        if (selectedEmotion == correctEmotion) {
            correctAnswers++;
            CorrectAnswersText.Text = correctAnswers.ToString();
            FeedbackText.Text = "✅ Правильно!";
            FeedbackText.Foreground = System.Windows.Media.Brushes.Green;
        } else {
            incorrectAnswers++;
            IncorrectAnswersText.Text = incorrectAnswers.ToString();
            FeedbackText.Text = $"❌ Неправильно! Верный ответ: {GetTranslatedEmotion(correctEmotion ?? "")}";
            FeedbackText.Foreground = System.Windows.Media.Brushes.Red;
        }

        ScoreText.Text = $"Счет: {correctAnswers}/{selectedAudioFiles.Count}";
        ProgressBar.Value = (double)(currentFileIndex + 1) / selectedAudioFiles.Count * 100;

        EmotionOptions.IsEnabled = false;
        NextButton.IsEnabled = true;
    }
}
}