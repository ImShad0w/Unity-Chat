using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


public class NetworkManager : MonoBehaviour
{
  // URL del servidor Node
  [SerializeField] private string url = "http://localhost:3000/jokes";

  private void Start()
  {
  }

  public void TestRequest(Action<Joke[]> onSuccess = null, Action<string> onError = null)
  {
    StartCoroutine(GetJokesFromServer(onSuccess, onError));
  }
  [ContextMenu("Test HTTP /(GET hello)")]
  public void ContextMenuTestRequest()
  {
    if (!Application.isPlaying)
    {
      Debug.LogWarning("Entra en Play Mode per fer la petició HTTP.");
      return;
    }
    TestRequest();
  }
  private IEnumerator GetJokesFromServer(
    Action<Joke[]> onSuccess,
    Action<string> onError)
  {
    Debug.Log($"Sending GET request to: {url}");

    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
      yield return request.SendWebRequest();

      if (request.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError($"Request error: {request.error}");
        onError?.Invoke(request.error);
      }
      else
      {
        string responseText = request.downloadHandler.text;

        // Wrap the raw JSON array for JsonUtility
        string wrappedJson = "{\"jokes\":" + responseText + "}";

        JokeList jokeList =
            JsonUtility.FromJson<JokeList>(wrappedJson);

        onSuccess?.Invoke(jokeList.jokes);
      }
    }
  }

  public void GetJoke(int id, Action<Joke> onSuccess = null, Action<string> onError = null)
  {
    StartCoroutine(GetJokeFromServer(id, onSuccess, onError));
  }

  private IEnumerator GetJokeFromServer(
    int id,
    Action<Joke> onSuccess,
    Action<string> onError)
  {
    Debug.Log($"Sending GET request to: {url}/{id}");

    using (UnityWebRequest request = UnityWebRequest.Get($"{url}/{id}"))
    {
      yield return request.SendWebRequest();

      if (request.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError($"Request error: {request.error}");
        onError?.Invoke(request.error);
      }
      else
      {
        onSuccess?.Invoke(JsonUtility.FromJson<Joke>(request.downloadHandler.text));
      }
    }
  }

  public void CreateJoke(Joke joke, Action<Joke> onSuccess = null, Action<string> onError = null)
  {
    StartCoroutine(CreateJokeOnServer(joke, onSuccess, onError));
  }

  private IEnumerator CreateJokeOnServer(
    Joke joke,
    Action<Joke> onSuccess,
    Action<string> onError)
  {
    string json = JsonUtility.ToJson(joke);
    Debug.Log($"Sending POST request to: {url}");

    using (UnityWebRequest request = UnityWebRequest.Post(url, json, "application/json"))
    {
      yield return request.SendWebRequest();

      if (request.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError($"Request error: {request.error}");
        onError?.Invoke(request.error);
      }
      else
      {
        onSuccess?.Invoke(JsonUtility.FromJson<Joke>(request.downloadHandler.text));
      }
    }
  }

  public void UpdateJoke(Joke joke, Action<Joke> onSuccess = null, Action<string> onError = null)
  {
    StartCoroutine(UpdateJokeOnServer(joke, onSuccess, onError));
  }

  private IEnumerator UpdateJokeOnServer(
    Joke joke,
    Action<Joke> onSuccess,
    Action<string> onError)
  {
    string json = JsonUtility.ToJson(joke);
    Debug.Log($"Sending PUT request to: {url}/{joke.id}");

    using (UnityWebRequest request = UnityWebRequest.Put($"{url}/{joke.id}", json))
    {
      request.SetRequestHeader("Content-Type", "application/json");

      yield return request.SendWebRequest();

      if (request.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError($"Request error: {request.error}");
        onError?.Invoke(request.error);
      }
      else
      {
        onSuccess?.Invoke(JsonUtility.FromJson<Joke>(request.downloadHandler.text));
      }
    }
  }

  public void DeleteJoke(int id, Action<Joke> onSuccess = null, Action<string> onError = null)
  {
    StartCoroutine(DeleteJokeOnServer(id, onSuccess, onError));
  }

  private IEnumerator DeleteJokeOnServer(
    int id,
    Action<Joke> onSuccess,
    Action<string> onError)
  {
    Debug.Log($"Sending DELETE request to: {url}/{id}");

    using (UnityWebRequest request = UnityWebRequest.Delete($"{url}/{id}"))
    {
      request.downloadHandler = new DownloadHandlerBuffer();

      yield return request.SendWebRequest();

      if (request.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError($"Request error: {request.error}");
        onError?.Invoke(request.error);
      }
      else
      {
        onSuccess?.Invoke(JsonUtility.FromJson<Joke>(request.downloadHandler.text));
      }
    }
  }
}
