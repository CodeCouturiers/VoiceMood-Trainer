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
        var emotionTranslations = new Dictionary<string, (string text, string svgPath, Color color)> {
            {"neutral", ("Нейтральная", "M15.999 29.998c9.334 0 13.999-6.268 13.999-14 0-7.73-4.665-13.998-14-13.998C6.665 2 2 8.268 2 15.999s4.664 13.999 13.999 13.999 M10.5 16a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9 M21.5 16a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9 M12.5 11.5a2 2 0 1 1-4 0 2 2 0 0 1 4 0 M23.5 11.5a2 2 0 1 1-4 0 2 2 0 0 1 4 0 M8.25 18C7.56 18 7 18.56 7 19.25V22h2.5v.75a1.25 1.25 0 1 0 2.5 0V22h2.5v.75a1.25 1.25 0 1 0 2.5 0V22h2.5v.75a1.25 1.25 0 1 0 2.5 0V22h1.314a2.11 2.11 0 0 0 .196 2.483l2.419 2.789a2.107 2.107 0 0 0 3.077.117l.382-.382c.86-.86.807-2.28-.116-3.076l-2.791-2.418a2.1 2.1 0 0 0-1.981-.424V19.25a1.25 1.25 0 1 0-2.5 0V20h-2.5v-.75a1.25 1.25 0 1 0-2.5 0V20h-2.5v-.75a1.25 1.25 0 1 0-2.5 0V20H9.5v-.75c0-.69-.56-1.25-1.25-1.25 M24.146 23.475a.934.934 0 0 1 0-1.315.936.936 0 0 1 1.316 0 .916.916 0 0 1 0 1.315.936.936 0 0 1-1.316 0", Color.FromArgb(255, 255, 176, 46))},
            {"calm", ("Спокойная", "M15.999 29.998c9.334 0 13.999-6.268 13.999-14 0-7.73-4.665-13.998-14-13.998C6.665 2 2 8.268 2 15.999s4.664 13.999 13.999 13.999 M6.974 10.658c.144-.43.502-1.108 1.095-1.67C8.654 8.433 9.452 8 10.5 8a.5.5 0 1 0 0-1c-1.352 0-2.387.567-3.12 1.262-.723.688-1.164 1.51-1.354 2.08a.5.5 0 1 0 .948.316 M25.026 10.658c-.144-.43-.502-1.108-1.095-1.67C23.346 8.433 22.548 8 21.5 8a.5.5 0 0 1 0-1c1.352 0 2.387.567 3.12 1.262.723.688 1.165 1.51 1.354 2.08a.5.5 0 0 1-.948.316 M7.707 16.293a1 1 0 0 0-1.414 1.414C6.818 18.232 8.14 19 10 19s3.182-.768 3.707-1.293a1 1 0 0 0-1.414-1.414C12.15 16.435 11.34 17 10 17s-2.15-.565-2.293-.707 M19.707 16.293a1 1 0 0 0-1.414 1.414C18.818 18.232 20.14 19 22 19s3.182-.768 3.707-1.293a1 1 0 0 0-1.414-1.414C24.15 16.435 23.34 17 22 17s-2.15-.565-2.293-.707 M11.8 23.4a1 1 0 0 0-1.6 1.2c.69.92 2.688 2.4 5.8 2.4s5.11-1.48 5.8-2.4a1 1 0 0 0-1.6-1.2c-.31.413-1.712 1.6-4.2 1.6s-3.89-1.187-4.2-1.6", Color.FromArgb(255, 255, 176, 46))},
            {"happy", ("Радостная", "M15.999 29.998c9.334 0 13.999-6.268 13.999-14 0-7.73-4.665-13.998-14-13.998C6.665 2 2 8.268 2 15.999s4.664 13.999 13.999 13.999 M11 16c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2 M27 16c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2 M8.982 12.19c.048-.246.158-.55.367-.777.18-.196.498-.413 1.15-.413.643 0 .97.222 1.158.429.218.24.323.545.358.742a1 1 0 0 0 1.97-.342 3.54 3.54 0 0 0-.85-1.747C12.563 9.452 11.696 9 10.5 9c-1.184 0-2.047.431-2.624 1.06-.548.596-.769 1.293-.858 1.75a1 1 0 1 0 1.964.38 M19.982 12.19c.048-.246.158-.55.367-.777.18-.196.498-.413 1.151-.413.642 0 .969.222 1.157.429.219.24.324.545.358.742a1 1 0 0 0 1.97-.342 3.54 3.54 0 0 0-.85-1.747C23.563 9.452 22.696 9 21.5 9c-1.184 0-2.047.431-2.624 1.06-.548.596-.769 1.293-.857 1.75a1 1 0 1 0 1.963.38 M9.4 18.2a1 1 0 0 1 1.4.2c.341.455 2.062 2.1 5.2 2.1s4.859-1.645 5.2-2.1a1 1 0 1 1 1.6 1.2c-.659.878-2.938 2.9-6.8 2.9s-6.141-2.022-6.8-2.9a1 1 0 0 1 .2-1.4", Color.FromArgb(255, 255, 176, 46))},
            {"sad", ("Грустная", "M15.999 29.998c9.334 0 13.999-6.268 13.999-14 0-7.73-4.665-13.998-14-13.998C6.665 2 2 8.268 2 15.999s4.664 13.999 13.999 13.999 M10.985 8.621a.5.5 0 0 0-.97-.242c-.225.9-.665 1.544-1.247 1.967-.585.425-1.35.654-2.268.654a.5.5 0 0 0 0 1c1.083 0 2.067-.271 2.857-.846.793-.577 1.353-1.433 1.628-2.533 M6.293 16.293a1 1 0 0 1 1.414 0C7.85 16.435 8.66 17 10 17s2.15-.565 2.293-.707a1 1 0 0 1 1.414 1.414C13.182 18.232 11.86 19 10 19s-3.182-.768-3.707-1.293a1 1 0 0 1 0-1.414 M18.293 16.293a1 1 0 0 1 1.414 0c.142.142.953.707 2.293.707s2.15-.565 2.293-.707a1 1 0 0 1 1.414 1.414C25.182 18.232 23.86 19 22 19s-3.182-.768-3.707-1.293a1 1 0 0 1 0-1.414 M21.379 8.015a.5.5 0 0 0-.364.606c.275 1.1.835 1.956 1.628 2.533.79.575 1.774.846 2.857.846a.5.5 0 1 0 0-1c-.917 0-1.683-.229-2.268-.654-.582-.423-1.022-1.067-1.247-1.967a.5.5 0 0 0-.606-.364 M13 24a1 1 0 1 0 0 2h6a1 1 0 1 0 0-2z", Color.FromArgb(255, 255, 176, 46))},
            {"angry", ("Злая", "M15.999 29.998c9.334 0 13.999-6.268 13.999-14 0-7.73-4.665-13.998-14-13.998C6.665 2 2 8.268 2 15.999s4.664 13.999 13.999 13.999 M10.5 21a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9 M21.5 21a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9 M14.29 12.501a.75.75 0 0 1-.08 1.498c-1.017-.054-1.989-.304-2.817-.88-.835-.582-1.46-1.452-1.854-2.631a.75.75 0 1 1 1.422-.476c.31.928.762 1.509 1.29 1.876.534.372 1.21.569 2.039.613 M17.71 12.501a.75.75 0 0 0 .08 1.498c1.017-.054 1.989-.304 2.817-.88.835-.582 1.46-1.452 1.854-2.631a.75.75 0 1 0-1.422-.476c-.31.928-.763 1.509-1.29 1.876-.534.372-1.21.569-2.039.613 M16 24c-2.005 0-2.934 1.104-3.106 1.447a1 1 0 1 1-1.789-.894C11.602 23.563 13.205 22 16 22s4.4 1.562 4.894 2.553a1 1 0 1 1-1.788.894C18.934 25.104 18.005 24 16 24 M14 17a2 2 0 1 1-4 0 2 2 0 0 1 4 0 M22 17a2 2 0 1 1-4 0 2 2 0 0 1 4 0", Color.FromArgb(255, 255, 176, 46))},
            {"fearful", ("Испуганная", "M29.998 15.999c0 7.731-4.665 13.999-14 13.999C6.665 29.998 2 23.73 2 15.998Q2 14.98 2.108 14l13.89-6 13.892 6q.108.98.108 1.999 M29.89 14c-.747-6.785-5.376-12-13.891-12S2.855 7.215 2.108 14z M10.5 18a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9 M21.5 18a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9 M8.952 7.2c.54-.475.875-1.127 1.065-1.83a.5.5 0 0 1 .966.26c-.225.831-.64 1.68-1.371 2.321C8.873 8.6 7.855 9 6.5 9a.5.5 0 1 1 0-1c1.145 0 1.92-.333 2.452-.8 M23.048 7.2c-.54-.475-.875-1.127-1.065-1.83a.5.5 0 0 0-.966.26c.225.831.64 1.68 1.371 2.321C23.127 8.6 24.145 9 25.5 9a.5.5 0 0 0 0-1c-1.145 0-1.92-.333-2.452-.8 M14 14a2 2 0 1 1-4 0 2 2 0 0 1 4 0 M22 14a2 2 0 1 1-4 0 2 2 0 0 1 4 0 M22.718 24.026c.31 1.06-.615 1.974-1.72 1.974h-9.997c-1.105 0-2.03-.914-1.719-1.974a7.003 7.003 0 0 1 13.436 0", Color.FromArgb(255, 255, 176, 46))},
            {"disgust", ("Отвращение", "M15.999 29.998c9.334 0 13.999-6.268 13.999-14 0-7.73-4.665-13.998-14-13.998C6.665 2 2 8.268 2 15.999s4.664 13.999 13.999 13.999 M10.5 19a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9 M21.5 19a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9 M10.988 5.61a.5.5 0 1 0-.976-.22c-.238 1.063-.801 1.7-1.447 2.082C7.904 7.864 7.133 8 6.5 8a.5.5 0 1 0 0 1c.757 0 1.718-.16 2.574-.667.872-.516 1.613-1.38 1.914-2.724 M21.012 5.61a.5.5 0 1 1 .976-.22c.238 1.063.801 1.7 1.447 2.082.661.392 1.432.528 2.065.528a.5.5 0 0 1 0 1c-.757 0-1.718-.16-2.574-.667-.872-.516-1.613-1.38-1.914-2.724 M13.903 21.005A1 1 0 0 1 14 21h4a1 1 0 0 1 .097.005c.2-.98.701-1.71 1.368-2.207C20.292 18.182 21.274 18 22 18a1 1 0 1 1 0 2c-.44 0-.959.118-1.34.402-.333.248-.66.69-.66 1.598s.327 1.35.66 1.598c.381.284.9.402 1.34.402a1 1 0 1 1 0 2c-.726 0-1.708-.182-2.535-.798-.666-.497-1.167-1.228-1.368-2.207A1 1 0 0 1 18 23h-4q-.05 0-.097-.005c-.2.98-.702 1.71-1.368 2.207-.827.616-1.809.798-2.535.798a1 1 0 1 1 0-2c.44 0 .959-.118 1.34-.402.333-.248.66-.69.66-1.598s-.327-1.35-.66-1.598C10.96 20.118 10.44 20 10 20a1 1 0 1 1 0-2c.726 0 1.708.182 2.535.798.666.497 1.167 1.228 1.368 2.207 M14 15a2 2 0 1 1-4 0 2 2 0 0 1 4 0 M22 15a2 2 0 1 1-4 0 2 2 0 0 1 4 0", Color.FromArgb(255, 0, 210, 106))},
            {"surprised", ("Удивленная", "M15.999 29.998c9.334 0 13.999-6.268 13.999-14 0-7.73-4.665-13.998-14-13.998C6.665 2 2 8.268 2 15.999s4.664 13.999 13.999 13.999 M10.5 19a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9 M21.5 19a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9 M8.07 7.988c-.594.562-.952 1.24-1.096 1.67a.5.5 0 1 1-.948-.316c.19-.57.63-1.392 1.355-2.08C8.113 6.567 9.148 6 10.5 6a.5.5 0 0 1 0 1c-1.048 0-1.846.433-2.43.988 M12 17a2 2 0 1 0 0-4 2 2 0 0 0 0 4 M20 17a2 2 0 1 0 0-4 2 2 0 0 0 0 4 M25.026 9.658c-.144-.43-.503-1.108-1.095-1.67C23.346 7.433 22.548 7 21.5 7a.5.5 0 1 1 0-1c1.352 0 2.387.567 3.12 1.262.723.688 1.164 1.51 1.354 2.08a.5.5 0 1 1-.948.316 M13.17 22c-.11.313-.17.65-.17 1v2a3 3 0 1 0 6 0v-2c0-.35-.06-.687-.17-1L16 21z M13.17 22a3.001 3.001 0 0 1 5.66 0z", Color.FromArgb(255, 255, 176, 46))}
        };

        foreach (var emotion in emotions) {
            if (emotionTranslations.TryGetValue(emotion, out var translationData)) {
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
                    Data = Geometry.Parse(translationData.svgPath),
                    Fill = new SolidColorBrush(translationData.color),
                    Width = 24,
                    Height = 24,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(0, 0, 5, 0)
                };

                var textBlock = new TextBlock {
                    Text = translationData.text,
                    VerticalAlignment = VerticalAlignment.Center
                };

                stackPanel.Children.Add(path);
                stackPanel.Children.Add(textBlock);

                button.Content = stackPanel;
                button.Click += EmotionButton_Click;
                EmotionOptions.Children.Add(button);
            } else {
                // Для эмоций без перевода и SVG
                var button = new Button {
                    Content = emotion,
                    Tag = emotion,
                    Height = 50,
                    Width = 150,
                    Margin = new Thickness(5)
                };
                button.Click += EmotionButton_Click;
                EmotionOptions.Children.Add(button);
            }
        }
    }

    private async void PlayNextAudio() {
        if (!isTestRunning) return;

        if (currentFileIndex < files_to_play.Count) {
            var currentFile = files_to_play[currentFileIndex];
            string filePath = currentFile["path"]!.ToString();
            currentCorrectEmotion = currentFile["emotion"]?.ToString();

            // Play audio asynchronously
            await Task.Run(() => PlayAudioFile(filePath));

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
            MessageBox.Show($"Тест завершен! Правильных ответов: {correctAnswers} из {files_to_play.Count}");
        }
    }


    private async void PlayAudioFile(string filePath) {
        using (var audioFile = new AudioFileReader(filePath))
            using (var outputDevice = new WaveOutEvent()) {
                outputDevice.Init(audioFile);
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