[System.Serializable]
public class Joke
{
  public int id;
  public string type;
  public string setup;
  public string punchline;

  public Joke() { }

  public Joke(string setup, string punchline)
  {
    this.setup = setup;
    this.punchline = punchline;
  }

  public Joke(string type, string setup, string punchline)
  {
    this.type = type;
    this.setup = setup;
    this.punchline = punchline;
  }
}

[System.Serializable]
public class JokeList
{
  public Joke[] jokes;
}
