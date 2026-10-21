[System.Serializable]
public class Message
{
  public string nickname;
  public string message;

  public Message() { }

  public Message(string nickname, string message)
  {
    this.nickname = nickname;
    this.message = message;
  }
}

[System.Serializable]
public class MessageList
{
  public Message[] jokes;
}
