using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
namespace VoiceMood_Trainer
{

    public class LocalizationManager
    {
        private static LocalizationManager _instance;
        private Dictionary<string, Dictionary<string, string>> _languages;
        private string _currentLanguage;

        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LocalizationManager();
                }
                return _instance;
            }
        }

        private LocalizationManager()
        {
            _languages = new Dictionary<string, Dictionary<string, string>>();
            LoadLanguages();
            _currentLanguage = "en"; // По умолчанию английский
        }

        private void LoadLanguages()
        {
            string langPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang");
            foreach (string file in Directory.GetFiles(langPath, "*.json"))
            {
                string langCode = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);
                var langDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                _languages[langCode] = langDict;
            }
        }

        public void SetLanguage(string langCode)
        {
            if (_languages.ContainsKey(langCode))
            {
                _currentLanguage = langCode;
                Application.Current.Resources.MergedDictionaries.Clear();
                foreach (var key in _languages[langCode].Keys)
                {
                    Application.Current.Resources[key] = _languages[langCode][key];
                }
            }
        }

        public string GetString(string key)
        {
            if (_languages.ContainsKey(_currentLanguage) && _languages[_currentLanguage].ContainsKey(key))
            {
                return _languages[_currentLanguage][key];
            }
            return key; // Возвращаем ключ, если перевод не найден
        }

        public List<string> GetAvailableLanguages()
        {
            return new List<string>(_languages.Keys);
        }
    }


}
