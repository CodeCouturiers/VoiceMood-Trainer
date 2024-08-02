using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Media.Imaging;
using NAudio.Wave;

namespace VoiceMood_Trainer
{
    public static class EmotionResourcesManager
    {
        private static readonly ResourceManager ResourceManager;
        private static IWavePlayer? correctSoundPlayer;
        private static IWavePlayer? incorrectSoundPlayer;
        private static WaveStream? correctSoundStream;
        private static WaveStream? incorrectSoundStream;


        static EmotionResourcesManager()
        {
            InitializeSounds();

            ResourceManager = new ResourceManager("VoiceMood_Trainer.EmotionResources", typeof(EmotionResourcesManager).Assembly);
        }

        public static(string text, string svgPath, System.Drawing.Color color) GetEmotionTranslation(string emotion)
        {
            string text = GetString(emotion + "_Text");
            string svgPath = GetString(emotion + "_SVGPath");
            string colorString = GetString(emotion + "_Color");
            System.Drawing.Color color = ColorTranslator.FromHtml(colorString);

            return (text, svgPath, color);
        }

        public static BitmapImage GetAppIcon()
        {
            using (var bitmap = EmotionResources.emotions)
                using (var memory = new MemoryStream())
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    return bitmapImage;
                }
        }

        private static void InitializeSounds()
        {
            try
            {
                var correctStream = new MemoryStream(EmotionResources.correct);
                var incorrectStream = new MemoryStream(EmotionResources.incorrect);

                var correctMp3Reader = new Mp3FileReader(correctStream);
                var incorrectMp3Reader = new Mp3FileReader(incorrectStream);

                correctSoundStream = WaveFormatConversionStream.CreatePcmStream(correctMp3Reader);
                incorrectSoundStream = WaveFormatConversionStream.CreatePcmStream(incorrectMp3Reader);

                correctSoundPlayer = new WaveOutEvent();
                incorrectSoundPlayer = new WaveOutEvent();

                correctSoundPlayer.Init(correctSoundStream);
                incorrectSoundPlayer.Init(incorrectSoundStream);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sound files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetString(string key)
        {
            return ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? string.Empty;
        }

        public static void PlayCorrectSound(bool enabled)
        {
            if (enabled && correctSoundStream != null && correctSoundPlayer != null)
            {
                correctSoundStream.Position = 0;
                correctSoundPlayer.Play();
            }
        }

        public static void PlayIncorrectSound(bool enabled)
        {
            if (enabled && incorrectSoundStream != null && incorrectSoundPlayer != null)
            {
                incorrectSoundStream.Position = 0;
                incorrectSoundPlayer.Play();
            }
        }

    }
}
