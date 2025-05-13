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

namespace UAudio.USubtitles.Editor
{
    public enum AudioState
    {
        AudioState_Stopped,
        AudioState_Playing,
        AudioState_Paused,
    }

    public class AudioPlayer
    {
        private AudioClip _clip = null;
        public AudioClip Clip => _clip;

        public AudioState _prev = AudioState.AudioState_Stopped;

        public AudioState Prev => _prev;
        public AudioState State { get; private set; } = AudioState.AudioState_Stopped;

        public float WavePosition = 0.0f; // This is the selection start playhead. 

        /// <summary>
        /// Sets the clip.
        /// </summary>
        /// <param name="clip">The audio clip.</param>
        public void SetClip(AudioClip clip)
        {
            _clip = clip;
        }

        /// <summary>
        /// Updates the state of the player.
        /// </summary>
        public void Update()
        {
            bool isClipPlaying = AudioUtility.IsClipPlaying();
            if (State == AudioState.AudioState_Playing && !isClipPlaying)
            {
                SetState(AudioState.AudioState_Stopped);
            }
        }

        /// <summary>
        /// Sets the current state of the player.
        /// </summary>
        /// <param name="state">The new state the player will have.</param>
        public void SetState(AudioState state)
        {
            if (State == state)
            {
                return;
            }

            _prev = State;
            State = state;
            switch (State)
            {
                case AudioState.AudioState_Playing:
                {
                    // Pause/Stop functionality. If the clip was stopped we should start from the WavePosition point (which is where the selection start playhead is).
                    if (Prev == AudioState.AudioState_Paused)
                    {
                        AudioUtility.ResumeClip();
                    }
                    else
                    {
                        AudioUtility.SetClipSamplePosition(_clip, (int)WavePosition);
                        AudioUtility.PlayClip(_clip, (int)WavePosition, false);
                    }
                    break;
                }
                case AudioState.AudioState_Paused:
                {
                    AudioUtility.PauseClip();
                    break;
                }
                case AudioState.AudioState_Stopped:
                {
                    AudioUtility.StopAllClips();
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the position of the current playback.
        /// </summary>
        /// <param name="samplePosition">The sample position in bytes.</param>
        public void SetPosition(float samplePosition)
        {
            WavePosition = samplePosition;
            AudioUtility.SetClipSamplePosition(_clip, (int)samplePosition);
        }
    }
}