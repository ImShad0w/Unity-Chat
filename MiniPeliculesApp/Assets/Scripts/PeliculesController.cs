using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PeliculesController : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;

    private VisualElement root;

    private List<Pelicula> _pelicules = new List<Pelicula>();

    void Start()
    {
        root = uiDocument.rootVisualElement;

        var nameField = root.Q<TextField>("nameField");
        var descField = root.Q<TextField>("descField");

        var peliculesContainer = root.Q<ListView>("jokesContainer");

        var selectedTitle = root.Q<Label>("descSetupLabel");
        var selectedText = root.Q<Label>("descPunchLabel");

        var addBtn = root.Q<Button>("addPeliculaButton");

        peliculesContainer.itemsSource = _pelicules;

        // Creates the visual element used for each row
        peliculesContainer.makeItem = () =>
        {
            var button = new Button();

            button.AddToClassList("peliculaItem");

            button.clicked += () =>
            {
                var pelicula = button.userData as Pelicula;

                if (pelicula != null)
                {
                    ShowPeliculaDetail(pelicula);
                }
            };

            return button;
        };

        // Gives each row its data
        peliculesContainer.bindItem = (element, index) =>
        {
            var button = (Button)element;
            var pelicula = _pelicules[index];

            button.text = pelicula.name;
            button.userData = pelicula;
        };

        addBtn.clicked += () =>
        {
            string name = nameField.value;
            string desc = descField.value;

            var newPelicula = new Pelicula(name, desc);

            _pelicules.Add(newPelicula);

            // Refresh the ListView
            peliculesContainer.RefreshItems();
        };


        selectedTitle.text = "";
        selectedText.text = "";
    }


    void ShowPeliculaDetail(Pelicula pelicula)
    {
        var selectedTitle = root.Q<Label>("descNameLabel");
        var selectedText = root.Q<Label>("descTextLabel");

        selectedTitle.text = pelicula.name;
        selectedText.text = pelicula.desc;
    }
}
