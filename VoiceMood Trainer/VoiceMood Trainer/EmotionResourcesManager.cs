using System.Drawing;
using System.Globalization;
using System.Resources;

namespace VoiceMood_Trainer {
public static class EmotionResourcesManager {
    private static readonly ResourceManager ResourceManager;

    static EmotionResourcesManager() {
        ResourceManager = new ResourceManager("VoiceMood_Trainer.EmotionResources", typeof(EmotionResourcesManager).Assembly);
    }

    public static (string text, string svgPath, System.Drawing.Color color) GetEmotionTranslation(string emotion) {
        string text = GetString(emotion + "_Text");
        string svgPath = GetString(emotion + "_SVGPath");
        string colorString = GetString(emotion + "_Color");
        System.Drawing.Color color = ColorTranslator.FromHtml(colorString);

        return (text, svgPath, color);
    }

    private static string GetString(string key) {
        return ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? string.Empty;
    }
}

}
