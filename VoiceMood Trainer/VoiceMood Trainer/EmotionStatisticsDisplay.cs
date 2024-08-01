using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceMood_Trainer
{
    public class EmotionStatisticsDisplay
    {
        public string Emotion { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public double Accuracy { get; set; }

        public EmotionStatisticsDisplay(string emotion, int correct, int incorrect)
        {
            Emotion = emotion;
            CorrectAnswers = correct;
            IncorrectAnswers = incorrect;
            Accuracy = CalculateAccuracy(correct, incorrect);
        }

        private static double CalculateAccuracy(int correct, int incorrect)
        {
            int total = correct + incorrect;
            return total > 0 ? (double)correct / total * 100 : 0;
        }
    }
}
