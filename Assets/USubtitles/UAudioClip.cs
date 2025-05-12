using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UAudio.USubtitles
{
    public enum SupportedLanguage
    {
        English,
        Nederlands
    }

    [CreateAssetMenu(fileName = "UAudioClip", menuName = "/UAudioClip", order = 1)]
    public class UAudioClip : ScriptableObject
    {
        public AudioClip Clip = null;
        public List<DialogueItem> Dialogue = new List<DialogueItem>();

        public List<string> GetSentences(SupportedLanguage language)
        {
            return GetSentencesUntilSamplePosition(language, uint.MaxValue);
        }

        public List<string> GetSentencesTillSamplePosition(SupportedLanguage language, uint samplePosition)
        {
            return GetSentencesUntilSamplePosition(language, samplePosition);
        }

        private List<string> GetSentencesUntilSamplePosition(SupportedLanguage language, uint samplePosition)
        {
            List<string> sentences = new();
            StringBuilder currentSentence = new();

            foreach (var item in Dialogue)
            {
                // Stop processing if the sample position exceeds the desired position
                if (item.SamplePosition > samplePosition)
                    break;

                Line line = item.Text.GetLineInfo(language);

                // Check for Clear and end the current sentence if necessary
                if (line.NewLine && currentSentence.Length > 0)
                {
                    sentences.Add(currentSentence.ToString().Trim());
                    currentSentence.Clear();
                }

                AppendTextToSentence(line, currentSentence);
            }

            if (currentSentence.Length > 0)
            {
                sentences.Add(currentSentence.ToString().Trim());
            }

            return sentences;
        }

        private void AppendTextToSentence(Line line, StringBuilder currentSentence)
        {
            string text = line.Text;

            // Apply rich text markup
            if (line.Bold) text = $"<b>{text}</b>";
            if (line.Italic) text = $"<i>{text}</i>";
            if (line.UseColor) text = $"<color=#{ColorUtility.ToHtmlStringRGBA(line.Color)}>{text}</color>";

            currentSentence.Append(text);
        }
    }
}