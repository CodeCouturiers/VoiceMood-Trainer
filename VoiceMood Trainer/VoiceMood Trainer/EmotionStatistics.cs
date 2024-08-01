using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceMood_Trainer
{
    public class EmotionStatistics
    {
        public string Emotion { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public DateTime Date { get; set; }

        public EmotionStatistics(string emotion, int correct, int incorrect, DateTime date)
        {
            Emotion = emotion;
            CorrectAnswers = correct;
            IncorrectAnswers = incorrect;
            Date = date;
        }
    }
}
