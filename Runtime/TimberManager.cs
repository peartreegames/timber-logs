using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PeartreeGames.TimberLogs
{
    public class TimberManager : MonoBehaviour
    {
        [SerializeField] private Texture2D locatorImage;

        public static readonly Dictionary<GameObject, TimberMessages> Timbers = new();
        private static readonly List<TimberMessages> ActiveTimbers = new();

        private Camera _cam;
        private bool _isSearchOpen;
        private bool _isFilterOpen;
        private int _searchIndex;
        private int _timberIndex;
        private string _inputText;
        private TimberMessages[] _searchResults;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            var go = Instantiate(Resources.Load<GameObject>("p_TimberManager"));
            go.name = "TimberManager";
            DontDestroyOnLoad(go);
        }
#endif
        private void Awake()
        {
            _searchResults = Array.Empty<TimberMessages>();
        }

        private void Start()
        {
            _cam = Camera.main;
        }

        private enum InputType
        {
            Filter,
            Search
        }

        private const string FileName = "/TimberLogs.txt";
        private void OnDestroy()
        {
            using var writer = new StreamWriter(Application.persistentDataPath + FileName, false);
            foreach (var timber in Timbers.Values)
            {
                writer.WriteLine($"==={timber.Name}===");
                for (var i = 0; i < timber.Messages.Count; i++) writer.WriteLine(timber.Messages[i]);
                writer.WriteLine("\n");
            }

            writer.Close();
            Timbers.Clear();
            ActiveTimbers.Clear();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var activeInput = keyboard.ctrlKey.isPressed && keyboard.shiftKey.isPressed;
            if (activeInput && keyboard.lKey.wasPressedThisFrame) ToggleInput(!_isSearchOpen, InputType.Search);

            if (_isSearchOpen)
            {
                if (keyboard.escapeKey.wasPressedThisFrame) ToggleInput(false, InputType.Search);

                if (_inputText.Length == 0) return;
                if (keyboard.upArrowKey.wasPressedThisFrame) _searchIndex = Mathf.Max(_searchIndex - 1, 0);
                if (keyboard.downArrowKey.wasPressedThisFrame)
                    _searchIndex = Mathf.Min(_searchIndex + 1, _searchResults.Length - 1);
                if (keyboard.enterKey.wasPressedThisFrame)
                {
                    ActiveTimbers.Add(_searchResults[_searchIndex]);
                    ToggleInput(false, InputType.Search);
                }
            }

            if (_isFilterOpen)
            {
                if (keyboard.enterKey.wasPressedThisFrame) ToggleInput(false, InputType.Filter);
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    ActiveTimbers[_timberIndex].Filter = ActiveTimbers[_timberIndex].PreviousFilter;
                    ToggleInput(false, InputType.Filter);
                }
                
            }

            if (!activeInput || ActiveTimbers.Count == 0) return;
            if (keyboard.leftArrowKey.wasPressedThisFrame) _timberIndex = Mathf.Max(_timberIndex - 1, 0);
            if (keyboard.rightArrowKey.wasPressedThisFrame)
                _timberIndex = Mathf.Min(_timberIndex + 1, ActiveTimbers.Count - 1);

            if (keyboard.fKey.wasPressedThisFrame) ToggleInput(true, InputType.Filter);

            if (_isFilterOpen) return;
            if (keyboard.wKey.wasPressedThisFrame)
            {
                ActiveTimbers.RemoveAt(_timberIndex);
                _timberIndex = Mathf.Max(_timberIndex - 1, 0);
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                ActiveTimbers.Clear();
                _timberIndex = 0;
            }
            
        }

        private void ToggleInput(bool enable, InputType type)
        {
            switch (type)
            {
                case InputType.Search:
                    _isSearchOpen = enable;
                    _inputText = string.Empty;
                    break;
                case InputType.Filter:
                    _isFilterOpen = enable;
                    var activeTimber = ActiveTimbers[_timberIndex];
                    activeTimber.PreviousFilter = activeTimber.Filter;
                    _inputText = activeTimber.Filter;
                    break;
            }

            if (enable)
            {
                EventSystem.current.currentInputModule.DeactivateModule();
                Keyboard.current.onTextInput += OnTextInput;
            }
            else
            {
                EventSystem.current.currentInputModule.ActivateModule();
                Keyboard.current.onTextInput -= OnTextInput;
            }
        }

        private void OnTextInput(char c)
        {
            if (c == '\b' && _inputText.Length > 0) _inputText = _inputText[..^1];
            if (char.IsLetterOrDigit(c) || c is '_' or '-')
            {
                _inputText += c;
                _searchIndex = 0;
            }

            if (_isFilterOpen) ActiveTimbers[_timberIndex].Filter = _inputText;
        }

        private void OnGUI()
        {
            if (_isSearchOpen)
            {
                var locatorPos = Vector2.zero;
                var rect = new Rect(Screen.width / 2f - 250, Screen.height / 2f - 20, 500, Screen.height / 2f);
                var style = GUI.skin.textArea;
                style.fontSize = 20;
                style.padding = new RectOffset(10, 5, 5, 5);
                GUILayout.BeginArea(rect);
                GUILayout.TextField(_inputText, style);
                if (_inputText != string.Empty)
                {
                    GUILayout.BeginVertical();
                    var regex = new Regex($"{_inputText}", RegexOptions.IgnoreCase);
                    _searchResults = Timbers.Values.Where(t =>
                        regex.IsMatch(t.Name) && !ActiveTimbers.Exists(a => a.GameObject == t.GameObject)).ToArray();
                    for (int i = 0; i < _searchResults.Length; i++)
                    {
                        var result = _searchResults[i];
                        var timberName = result.Name;
                        if (i == _searchIndex)
                        {
                            timberName = " > " + timberName;
                            if (result.GameObject != null)
                            {
                                locatorPos = _cam.WorldToScreenPoint(result.GameObject.transform.position);
                                locatorPos.y = Screen.height - locatorPos.y - locatorImage.height / 2f;
                                locatorPos.x -= locatorImage.width / 2f;
                            }
                        }

                        GUILayout.TextField(timberName);
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.EndArea();
                if (locatorPos != default)
                    GUI.DrawTexture(new Rect(locatorPos.x, locatorPos.y, locatorImage.width, locatorImage.height),
                        locatorImage);
                return;
            }

            var labelStyle = GUI.skin.label;
            labelStyle.wordWrap = true;
            GUILayout.BeginArea(new Rect(25, 25, Screen.width - 50, Screen.height - 50));
            GUILayout.BeginHorizontal();

            var viewCount = Mathf.FloorToInt(Screen.height - 50 / labelStyle.lineHeight);
            for (var i = 0; i < ActiveTimbers.Count; i++)
            {
                var timber = ActiveTimbers[i];
                GUILayout.BeginVertical();
                
                var timberName = timber.Name;
                if (i == _timberIndex) timberName = $"==={timberName}===";
                GUILayout.Label(timberName);
                if (i == _timberIndex && _isFilterOpen) GUILayout.TextField(timber.Filter);
                else GUILayout.Label(timber.Filter);
                
                var regex = new Regex("temp");
                if (timber.Filter != string.Empty) regex = new Regex(timber.Filter, RegexOptions.IgnoreCase);
                for (var j = -1; j >= -Mathf.Min(timber.Messages.Count, viewCount); j--)
                {
                    if (timber.Filter != string.Empty && !regex.IsMatch(timber.Messages[j])) continue;
                    GUILayout.Label(timber.Messages[j], labelStyle);
                }
                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}