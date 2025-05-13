/*
 *  Copyright(c) 2025 Kylian Dekker
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 *  IN THE SOFTWARE.
 */

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