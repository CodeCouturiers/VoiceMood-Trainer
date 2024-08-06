using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace VoiceMood_Trainer
{
    public partial class EmotionDiscriminationWindow : Window
    {
        private MainWindow mainWindow;
        private List<(string, string)> emotionPairs = new List<(string, string)>
        {
            ("calm", "neutral"),
            ("happy", "surprised"),
            ("sad", "fearful"),
            ("angry", "disgust")
        };
        private Random random = new Random();
        private bool isTestRunning = false;
        private string currentCorrectEmotion;
        private int totalFiles = 100;
        private int currentFileCount = 0;
        private int correctAnswers = 0;
        private int incorrectAnswers = 0;

        public EmotionDiscriminationWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.Icon = EmotionResourcesManager.GetAppIcon();

            UpdateButtonStates();
            UpdateScoreDisplay();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartDiscriminationTest();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopDiscriminationTest();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            PlayNextEmotionPair();
        }

        private void StartDiscriminationTest()
        {
            isTestRunning = true;
            currentFileCount = 0;
            correctAnswers = 0;
            incorrectAnswers = 0;
            PlayNextEmotionPair();
            UpdateButtonStates();
            UpdateScoreDisplay();
        }

        private void StopDiscriminationTest()
        {
            isTestRunning = false;
            mainWindow.StopAudio();
            UpdateButtonStates();
        }

        private void PlayNextEmotionPair()
        {
            if (currentFileCount < totalFiles)
            {
                var currentPair = emotionPairs[random.Next(emotionPairs.Count)];
                currentCorrectEmotion = random.Next(2) == 0 ? currentPair.Item1 : currentPair.Item2;

                Emotion1Button.Content = LocalizationManager.Instance.GetString(currentPair.Item1);
                Emotion1Button.Tag = currentPair.Item1;
                Emotion2Button.Content = LocalizationManager.Instance.GetString(currentPair.Item2);
                Emotion2Button.Tag = currentPair.Item2;

                mainWindow.PlayRandomAudioForEmotion(currentCorrectEmotion);
                UpdateButtonStates();
                currentFileCount++;
            }
            else
            {
                MessageBox.Show(
                    $"Congratulations! You've completed the emotion discrimination test.\nCorrect answers: {correctAnswers} out of {totalFiles}");
                StopDiscriminationTest();
            }
        }

        private void EmotionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isTestRunning) { return; }

            Button clickedButton = (Button)sender;
            string selectedEmotion = (string)clickedButton.Tag;

            if (selectedEmotion == currentCorrectEmotion)
            {
                correctAnswers++;
                PlayNextEmotionPair();
            }
            else
            {
                incorrectAnswers++;
                mainWindow.PlayRandomAudioForEmotion(currentCorrectEmotion);
            }

            UpdateScoreDisplay();
        }

        private void BackToMainButton_Click(object sender, RoutedEventArgs e)
        {
            StopDiscriminationTest();
            Close();
        }

        private void UpdateButtonStates()
        {
            StartButton.IsEnabled = !isTestRunning;
            StopButton.IsEnabled = isTestRunning;
            NextButton.IsEnabled = isTestRunning;
            Emotion1Button.IsEnabled = isTestRunning;
            Emotion2Button.IsEnabled = isTestRunning;
        }

        private void UpdateScoreDisplay()
        {
            CorrectAnswersText.Text = correctAnswers.ToString();
            IncorrectAnswersText.Text = incorrectAnswers.ToString();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            mainWindow.StopAudio();
        }
    }
}