using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

public class RestResponse {

    public long code;
    public string text;
}
public class RestApiController : MonoBehaviour {
    public event Action<RestResponse> LoginResponded;
    public event Action<RestResponse> RegisterResponded;
    public event Action<RestResponse> AddMatchResponded;
    public event Action<RestResponse> GetMatchesResponded;


    private string _token;

    private const string c_loginURL = "https://localhost:7131/api/Login";
    private const string c_registerURL = "https://localhost:7131/api/Login/register";
    private const string c_addMatchURL = "https://localhost:7131/api/Matches/add";
    private const string c_getAllMatchesURL = "https://localhost:7131/api/Matches";
    private const string c_getUserMatchesURL = "https://localhost:7131/api/Matches/nickname";

    private void Awake() {

        Debug.Log("Add RestApiController to context");
        Context.Instance.RestApi = this;

        LoginResponded += OnLoginResponded;
    }

    private void OnDestroy() {
        
        LoginResponded -= OnLoginResponded;
    }

    public void SendLoginRequest(LoginRequest loginData) {

        string json = JsonUtility.ToJson(loginData);
        StartCoroutine(Post(c_loginURL, json, null, LoginResponded));
    }

    public void SendRegisterRequest(RegisterRequest registerData) {

        string json = JsonUtility.ToJson(registerData);
        StartCoroutine(Post(c_registerURL, json, null, RegisterResponded));
    }

    public void AddMatchRequest(MatchRestApi matchData) {

        MatchRestApi[] matches = new MatchRestApi[1];
        matches[0] = matchData;

        string json = JsonHelper.ToJson(matches);

        StartCoroutine(Post(c_addMatchURL, json, _token, AddMatchResponded));
    }

    public void GetMatchesRequest(MatchRequest matchRequest) {

        string json = JsonUtility.ToJson(matchRequest);
        StartCoroutine(Post(c_getUserMatchesURL,json,_token, GetMatchesResponded));
    }

    //private 

    private IEnumerator Post(string url, string json, string token, Action<RestResponse> afterMethod) {

        //Using the using statement ensures that the UnityWebRequest object is disposed of as soon as it goes out of scope, which helps prevent memory leaks.
        using (UnityWebRequest request = new UnityWebRequest(url, "POST")) {

            // Add the JWT token to the Authorization header
            if (token != null)
                request.SetRequestHeader("Authorization", "Bearer " + token);

            byte[] bodyRaw = new System.Text.UTF8Encoding(true).GetBytes(json);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();


            RestResponse response = new RestResponse();

            response.code = request.responseCode;
            response.text = request.downloadHandler.text;

            afterMethod?.Invoke(response);
        }
    }

    private IEnumerator Get(string url, string token, Action<RestResponse> afterMethod) {

        using (UnityWebRequest request = UnityWebRequest.Get(url)) {

            // Add the JWT token to the Authorization header
            if (token != null)
                request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            RestResponse response = new RestResponse();

            response.code = request.responseCode;
            response.text = request.downloadHandler.text;

            afterMethod?.Invoke(response);
        }
    }

    private void OnLoginResponded(RestResponse response) {

        if (response.code == 200) {

            LoginResponse a = JsonUtility.FromJson<LoginResponse>(response.text);
            _token = a.token;
        }
    }
}

