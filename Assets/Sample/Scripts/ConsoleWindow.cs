using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-10000)]
[RequireComponent(typeof(UIDocument))]
public class ConsoleWindow : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;

    [Header("MessageItem")]
    [SerializeField] private VisualTreeAsset _messageItemTemplate;
    [SerializeField] private Texture2D _logIcon;
    [SerializeField] private Texture2D _warningIcon;
    [SerializeField] private Texture2D _errorIcon;
    [SerializeField] private Color _color1 = new Color(0.1f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color _color2 = new Color(0.2f, 0.2f, 0.2f, 1f);

    private Button _clearButton;
    private Button _logButton;
    private Button _warningButton;
    private Button _errorButton;
    private ListView _messageListView;
    private Scroller _messageListScroller;

    private readonly List<(string timeCode, string Condition, string StackTrace, LogType Type)> _messages = new();

    private void Awake()
    {
        Application.logMessageReceived += OnLogMessageReceived;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
    }

    private void Start()
    {
        var root = _uiDocument.rootVisualElement;

        _clearButton = root.Q<Button>("clear-button");
        _logButton = root.Q<Button>("log-button");
        _warningButton = root.Q<Button>("warning-button");
        _errorButton = root.Q<Button>("error-button");
        _messageListView = root.Q<ListView>("message-list");
        _messageListScroller = _messageListView.Q<Scroller>();

        // setup buttons
        _clearButton.clicked += OnClearButtonClick;

        // setup logListView
        _messageListView.itemsSource = _messages;
        _messageListView.makeItem = () => _messageItemTemplate.Instantiate();
        _messageListView.bindItem = OnItemBind;

        // setup scroller
        _messageListScroller.value = _messageListScroller.highValue;
    }

    private void UpdateView()
    {
        if (_messageListView == null
            || _logButton == null
            || _warningButton == null
            || _errorButton == null)
        {
            return;
        }

        _messageListView.RefreshItems();

        _logButton.text = _messages
            .Count(x => x.Type == LogType.Log)
            .ToString();

        _warningButton.text = _messages
            .Count(x => x.Type == LogType.Warning)
            .ToString();

        _errorButton.text = _messages
            .Count(x => (x.Type == LogType.Error)
                    || (x.Type == LogType.Exception)
                    || (x.Type == LogType.Assert))
            .ToString();

        // scroll to bottom
        if (_messageListScroller.value + _messageListView.fixedItemHeight >= _messageListScroller.highValue)
        {
            _messageListScroller.value = _messageListScroller.highValue;
        }
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        _messages.Add((DateTime.Now.ToString("HH:mm:ss"), condition, stackTrace, type));
        UpdateView();
    }

    private void OnClearButtonClick()
    {
        _messages.Clear();
        UpdateView();
    }

    private void OnItemBind(VisualElement element, int index)
    {
        var (timeCode, condition, stackTrace, type) = _messages[index];

        element.Q<VisualElement>("message-container").style.backgroundColor = index % 2 == 0 ? _color1 : _color2;

        var message = $"[{timeCode}] {condition}\n{stackTrace.Split('\n')[0]}";
        element.Q<Label>("text").text = message;

        element.Q<VisualElement>("icon").style.backgroundImage = type switch
        {
            LogType.Log => _logIcon,
            LogType.Warning => _warningIcon,
            LogType.Error => _errorIcon,
            LogType.Assert => _errorIcon,
            LogType.Exception => _errorIcon,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
