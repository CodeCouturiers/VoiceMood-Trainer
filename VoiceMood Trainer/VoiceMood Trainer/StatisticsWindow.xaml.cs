using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VoiceMood_Trainer
{
    public partial class StatisticsWindow : Window
    {
        private Dictionary<DateTime, List<EmotionStatistics>> statisticsByDate;

        public StatisticsWindow(Dictionary<DateTime, List<EmotionStatistics>> statistics)
        {
            InitializeComponent();
            statisticsByDate = statistics;
            DateSelector.SelectedDate = DateTime.Today;
            UpdateStatistics(DateTime.Today);
        }

        private void DateSelector_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateSelector.SelectedDate.HasValue)
            {
                UpdateStatistics(DateSelector.SelectedDate.Value);
            }
        }

        private void UpdateStatistics(DateTime date)
        {
            if (statisticsByDate.ContainsKey(date))
            {
                var statistics = statisticsByDate[date];
                var displayData = statistics.Select(s => new EmotionStatisticsDisplay(s.Emotion, s.CorrectAnswers,
                        s.IncorrectAnswers)).ToList();

                StatisticsGrid.ItemsSource = displayData;
            }
            else
            {
                StatisticsGrid.ItemsSource = null;
            }
        }

        private double CalculateAccuracy(int correct, int incorrect)
        {
            int total = correct + incorrect;
            return total > 0 ? (double)correct / total * 100 : 0;
        }
    }
}
