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

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UAudio.USubtitles.Editor
{
    public class Constants
    {
        public static Vector2 MARKER_SIZE = new Vector2(20, 15);
        public const float
            ZOOM_STRENGTH = 0.1f,
            ZOOM_MAX = 5.0f,
            ZOOM_MIN = 1.0f;
        public static Color PLAYBACK_PLAYHEAD_COLOR = Color.red;
        public static Color SELECTION_START_PLAYHEAD_COLOR = Color.white;
    }

    [ExecuteInEditMode]
    [CustomEditor(typeof(UAudioClip), true)]
    public class UAudioClipEditor : UnityEditor.Editor
    {
        /*##################
		 * Important variables like systems, etc. These variables only change once.
		##################*/

        private UAudioClip _clip = null;

        private AudioPlayer _audioPlayer = new AudioPlayer();
        private WaveformDisplay _waveformDisplay = new WaveformDisplay();

        private PreviewRenderUtility _PreviewUtility = null;

        static bool _autoPlay;
        static bool _isLooping;

        static private GUIContent
            _stopIcon = null,
            _playIcon = null,
            _pauseIcon = null,
            _plusIcon = null,
            _minusIcon = null,
            _settingsIcon = null;

        private Material _HandleLinesMaterial = null;

        /*##################
		 * Variables that change in editor runtime.
		##################*/

        // This is the language that is used in the preview window (it shows text, colors, new line marks, etc).
        private static SupportedLanguage _language = SupportedLanguage.English;

        // This is how much the user has zoomed in the waveform display.
        private float _zoom = Constants.ZOOM_MIN;
        // This is how much the user has scrolled in the waveform display.
        private Vector2 _scrollPos = Vector2.zero;

        public enum InteractionType
        {
            TimelineInteraction_None,
            TimelineInteraction_TimeMarker,
            TimelineInteraction_Timeline
        }

        public class Interaction
        {
            private InteractionType _currentInteraction = InteractionType.TimelineInteraction_None;
            public InteractionType CurrentInteraction => _currentInteraction;

            private EventType _previousEventType = EventType.Repaint;
            public EventType PreviousEventType => _previousEventType;

            private int _index = -1;
            public int Index => _index;

            private int _dragIndex = -1;
            public int DragIndex => _dragIndex;

            public void SetInteraction(InteractionType interaction)
            {
                _currentInteraction = interaction;
            }

            public void SetIndex(int index)
            {
                _index = index;
            }

            public void SetDragIndex(int index)
            {
                _dragIndex = index;
            }

            public void SetEventType(EventType eventType)
            {
                _previousEventType = eventType;
            }
        }

        private Interaction _interaction = new();

        /*##################
		 * Initialization
		##################*/

        private void Init()
        {
            // Load all preferences for the subtitle editor.
            USubtitleEditorPreferences.Load();

            // Make sure all other audio is stopped.
            AudioUtility.StopAllClips();

            // Set the clip in the audio player and waveform display.
            _clip = target as UAudioClip;
            _audioPlayer.SetClip(_clip.Clip);
            _waveformDisplay.SetClip(_clip.Clip);

            // Load the icons.
            _stopIcon = EditorGUIUtility.TrIconContent("d_PreMatQuad", "Stop");
            _playIcon = EditorGUIUtility.TrIconContent("d_PlayButton", "Play");
            _pauseIcon = EditorGUIUtility.TrIconContent("d_PauseButton", "Pause");
            _plusIcon = EditorGUIUtility.TrIconContent("d_Toolbar Plus", "Add Marker");
            _minusIcon = EditorGUIUtility.TrIconContent("d_Toolbar Minus", "Remove Marker");
            _settingsIcon = EditorGUIUtility.TrIconContent("d_Settings", "Open Settings");
        }

        private void OnEnable()
        {
            _HandleLinesMaterial = EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat") as Material;

            // Initialize.
            Init();
        }

        // Needed for recoloring when changing preference colors.
        public override bool RequiresConstantRepaint() => true;

        public void OnDisable()
        {
            _audioPlayer.SetState(AudioState.AudioState_Stopped);

            if (_PreviewUtility != null)
            {
                _PreviewUtility.Cleanup();
                _PreviewUtility = null;
            }
            _HandleLinesMaterial = null;
        }

        /*##################
		 * Settings
		##################*/

        public override bool HasPreviewGUI() => targets != null;

        /*##################
		 * Utilities
		##################*/

        /// <summary>
        /// Calculates the sample based on the position in the waveform and the length of the clip.
        /// </summary>
        /// <param name="position">Position of the timeline line.</param>
        /// <returns></returns>
        public uint CalculateSamples(float position)
        {
            float time_percentage = 1.0f / _clip.Clip.length * position;
            int samples = _clip.Clip.samples;
            uint playFrom = (uint)Mathf.FloorToInt(samples * time_percentage);

            return playFrom;
        }

        /*##################
		 * Zoom
		##################*/

        /// This is the preview image shown in the explorer.
        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            if (!_clip)
            {
                Init();
            }

            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                return null;
            }

            _PreviewUtility ??= new PreviewRenderUtility();

            _PreviewUtility.BeginStaticPreview(new Rect(0, 0, width, height));
            _HandleLinesMaterial.SetPass(0);

            _waveformDisplay.Draw(new Rect(0.05f * width * EditorGUIUtility.pixelsPerPoint, 0.05f * width * EditorGUIUtility.pixelsPerPoint, 1.9f * width * EditorGUIUtility.pixelsPerPoint, 1.9f * height * EditorGUIUtility.pixelsPerPoint), true);
            return _PreviewUtility.EndStaticPreview();
        }

        public override void OnPreviewSettings()
        {
            if (targets.Length == 1)
            {
                using (new EditorGUI.DisabledScope(_audioPlayer.State != AudioState.AudioState_Playing && _audioPlayer.State != AudioState.AudioState_Paused))
                {
                    if (GUILayout.Button(_stopIcon, EditorStyles.toolbarButton))
                    {
                        _audioPlayer.SetState(AudioState.AudioState_Stopped);
                    }
                }

                {
                    if (GUILayout.Button(_audioPlayer.State == AudioState.AudioState_Playing ? _pauseIcon : _playIcon, EditorStyles.toolbarButton))
                    {
                        if (_audioPlayer.State != AudioState.AudioState_Playing)
                        {
                            _audioPlayer.SetState(AudioState.AudioState_Playing);
                        }
                        else
                        {
                            _audioPlayer.SetState(AudioState.AudioState_Paused);
                        }
                    }
                }

                if (GUILayout.Button(_plusIcon, EditorStyles.toolbarButton))
                {
                    AddMarker();
                }
                using (new EditorGUI.DisabledScope(_interaction.Index == -1))
                {
                    if (GUILayout.Button(_minusIcon, EditorStyles.toolbarButton))
                    {
                        RemoveMarker();
                    }
                }

                var sliderRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(55f));
                sliderRect.y -= 1f;
                _zoom = GUI.HorizontalSlider(sliderRect, _zoom, Constants.ZOOM_MIN, Constants.ZOOM_MAX);

                _language = (SupportedLanguage)EditorGUILayout.EnumPopup(_language);

                if (GUILayout.Button(_settingsIcon, EditorStyles.toolbarButton))
                {
                    OpenPreferences();
                }
            }
        }

        /// <summary>
        /// Removes the current marker that is being selected.
        /// </summary>
        private void RemoveMarker()
        {
            if (_interaction.Index != -1)
            {
                _clip.Dialogue.RemoveAt(_interaction.Index);
                _interaction.SetIndex(-1);
            }
        }

        /// <summary>
        /// Creates a marker at the current playhead position.
        /// </summary>
        private void AddMarker()
        {
            if (_clip.Dialogue.Find(x => x.SamplePosition == (uint)_audioPlayer.WavePosition) != null)
            {
                return;
            }

            int index = 0;
            Undo.RecordObject(_clip, "Added marker");

            DialogueItem dialogueItem = new DialogueItem();
            dialogueItem.SamplePosition = (uint)_audioPlayer.WavePosition;
            _clip.Dialogue.Add(dialogueItem);

            ReorderTimeMarkers();

            index = _clip.Dialogue.FindIndex(x => x.SamplePosition == (uint)_audioPlayer.WavePosition);
            _interaction.SetIndex(index);
            _interaction.SetDragIndex(index);

            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// Simple menu that appears when right clicking a marker in the timeline.
        /// </summary>
        /// <returns>The menu for the marker with all events binded.</returns>
        private GenericMenu GetMarkerMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete"), false, OnMarkerRemove);
            return menu;
        }

        private void OnMarkerRemove() => RemoveMarker();

        /// <summary>
        /// Opens the preferences window for the subtitle editor.
        /// </summary>
        private void OpenPreferences()
        {
            SettingsService.OpenUserPreferences("Preferences/Subtitle Editor");
        }

        public override void OnPreviewGUI(Rect previewWindowRect, GUIStyle background)
        {
            Rect previewRect = previewWindowRect;
            previewRect.height *= 0.15f;

            float playheadMarkerPosition = _audioPlayer.WavePosition;
            if (AudioUtility.IsClipPlaying())
            {
                playheadMarkerPosition = ((float)_clip.Clip.samples) / _clip.Clip.length * AudioUtility.GetClipPosition();
            }
            string text = UpdateTextPreview(playheadMarkerPosition);
            GUIStyle textStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUI.DrawRect(previewRect, new Color32(0, 0, 0, 155));
            EditorGUI.LabelField(previewRect, new GUIContent(text), textStyle);

            EditorGUILayout.Separator();

            previewWindowRect.height -= previewRect.height;
            previewWindowRect.y += previewRect.height;

            // Calculate the rect for the waveform display with zoom and scroll position.
            Rect zoomedPreviewWindowRect = previewWindowRect;
            zoomedPreviewWindowRect.width *= _zoom;
            zoomedPreviewWindowRect.x = -_scrollPos.x;

            // Scrollbar around waveform.
            _scrollPos = GUI.BeginScrollView(previewWindowRect, _scrollPos, zoomedPreviewWindowRect, true, false, GUI.skin.horizontalScrollbar, GUIStyle.none);
            _waveformDisplay.Draw(zoomedPreviewWindowRect);
            DrawMarkers(previewWindowRect, zoomedPreviewWindowRect);
            DrawPlayheadMarkers(previewWindowRect);
            HandlePlayheadEvents(previewWindowRect, zoomedPreviewWindowRect);
            HandleTimelineZoom(previewWindowRect);

            GUI.EndScrollView();

            if (previewWindowRect.Contains(Event.current.mousePosition))
            {
                // You only want to consume mouse events
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDrag)
                {
                    Event.current.Use(); // This prevents Unity from toggling the foldout

                    serializedObject.Update();
                }
            }
        }

        /// <summary>
        /// Handles the hovering, dragging and clicking of the play head. 
        /// </summary>
        /// <param name="previewWindowRect">The complete rect of the preview window.</param>
        /// <param name="zoomedPreviewWindowRect">The zoomed in rect of the preview window (based on the timeline).</param>
        private void HandlePlayheadEvents(Rect previewWindowRect, Rect zoomedPreviewWindowRect)
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.MouseDrag:
                case EventType.MouseDown:
                {
                    if (evt.button == 0)
                    {
                        if (previewWindowRect.Contains(evt.mousePosition) && _interaction.CurrentInteraction != InteractionType.TimelineInteraction_TimeMarker)
                        {
                            _interaction.SetIndex(-1);

                            _interaction.SetInteraction(InteractionType.TimelineInteraction_Timeline);

                            float startSample = _clip.Clip.samples / zoomedPreviewWindowRect.width * (_scrollPos.x + evt.mousePosition.x);
                            _audioPlayer.SetPosition(startSample);

                            evt.Use();
                        }
                    }
                    break;
                }
                case EventType.MouseUp:
                {
                    if (evt.button == 0)
                    {
                        if (_interaction.CurrentInteraction != InteractionType.TimelineInteraction_TimeMarker)
                        {
                            _interaction.SetInteraction(InteractionType.TimelineInteraction_None);

                            EditorUtility.SetDirty(target);
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Handles the zooming of the timeline by using the scroll wheel.
        /// </summary>
        /// <param name="previewWindowRect">The complete rect of the preview window.</param>
        private void HandleTimelineZoom(Rect previewWindowRect)
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.ScrollWheel:
                {
                    if (previewWindowRect.Contains(evt.mousePosition))
                    {
                        // Get the relative position of the mouse position (so that it scrolls towards the mouse position)
                        var relX = evt.mousePosition.x - previewWindowRect.x;

                        float initialZoom = _zoom;
                        Vector2 initialScrollPos = _scrollPos;

                        float strength = Constants.ZOOM_STRENGTH * (evt.delta.y < 0 ? 1 : -1);
                        relX *= evt.delta.y < 0 ? 1 : -1;
                        _zoom += strength;
                        _scrollPos += new Vector2(relX * Constants.ZOOM_STRENGTH, 0);

                        if (_zoom < Constants.ZOOM_MIN || _zoom > Constants.ZOOM_MAX)
                        {
                            _scrollPos = initialScrollPos;
                            _zoom = initialZoom;
                        }

                        evt.Use();
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// The red marker refers to the moving indicator that shows the current playback position as the audio plays.
        /// The white marker refers to the indicator that shows the start position when the user hits play, and it remains static while the playhead moves.
        /// </summary>
        /// <param name="previewWindowRect">The complete rect of the preview window.</param>
        private void DrawPlayheadMarkers(Rect previewWindowRect)
        {
            Rect rect_waveMarker = new Rect(previewWindowRect.x, previewWindowRect.y, 2, previewWindowRect.height);
            float playheadMarkerPosition = previewWindowRect.width / (_clip ? _clip.Clip.length : 0) * AudioUtility.GetClipPosition();
            float selectionStartMarkerPosition = previewWindowRect.width / (_clip ? _clip.Clip.samples : 0) * _audioPlayer.WavePosition;

            Rect rect_timelineMarker = new Rect(((rect_waveMarker.x + playheadMarkerPosition) * _zoom) - _scrollPos.x, rect_waveMarker.y, rect_waveMarker.width, rect_waveMarker.height);

            // Playhead marker.
            EditorGUI.DrawRect(rect_timelineMarker, Constants.PLAYBACK_PLAYHEAD_COLOR);

            // Selection start playhead.
            EditorGUI.DrawRect(new Rect(((rect_waveMarker.x + selectionStartMarkerPosition) * _zoom) - _scrollPos.x, rect_waveMarker.y, rect_waveMarker.width, rect_waveMarker.height), Constants.SELECTION_START_PLAYHEAD_COLOR);
        }

        private float CalculateMarkerPosition(Rect previewWindowRect, uint samplePosition) => previewWindowRect.width / (_clip ? _clip.Clip.samples : 0) * samplePosition;

        /// <summary>
        /// Returns a list containing the marker rects used for displaying and event checking.
        /// </summary>
        /// <param name="previewWindowRect">The complete rect of the preview window.</param>
        /// <param name="marker">The marker that is being displayed.</param>
        /// <returns>List containing the marker rects. Index 0 is the full rect, index 1 is the top rect, index 2 is the line.</returns>
        List<Rect> GetMarkerRect(Rect previewWindowRect, DialogueItem marker)
        {
            // The exact x sample position of the marker.
            float markerX = CalculateMarkerPosition(previewWindowRect, marker.SamplePosition);

            Rect markerLine = new(((previewWindowRect.x + markerX) * _zoom) - _scrollPos.x, previewWindowRect.y, 2, previewWindowRect.height);

            // The top part of the marker line.
            Rect markerTop = new(new Vector2(markerLine.x, markerLine.y), Constants.MARKER_SIZE);

            Rect fullRect = new(markerLine.x, markerLine.y, markerTop.width, markerLine.height);

            return new()
            {
                fullRect,
                markerTop,
                markerLine
            };
        }

        /// <summary>
        /// Draws text from a marker at a given rect with the right colors.
        /// </summary>
        /// <param name="previewWindowRect">The complete rect of the preview window.</param>
        /// <param name="markerRects">List of rects for the marker (top part, middle part and full collision part used for hover detection).</param>
        /// <param name="index">The index of the marker that is being displayed.</param>
        void DrawMarkerText(Rect previewWindowRect, List<Rect> markerRects, int index)
        {
            DialogueItem marker = _clip.Dialogue[index];

            Line line = marker.Text.GetLineInfo(_language);

            string text = marker.Text.GetLine(_language);
            Rect labelRect = markerRects[0];
            labelRect.x += 2;
            labelRect.y = markerRects[0].size.y / 2;
            labelRect.height = EditorGUIUtility.singleLineHeight;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                richText = true
            };

            // Styling of the subtitles.
            if (line.Bold && line.Italic)
            {
                style.fontStyle = FontStyle.BoldAndItalic;
            }
            else if (line.Bold)
            {
                style.fontStyle = FontStyle.Bold;
            }
            else if (line.Italic)
            {
                style.fontStyle = FontStyle.Italic;
            }

            if (line.UseColor)
            {
                style.normal.textColor = line.Color;
            }

            // Calculate the width of the label field.
            {
                float width = previewWindowRect.width - labelRect.x;
                if (_clip.Dialogue.Count < index + 1)
                {
                    float nextMarkerX = CalculateMarkerPosition(previewWindowRect, _clip.Dialogue[index + 1].SamplePosition);
                    width = nextMarkerX - labelRect.x;
                }
                labelRect.width = width;
                labelRect.width -= 2;

                float actualWidth = style.CalcSize(new GUIContent(text)).x;
                if (actualWidth < labelRect.width)
                {
                    labelRect.width = actualWidth;
                }
            }

            EditorGUI.DrawRect(labelRect, new Color32(0, 0, 0, 155));
            EditorGUI.LabelField(labelRect, new GUIContent(text), style);
        }

        /// <summary>
        /// Draws a marker at a given rect with the right colors.
        /// </summary>
        /// <param name="markerRects">List of rects for the marker (top part, middle part and full collision part used for hover detection).</param>
        /// <param name="index">The index of the marker that is being displayed.</param>
        void DrawMarkerRect(List<Rect> markerRects, int index)
        {
            DialogueItem marker = _clip.Dialogue[index];

            Line line = marker.Text.GetLineInfo(_language);

            Color color = USubtitleEditorVariables.Preferences.Color_LineMarker;
            if (line.NewLine)
            {
                color = USubtitleEditorVariables.Preferences.Color_NewLineMarker;
            }
            Event evt = Event.current;
            if (markerRects[0].Contains(evt.mousePosition))
            {
                color = new Color(color.r * 1.3f, color.g * 1.3f, color.b * 1.2f);
            }
            if (index == _interaction.Index)
            {
                color = new Color(color.r * 1.5f, color.g * 1.5f, color.b * 1.5f);
            }
            EditorGUI.DrawRect(markerRects[1], color);
            EditorGUI.DrawRect(markerRects[2], color);
        }

        /// <param name="previewWindowRect">The complete rect of the preview window.</param>
        /// <param name="zoomedPreviewWindowRect">The zoomed in rect of the preview window (based on the timeline).</param>
        /// <param name="markerRects">List of rects for the marker (top part, middle part and full collision part used for hover detection).</param>
        /// <param name="index">The index of the marker that is being displayed.</param>
        private void HandleMarkerEvents(Rect previewWindowRect, Rect zoomedPreviewWindowRect, List<Rect> markerRects, int index)
        {
            DialogueItem marker = _clip.Dialogue[index];

            Event evt = Event.current;

            if (_interaction.Index != -1 && _interaction.Index != index && _interaction.PreviousEventType == EventType.MouseDrag)
            {
                return;
            }

            if (_interaction.CurrentInteraction == InteractionType.TimelineInteraction_Timeline)
            {
                return;
            }

            switch (evt.type)
            {
                case EventType.MouseDown:
                {
                    if (markerRects[0].Contains(evt.mousePosition))
                    {
                        // Left click OR right click.
                        if (evt.button == 0 || evt.button == 1)
                        {
                            // Set this marker as the current marker and drag it to the new position.
                            _interaction.SetInteraction(InteractionType.TimelineInteraction_TimeMarker);
                            _interaction.SetIndex(index);
                            _interaction.SetDragIndex(index);
                            if (evt.button == 1)
                            {
                                GenericMenu menu = GetMarkerMenu();
                                menu.ShowAsContext();
                            }
                            evt.Use();

                            _interaction.SetEventType(EventType.MouseDown);
                        }
                    }

                    break;
                }
                case EventType.MouseDrag:
                {
                    if (evt.button == 0 && _interaction.DragIndex == index)
                    {
                        float rectPos = _clip.Clip.length / zoomedPreviewWindowRect.size.x * (_scrollPos.x + evt.mousePosition.x);
                        uint pos = CalculateSamples(rectPos);
                        marker.SamplePosition = pos;
                        evt.Use();

                        _interaction.SetEventType(EventType.MouseDrag);
                    }
                    break;
                }
                case EventType.MouseUp:
                {
                    if (evt.button != 0)
                    {
                        return;
                    }

                    if (_interaction.PreviousEventType == EventType.MouseDrag)
                    {
                        DialogueItem item = _clip.Dialogue[_interaction.Index];
                        uint pos = item.SamplePosition;

                        ReorderTimeMarkers();

                        index = _clip.Dialogue.FindIndex(x => x.SamplePosition == pos);
                        _interaction.SetIndex(index);
                        _interaction.SetDragIndex(index);
                    }

                    _interaction.SetInteraction(InteractionType.TimelineInteraction_None);
                    _interaction.SetEventType(EventType.MouseUp);
                    evt.Use();
                    break;
                }
            }
        }

        /// <summary>
        /// Reorders all time markers based on their sample position.
        /// </summary>
        private void ReorderTimeMarkers()
        {
            _clip.Dialogue.Sort((x, y) => x.SamplePosition.CompareTo(y.SamplePosition));
            serializedObject.Update();
            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// This draws all time markers. These are the markers the user creates with the SamplePosition and labels and such.
        /// </summary>
        /// <param name="previewWindowRect">The complete rect of the preview window.</param>
        /// <param name="zoomedPreviewWindowRect">The zoomed in rect of the preview window (based on the audio player).</param>
        private void DrawMarkers(Rect previewWindowRect, Rect zoomedPreviewWindowRect)
        {
            for (int i = 0; i < _clip.Dialogue.Count; i++)
            {
                List<Rect> markerRects = GetMarkerRect(previewWindowRect, _clip.Dialogue[i]);

                DrawMarkerText(previewWindowRect, markerRects, i);
                DrawMarkerRect(markerRects, i);
                HandleMarkerEvents(previewWindowRect, zoomedPreviewWindowRect, markerRects, i);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _audioPlayer.Update();

            GUIStyle headerTextStyle = new GUIStyle();
            headerTextStyle.fontStyle = FontStyle.Bold;
            headerTextStyle.fontSize = 15;
            headerTextStyle.normal.textColor = Color.white;

            EditorGUILayout.BeginVertical("GroupBox", GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Clip Info", headerTextStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Clip"), new GUIContent("Clip"));
            EditorGUI.indentLevel--;
            EditorGUILayout.EndHorizontal();

            if (_clip && _clip.Clip != null)
            {
                if (_interaction.Index != -1 && _interaction.Index < _clip.Dialogue.Count)
                {
                    var _currentDialogueItem = serializedObject.FindProperty("Dialogue").GetArrayElementAtIndex(_interaction.Index);

                    EditorGUILayout.BeginVertical("GroupBox", GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField("Current Dialogue", headerTextStyle);
                    EditorGUI.indentLevel++;
                    _ = EditorGUILayout.PropertyField(_currentDialogueItem, new GUIContent("Current DialogueItem"));
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.BeginVertical("GroupBox", GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Lines", headerTextStyle);
            List<string> sentences = _clip.GetSentences(_language);
            EditorGUI.indentLevel++;
            for (int i = 0; i < sentences.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUIStyle style = new GUIStyle();
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.white;
                string sentence = $"{i + 1}. {sentences[i]}";
                GUIStyle textStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true
                };
                EditorGUILayout.LabelField(sentence, textStyle);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            _ = serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// This returns the preview string based on the position of the position of the start selection marker or the played audio.
        /// </summary>
        /// <param name="position"></param>
        private string UpdateTextPreview(float position)
        {
            List<string> sentences = _clip.GetSentencesTillSamplePosition(_language, (uint)position);
            if (sentences.Count > 0)
            {
                return sentences[^1];
            }
            return "";
        }

        [MenuItem("Assets/Create/UAudioClip", priority = 1)]
        private static void CreateUAudioClipFromAudioClip()
        {
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                UnityEngine.Object obj = Selection.objects[i];

                UAudioClip uaudioClip = ScriptableObject.CreateInstance<UAudioClip>();
                uaudioClip.Clip = obj as AudioClip;
                string path = AssetDatabase.GetAssetPath(obj);
                int fileExtPos = path.LastIndexOf(".");
                if (fileExtPos >= 0)
                    path = path.Substring(0, fileExtPos);
                string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + ".asset");

                AssetDatabase.CreateAsset(uaudioClip, assetPathAndName);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
            }
        }

        [MenuItem("Assets/Create/UAudioClip", true)]
        private static bool CreateUAudioClipFromAudioClipValidation() => Selection.activeObject is AudioClip;
    }
}