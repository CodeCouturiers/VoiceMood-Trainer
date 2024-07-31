using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using NAudio.Wave;

namespace VoiceMood_Trainer {
public partial class MainWindow : Window {
    private JObject ravdessData = new JObject();
    private List<JObject> files_to_play = new List<JObject>();
    private int currentFileIndex;
    private int correctAnswers;
    private int incorrectAnswers;
    private Random random = new Random();
    private bool isTestRunning;
    private string? currentCorrectEmotion;
    // New fields for statistics
    private int totalFiles;
    private int loadedFiles;
    private int numberOfActors;
    private int numberOfEmotions;
    private string? selectedPresetKey;

    public MainWindow() {
        InitializeComponent();
        LoadRavdessData();
    }

    private void LoadRavdessData() {
        string jsonText = File.ReadAllText("ravdess_data.json");
        ravdessData = JObject.Parse(jsonText);

        // Обновляем статистику
        totalFiles = ravdessData["actors"]
                     .Children()
                     .Sum(actor => actor.First.Children<JObject>().Count());
        numberOfActors = ravdessData["actors"]
                         .Count();
        numberOfEmotions = ravdessData["presets"]
                           ["all_emotions"]
                           .Count();

        // Добавляем пресеты в код
        ravdessData["presets"] = new JObject {
            // Пресеты, которые уже были
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
            // Новые пресеты
            {"calm_and_tension", new JArray("calm", "fearful", "angry")},
            {"surprise_and_disgust", new JArray("surprised", "disgust")},
            {"happy_and_sad", new JArray("happy", "sad")},

            // Новые пресеты для распознавания лжи
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


    // Обработчик события для ComboBox
    private void PresetComboBox_SelectionChanged(object sender,
            SelectionChangedEventArgs e) {
        var selectedItem = (ComboBoxItem)e.AddedItems[0];
        selectedPresetKey = (string)selectedItem.Tag;
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
        if (isTestRunning && currentFileIndex < files_to_play.Count) {
            PlayAudioFile(files_to_play[currentFileIndex]
                          ["path"]!.ToString());
            EmotionOptions.IsEnabled = true;
        }
    }

    private void SetupTest(string presetKey) {
        var emotionsToTest = ravdessData["presets"]
                             [presetKey]
                             .ToObject<List<string>>();
        files_to_play = new List<JObject>();

        foreach (var actor in ravdessData["actors"]
                 .Children()) {
            var actorFiles =
                actor.First.Children<JObject>()
                .Where(file => emotionsToTest.Contains(file["emotion"]
                        .ToString()))
                .ToList();
            files_to_play.AddRange(actorFiles);
        }

        currentFileIndex = 0;
        correctAnswers = 0;
        CorrectAnswersText.Text = "0";
        IncorrectAnswersText.Text = "0";

        // Перемешиваем файлы
        files_to_play =
            files_to_play.OrderBy(x => random.Next()).Take(100).ToList();

        UpdateEmotionButtons(emotionsToTest);

        PlayNextAudio();
    }

    private void UpdateEmotionButtons(List<string> emotions) {
        EmotionOptions.Children.Clear();
        var emotionTranslations = new Dictionary<string, string> {
            {"neutral", "🤐 Нейтральная"}, {"calm", "😌 Спокойная"},
            {"happy", "😊 Радостная"},     {"sad", "😔 Грустная"},
            {"angry", "😠 Злая"},          {"fearful", "😨 Испуганная"},
            {"disgust", "🤢 Отвращение"},  {"surprised", "😲 Удивленная"}
        };
        // Создаем кнопки для каждой эмоции
        foreach (var emotion in emotions) {
            var translatedEmotion = emotionTranslations.ContainsKey(emotion)
                                    ? emotionTranslations[emotion]
                                    : emotion;  // Перевод, если есть
            var button = new Button {
                Content = translatedEmotion,
                Tag = emotion,
                Height = 50,  // Устанавливаем высоту кнопки
                Width = 150,  // Устанавливаем ширину кнопки
                Margin = new Thickness(5)  // Добавляем отступ
            };
            button.Click += EmotionButton_Click;
            EmotionOptions.Children.Add(button);
        }
    }

    private void PlayNextAudio() {
        if (!isTestRunning) return;

        if (currentFileIndex < files_to_play.Count) {
            var currentFile = files_to_play[currentFileIndex];
            string filePath = currentFile["path"]!.ToString();
            currentCorrectEmotion = currentFile["emotion"]?
                                    .ToString();

            // Play audio
            PlayAudioFile(filePath);

            // Обновляем кнопки с эмоциями
            var currentEmotions = files_to_play
                                  .Select(f => f["emotion"]
                                          .ToString())
                                  .Distinct()
                                  .ToList();
            UpdateEmotionButtons(currentEmotions);

            RepeatButton.IsEnabled = true;  // Enable the repeat button
            EmotionOptions.IsEnabled = true;  // Активировать кнопки выбора эмоций
        } else {
            // Test completed
            isTestRunning = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            NextButton.IsEnabled = false;
            RepeatButton.IsEnabled = false;  // Disable the repeat button
            StatusText.Text = "Тест завершен";
            MessageBox.Show(
                $"Тест завершен! Правильных ответов: {correctAnswers} из {files_to_play.Count}");
        }
    }

    private void PlayAudioFile(string filePath) {
        using (var audioFile = new AudioFileReader(filePath)) using (
                var outputDevice = new WaveOutEvent()) {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing) {
                    System.Threading.Thread.Sleep(100);
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

        if (emotionTranslations.ContainsKey(emotion)) {
            return emotionTranslations[emotion];
        } else {
            return emotion;
        }
    }

    private void EmotionButton_Click(object sender, RoutedEventArgs e) {
        var selectedEmotion = ((Button)sender).Tag.ToString();
        var correctEmotion = files_to_play[currentFileIndex]
                             ["emotion"]
                             .ToString();

        if (selectedEmotion == correctEmotion) {
            correctAnswers++;
            CorrectAnswersText.Text = correctAnswers.ToString();
            FeedbackText.Text = "✅ Правильно!";
            FeedbackText.Foreground = Brushes.Green;
        } else {
            incorrectAnswers++;
            IncorrectAnswersText.Text = incorrectAnswers.ToString();
            FeedbackText.Text =
                $"❌ Неправильно! Верный ответ: {GetTranslatedEmotion(correctEmotion)}";  // Перевод эмоции
            FeedbackText.Foreground = Brushes.Red;
        }

        ScoreText.Text = $"Счет: {correctAnswers}/{files_to_play.Count}";
        ProgressBar.Value =
            (double)(currentFileIndex + 1) / files_to_play.Count * 100;

        EmotionOptions.IsEnabled = false;
        NextButton.IsEnabled = true;
    }
}
}