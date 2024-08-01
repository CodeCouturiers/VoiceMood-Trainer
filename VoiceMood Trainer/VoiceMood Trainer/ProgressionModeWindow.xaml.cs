using System.Windows;
using System.Collections.ObjectModel;

namespace VoiceMood_Trainer
{
    public partial class ProgressionModeWindow : Window
    {
        private MainWindow mainWindow;
        private ObservableCollection<EmotionItem> emotionItems;

        public ProgressionModeWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            emotionItems = new ObservableCollection<EmotionItem>();
            InitializeEmotions();
            EmotionsListView.ItemsSource = emotionItems;
            UpdateUI();
        }

        private void InitializeEmotions()
        {
            var allEmotions = mainWindow.GetAllEmotions();
            emotionItems = new ObservableCollection<EmotionItem>(
            allEmotions.Select(e => new EmotionItem { Name = e, IsSelected = false })
            );
        }

        private void UpdateUI()
        {
            int selectedCount = emotionItems.Count(e => e.IsSelected);
            EmotionsCountText.Text = $"Selected emotions: {selectedCount}";
            StartButton.IsEnabled = selectedCount > 0;
        }

        private void EmotionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void EmotionCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmotions = emotionItems.Where(e => e.IsSelected).Select(e => e.Name).ToList();
            mainWindow.SetupProgressionTest(selectedEmotions);
            Close();
        }

        private void BackToMainButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class EmotionItem
    {
        public string Name { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
