using UnityEngine;

namespace UAudio.USubtitles
{
    [System.Serializable]
    public class Line
    {
        public string Text;
        public UnityEngine.Color Color = UnityEngine.Color.white;
        public bool UseColor = false;
        public bool Bold = false;
        public bool Italic = false;
        public bool NewLine = false;

        public Line()
        { }

        public string Get() => Text;

        public static string GenerateString(string text, bool bold, bool italic, bool useColor, UnityEngine.Color color, bool useMarkup = false)
        {
            if (bold)
            {
                text = $"<b>{text}</b>";
            }
            if (italic)
            {
                text = $"<i>{text}</i>";
            }
            if (useMarkup)
            {
                if (useColor)
                {
                    text = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>";
                }
            }
            return text;
        }

        public string GetLine(bool useMarkup = false)
        {
            return GenerateString(Text, Bold, Italic, UseColor, Color, useMarkup);
        }
    }

    [System.Serializable]
    public class Text
    {
        public Line English = new Line();
        public Line Nederlands = new Line();

        public string GetLine(SupportedLanguage language, bool useMarkup = false)
        {
            Line line = English;
            switch (language)
            {
                case SupportedLanguage.English:
                {
                    line = English;
                    break;
                }
                case SupportedLanguage.Nederlands:
                {
                    line = Nederlands;
                    break;
                }
            }

            return line.GetLine(useMarkup);
        }

        public Line GetLineInfo(SupportedLanguage language)
        {
            switch (language)
            {
                case SupportedLanguage.English:
                {
                    return English;
                }
                case SupportedLanguage.Nederlands:
                {
                    return Nederlands;
                }
            }
            return English;
        }
    }

    [System.Serializable]
    public class DialogueItem
    {
        public uint SamplePosition = 0;
        public Text Text = new Text();
    }
}