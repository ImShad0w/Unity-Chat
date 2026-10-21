using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class HelloRequest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(GetHello());
    }

    IEnumerator GetHello()
    {
        using (UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/hello"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);
            }
        }
    }
}