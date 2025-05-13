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

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UAudio.USubtitles.Editor
{
    public class WaveformDisplay
    {
        private AudioClip _currentClip = null;
        public AudioClip Clip => _currentClip;

        private AudioImporter _audioImporter = null;
        private Type _audioUtilType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AudioUtil");
        private Func<AudioImporter, float[]> _getAudioMinMaxData;

        /// <summary>
        /// Sets the audio clip (used for the waveform).
        /// </summary>
        /// <param name="clip">The audio clip.</param>
        public void SetClip(AudioClip clip)
        {
            _currentClip = clip;

            var path = AssetDatabase.GetAssetPath(_currentClip);
            _audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;

            _getAudioMinMaxData = (Func<AudioImporter, float[]>)_audioUtilType.GetMethod("GetMinMaxData",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .CreateDelegate(typeof(Func<AudioImporter, float[]>));
        }

        /// <summary>
        /// Draws the waveform.
        /// </summary>
        /// <param name="rect">The total space reserved for the waveform.</param>
        public void Draw(Rect rect, bool useMaterial = false)
        {
            if (_audioImporter == null)
            {
                Debug.LogWarning("Audio Importer is null.");
                return;
            }

            float[] minMaxData = (_audioImporter == null) ? null : _getAudioMinMaxData(_audioImporter);
            int numChannels = _currentClip.channels;
            int numSamples = (minMaxData == null) ? 0 : (minMaxData.Length / (2 * numChannels));
            float h = (float)rect.height / (float)numChannels;
            for (int channel = 0; channel < numChannels; channel++)
            {
                Rect channelRect = new Rect(rect.x, rect.y + h * channel, rect.width, h);
                var curveColor = USubtitleEditorVariables.Preferences.Color_Waveform;

                AudioCurveRendering.AudioMinMaxCurveAndColorEvaluator dlg = delegate (float x, out Color col, out float minValue, out float maxValue)
                {
                    col = curveColor;
                    if (numSamples <= 0)
                    {
                        minValue = 0.0f;
                        maxValue = 0.0f;
                    }
                    else
                    {
                        float p = Mathf.Clamp(x * (numSamples - 2), 0.0f, numSamples - 2);
                        int i = (int)Mathf.Floor(p);
                        int offset1 = (i * numChannels + channel) * 2;
                        int offset2 = offset1 + numChannels * 2;
                        minValue = Mathf.Min(minMaxData[offset1 + 1], minMaxData[offset2 + 1]) * 0.95f;
                        maxValue = Mathf.Max(minMaxData[offset1 + 0], minMaxData[offset2 + 0]) * 0.95f;
                        if (minValue > maxValue) { float tmp = minValue; minValue = maxValue; maxValue = tmp; }
                    }
                };

                if (useMaterial)
                {
                    _ = typeof(AudioCurveRendering).GetMethod("DrawMinMaxFilledCurveInternal",
                            BindingFlags.Static | BindingFlags.NonPublic)
                        .Invoke(null, new object[] { channelRect, dlg });
                }
                else
                {
                    AudioCurveRendering.DrawMinMaxFilledCurve(
                        channelRect,
                        dlg);
                }
            }
        }
    }
}