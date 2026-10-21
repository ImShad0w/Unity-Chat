using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using NativeWebSocket;
using System.Collections.Generic;


public class WebSocketClient : MonoBehaviour
{

  [Header("WebSocket config")]
  [SerializeField]
  private readonly string websocketUrl = "ws://localhost:8000";

  [SerializeField] UIDocument uiDocument;
  private VisualElement root;

  private List<Message> _messages = new List<Message>();

  //Init the UI elements
  private ListView _chatMessages;
  private VisualElement _messageField;
  private VisualElement _nicknameField;
  private TextField _nickname;
  private TextField _message;
  private Button _sendNick;
  private Button _sendMsg;

  private bool hasNickname = false;

  private static readonly JsonSerializerOptions MessageJsonOptions = new JsonSerializerOptions
  {
    IncludeFields = true
  };

  //Store the websocket
  WebSocket websocket;
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  async void Start()
  {
    //Init the document fields
    root = uiDocument.rootVisualElement;

    _chatMessages = root.Q<ListView>("ChatMessages");
    _nicknameField = root.Q<VisualElement>("NickContainer");
    _messageField = root.Q<VisualElement>("InputContainer");
    _nickname = root.Q<TextField>("Nickname");
    _message = root.Q<TextField>("ChatInput");
    _sendNick = root.Q<Button>("AddNickname");
    _sendMsg = root.Q<Button>("SendMessage");

    _chatMessages.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
    _chatMessages.makeItem = MakeMessageItem;
    _chatMessages.bindItem = BindMessageItem;
    _chatMessages.itemsSource = _messages;

    _sendNick.clicked += SendWebSocketNickname;
    _sendMsg.clicked += SendWebSocketMessage;

    //Hide the input field for now till the user doesn't provide with a nickname
    HideInputTextbox();

    await ConnectWebSocket();
  }
  private async Task ConnectWebSocket()
  {
    websocket = new WebSocket(websocketUrl);
    websocket.OnOpen += () =>
    {
      Debug.Log("WebSocket connectat!");
    };

    websocket.OnError += (e) =>
    {
      Debug.LogError("WebSocket error: " + e);
    };

    websocket.OnClose += (e) =>
    {
      Debug.Log("WebSocket tancat!");
    };

    // Callback: Quan es rep un missatge.
    websocket.OnMessage += (bytes) =>
    {
      // Converteix els bytes a una cadena.
      string message = Encoding.UTF8.GetString(bytes);
      Debug.Log("Missatge rebut: " + message);
      RenderMessage(message);
    };
    // Inicia la connexió de manera asíncrona.
    await websocket.Connect();
  }

  private void HideInputTextbox()
  {
    //Hide the textbox
    _messageField.style.display = hasNickname ? DisplayStyle.Flex : DisplayStyle.None;
    //Hide also the input for the nickname
    _nicknameField.style.display = hasNickname ? DisplayStyle.None : DisplayStyle.Flex;
  }

  public async void SendWebSocketMessage()
  {
    var msg = _message.value;

    if (websocket != null && websocket.State == WebSocketState.Open)
    {

      var message = new
      {
        @event = "message",
        data = msg
      };

      await websocket.SendText(JsonSerializer.Serialize(message));
      Debug.Log("Missatge enviat: " + message);
    }
    else
    {
      Debug.LogWarning("No es pot enviar el missatge: connexió no oberta.");
    }
  }

  public async void SendWebSocketNickname()
  {
    //Get the value
    var nickname = _nickname.value;

    if (websocket != null && websocket.State == WebSocketState.Open)
    {

      var message = new
      {
        @event = "nickname",
        data = nickname
      };

      await websocket.SendText(JsonSerializer.Serialize(message));
      Debug.Log("Missatge enviat: " + nickname);

      hasNickname = true;

      HideInputTextbox();
    }
    else
    {
      Debug.LogWarning("No es pot enviar el missatge: connexió no oberta.");
    }
  }

  private void RenderMessage(string message)
  {
    Message parsed;

    try
    {
      parsed = JsonSerializer.Deserialize<Message>(message, MessageJsonOptions);
    }
    catch (JsonException e)
    {
      Debug.LogWarning("Missatge no vàlid: " + message + "\n" + e.Message);
      return;
    }

    if (parsed == null || string.IsNullOrEmpty(parsed.message))
    {
      return;
    }

    if (string.IsNullOrWhiteSpace(parsed.nickname))
    {
      parsed.nickname = string.IsNullOrWhiteSpace(_nickname?.value) ? "Anònim" : _nickname.value;
    }

    CreateMessage(parsed);
  }

  private void CreateMessage(Message message)
  {
    _messages.Add(message);

    _chatMessages.RefreshItems();
    _chatMessages.ScrollToItem(_messages.Count - 1);
  }

  private VisualElement MakeMessageItem()
  {
    var item = new VisualElement();
    item.AddToClassList("message-item");

    var nickname = new Label();
    nickname.AddToClassList("message-nickname");
    item.Add(nickname);

    var bubble = new VisualElement();
    bubble.AddToClassList("message-bubble");

    var text = new Label();
    text.AddToClassList("message-text");
    bubble.Add(text);

    item.Add(bubble);

    return item;
  }

  private void BindMessageItem(VisualElement element, int index)
  {
    var message = _messages[index];

    element.Q<Label>(className: "message-nickname").text = message.nickname;
    element.Q<Label>(className: "message-text").text = message.message;
  }

  // Update is called once per frame
  void Update()
  {
    if (websocket != null)
    {
      websocket.DispatchMessageQueue();
    }
  }

  private async void OnApplicationQuit()
  {
    if (websocket != null)
    {
      await websocket.Close();
    }
  }
}
