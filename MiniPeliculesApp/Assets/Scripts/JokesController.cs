using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class JokesController : MonoBehaviour
{
  [SerializeField] UIDocument uiDocument;
  [SerializeField] private NetworkManager networkManager;
  private VisualElement root;

  private List<Joke> _jokes = new List<Joke>();

  private TextField _setupField;
  private TextField _punchField;
  private ListView _jokesContainer;
  private Button _addJoke;
  private Label _descSetupLabel;
  private Label _descPunchLabel;

  // When set, the form edits this joke (PUT); when null it creates a new one (POST)
  private Joke _editingJoke;

  private const string AddButtonText = "Afegir broma";
  private const string SaveButtonText = "Desar";

  void Start()
  {
    root = uiDocument.rootVisualElement;

    _setupField = root.Q<TextField>("setupField");
    _punchField = root.Q<TextField>("punchField");

    _jokesContainer = root.Q<ListView>("jokesContainer");

    _descSetupLabel = root.Q<Label>("descSetupLabel");
    _descPunchLabel = root.Q<Label>("descPunchLabel");

    _addJoke = root.Q<Button>("addJoke");

    _jokesContainer.itemsSource = _jokes;

    // Creates the visual element used for each row
    _jokesContainer.makeItem = MakeJokeItem;

    // Gives each row its data
    _jokesContainer.bindItem = BindJokeItem;

    _addJoke.clicked += OnSaveClicked;

    ClearForm();

    //Call the netManager to get the jokes
    LoadJokes();
  }

  void LoadJokes()
  {
    networkManager.TestRequest(
      onSuccess: (serverJokes) =>
      {
        _jokes.Clear();
        _jokes.AddRange(serverJokes);

        // Refresh the UI after the data arrives
        _jokesContainer.RefreshItems();

        Debug.Log($"Loaded {_jokes.Count} jokes.");
      },
      onError: (error) =>
      {
        Debug.LogError($"Failed to load jokes: {error}");
      }
    );
  }

  // Builds one row: a label (opens the detail) plus modify and delete buttons
  VisualElement MakeJokeItem()
  {
    var row = new VisualElement();
    row.AddToClassList("jokeItem");
    row.style.flexDirection = FlexDirection.Row;
    row.style.alignItems = Align.Center;

    var label = new Label();
    label.name = "jokeLabel";
    label.style.flexGrow = 1;

    label.RegisterCallback<ClickEvent>(_ =>
    {
      var joke = row.userData as Joke;
      if (joke != null)
      {
        ShowJokeDetail(joke);
      }
    });

    // The row stores the joke as userData, so the buttons act on the bound item
    var modifyButton = new Button(() =>
    {
      var joke = row.userData as Joke;
      if (joke != null)
      {
        StartEdit(joke);
      }
    });
    modifyButton.name = "modifyButton";
    modifyButton.text = "Modificar";

    var deleteButton = new Button(() =>
    {
      var joke = row.userData as Joke;
      if (joke != null)
      {
        DeleteJoke(joke);
      }
    });
    deleteButton.name = "deleteButton";
    deleteButton.text = "Eliminar";

    row.Add(label);
    row.Add(modifyButton);
    row.Add(deleteButton);

    return row;
  }

  void BindJokeItem(VisualElement element, int index)
  {
    var joke = _jokes[index];

    element.userData = joke;

    var label = element.Q<Label>("jokeLabel");
    if (label != null)
    {
      label.text = joke.setup;
    }
  }

  // Fired by the "Afegir broma" / "Desar" button
  void OnSaveClicked()
  {
    string setup = _setupField.value;
    string punch = _punchField.value;

    if (string.IsNullOrWhiteSpace(setup) || string.IsNullOrWhiteSpace(punch))
    {
      Debug.LogWarning("Cal omplir el setup i el punchline.");
      return;
    }

    if (_editingJoke == null)
    {
      CreateJoke(setup, punch);
    }
    else
    {
      UpdateJoke(setup, punch);
    }
  }

  // CREATE
  void CreateJoke(string setup, string punch)
  {
    var newJoke = new Joke(setup, punch);

    networkManager.CreateJoke(
      newJoke,
      onSuccess: (created) =>
      {
        _jokes.Add(created);

        // Refresh the ListView
        _jokesContainer.RefreshItems();

        ClearForm();

        Debug.Log($"Created joke #{created.id}.");
      },
      onError: (error) => Debug.LogError($"Failed to create joke: {error}")
    );
  }

  // UPDATE
  void UpdateJoke(string setup, string punch)
  {
    _editingJoke.setup = setup;
    _editingJoke.punchline = punch;

    networkManager.UpdateJoke(
      _editingJoke,
      onSuccess: (updated) =>
      {
        int index = _jokes.FindIndex(joke => joke.id == updated.id);
        if (index >= 0)
        {
          _jokes[index] = updated;
        }

        _jokesContainer.RefreshItems();

        ClearForm();

        Debug.Log($"Updated joke #{updated.id}.");
      },
      onError: (error) => Debug.LogError($"Failed to update joke: {error}")
    );
  }

  // DELETE
  void DeleteJoke(Joke joke)
  {
    networkManager.DeleteJoke(
      joke.id,
      onSuccess: (deleted) =>
      {
        _jokes.RemoveAll(item => item.id == deleted.id);

        _jokesContainer.RefreshItems();

        if (_editingJoke != null && _editingJoke.id == deleted.id)
        {
          ClearForm();
        }

        Debug.Log($"Deleted joke #{deleted.id}.");
      },
      onError: (error) => Debug.LogError($"Failed to delete joke: {error}")
    );
  }

  // Loads a joke into the form and switches the button into "save" mode
  void StartEdit(Joke joke)
  {
    _editingJoke = joke;
    _setupField.value = joke.setup;
    _punchField.value = joke.punchline;
    _addJoke.text = SaveButtonText;

    ShowJokeDetail(joke);
  }

  void ClearForm()
  {
    _editingJoke = null;
    _setupField.value = "";
    _punchField.value = "";
    _addJoke.text = AddButtonText;
  }

  void ShowJokeDetail(Joke joke)
  {
    if (_descSetupLabel != null)
    {
      _descSetupLabel.text = joke.setup;
    }

    if (_descPunchLabel != null)
    {
      _descPunchLabel.text = joke.punchline;
    }
  }
}
