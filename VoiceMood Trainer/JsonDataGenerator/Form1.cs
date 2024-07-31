using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace JsonDataGenerator
{
    public partial class Form1 : Form
    {
        private string mainFolder;

        public Form1()
        {
            InitializeComponent();
        }

        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    mainFolder = folderDialog.SelectedPath;
                    FolderPathTextBox.Text = mainFolder;
                    StatusLabel.Text = $"Selected folder: {mainFolder}";
                }
            }
        }

        private void SaveJsonButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(mainFolder) && string.IsNullOrEmpty(FolderPathTextBox.Text))
            {
                StatusLabel.Text = "Status: Please select or enter a source folder first.";
                return;
            }

            if (string.IsNullOrEmpty(mainFolder) && !string.IsNullOrEmpty(FolderPathTextBox.Text))
            {
                mainFolder = FolderPathTextBox.Text;
            }

            if (!Directory.Exists(mainFolder))
            {
                StatusLabel.Text = "Status: The specified folder does not exist.";
                return;
            }

            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON Files (*.json)|*.json";
                saveFileDialog.FileName = "ravdess_data.json";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string jsonFilePath = saveFileDialog.FileName;
                    ProcessFilesAndSaveJson(jsonFilePath);
                }
            }
        }

        private void ProcessFilesAndSaveJson(string jsonFilePath)
        {
            var data = new
            {
                actors = new Dictionary<string, List<object>>(),
                emotions = new Dictionary<string, List<object>>(),
                intensities = new Dictionary<string, List<object>>(),
                statements = new Dictionary<string, List<object>>(),
                presets = new
                {
                    all_emotions = new List<string> { "neutral", "calm", "happy", "sad", "angry", "fearful", "disgust", "surprised" },
                    basic_emotions = new List<string> { "happy", "sad", "angry", "fearful" },
                    positive_emotions = new List<string> { "happy", "calm" },
                    negative_emotions = new List<string> { "sad", "angry", "fearful", "disgust" },
                    neutral_and_extreme = new List<string> { "neutral", "happy", "angry", "fearful" }
                }
            };

            var emotions = new Dictionary<string, string>
            {
                { "01", "neutral" },
                { "02", "calm" },
                { "03", "happy" },
                { "04", "sad" },
                { "05", "angry" },
                { "06", "fearful" },
                { "07", "disgust" },
                { "08", "surprised" }
            };

            var intensities = new Dictionary<string, string>
            {
                { "01", "normal" },
                { "02", "strong" }
            };

            var statements = new Dictionary<string, string>
            {
                { "01", "Kids are talking by the door" },
                { "02", "Dogs are sitting by the door" }
            };

            foreach (var actorFolder in Directory.GetDirectories(mainFolder))
            {
                var actorNumber = Path.GetFileName(actorFolder).Split('_')[1];
                var actorFiles = new List<object>();

                foreach (var filename in Directory.GetFiles(actorFolder, "*.wav"))
                {
                    var fileParts = Path.GetFileName(filename).Split('-');
                    if (fileParts.Length >= 6)
                    {
                        var emotionCode = fileParts[2];
                        var intensityCode = fileParts[3];
                        var statementCode = fileParts[4];
                        var repetition = fileParts[5];

                        var fileInfo = new
                        {
                            filename = Path.GetFileName(filename),
                            path = filename,
                            emotion = GetValueOrDefault(emotions, emotionCode),
                            intensity = GetValueOrDefault(intensities, intensityCode),
                            statement = GetValueOrDefault(statements, statementCode),
                            repetition
                        };

                        actorFiles.Add(fileInfo);

                        AddToDictionary(data.emotions, emotionCode, fileInfo);
                        AddToDictionary(data.intensities, intensityCode, fileInfo);
                        AddToDictionary(data.statements, statementCode, fileInfo);
                    }
                }

                data.actors[actorNumber] = actorFiles;
            }

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);

            StatusLabel.Text = $"Status: JSON file created successfully at {jsonFilePath}!";
        }

        private static string GetValueOrDefault(Dictionary<string, string> dictionary, string key)
        {
            return dictionary.TryGetValue(key, out var value) ? value : null;
        }

        private static void AddToDictionary(Dictionary<string, List<object>> dictionary, string key, object fileInfo)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = new List<object>();
            }
            dictionary[key].Add(fileInfo);
        }
    }
}
