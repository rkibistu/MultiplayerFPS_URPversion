using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class TestingRestApi : MonoBehaviour {

    string _token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoidXNlcjEiLCJuYmYiOjE2Nzk3NjM3MjQsImV4cCI6MTY3OTc2NDMyNCwiaWF0IjoxNjc5NzYzNzI0fQ.NZEYngYUet-rc7lAqgtQ86XhSdKjwyV-MmBAtScaqGQ";
    void Start() {
        //StartCoroutine(GetMatches("https://localhost:7131/api/Matches", _token));

        //createPostRequest(_token);
    }

    private void Update() {

        //if (Input.GetKeyDown(KeyCode.Space)) {

        //    createPostMatchRequest(_token);
        //}
        //if (Input.GetKeyDown(KeyCode.L)) {

        //    createLoginRequestPost();
        //}
        //if (Input.GetKeyDown(KeyCode.R)) {

        //    createRegisterRequestPost();
        //}
    }
    IEnumerator GetMatches(string url, string token) {
        UnityWebRequest www = UnityWebRequest.Get(url);

        // Add the JWT token to the Authorization header
        www.SetRequestHeader("Authorization", "Bearer " + token);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {

            string a = www.downloadHandler.text;
            Debug.Log(a);


            MatchRestApi[] b;
            b = JsonHelper.getJsonArray<MatchRestApi>(a);


            foreach (MatchRestApi match in b) {
                Debug.Log(match.startTime);
                Debug.Log(match.winnerNickname);
            }
        }
    }


    IEnumerator Post(string url, string json, string token) {

        UnityWebRequest request = new UnityWebRequest(url, "POST");

        // Add the JWT token to the Authorization header
        if (token != null)
            request.SetRequestHeader("Authorization", "Bearer " + token);

        byte[] bodyRaw = new System.Text.UTF8Encoding(true).GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success) {
            Debug.Log("Error: " + request.error);
        }
        else {
            Debug.Log("Success");
            Debug.Log(request.downloadHandler.text);
        }
    }

    private void createPostMatchRequest(string token) {

        MatchRestApi match = CreateSampleAddMatchRequest();

        MatchRestApi[] matches = new MatchRestApi[1];
        matches[0] = match;

        string json = JsonHelper.ToJson(matches);


        //Debug.Log(json);

        StartCoroutine(Post("https://localhost:7131/api/Matches/add", json, token));
    }

    private void createLoginRequestPost() {

        LoginRequest login = new LoginRequest();
        login.nickname = "user1";
        login.password = "pass1";

        string json = JsonUtility.ToJson(login);

        Debug.Log(json);

        StartCoroutine(Post("https://localhost:7131/api/Login", json, null));
    }

    private void createRegisterRequestPost() {

        RegisterRequest register = new RegisterRequest();
        register.nickname = "user3";
        register.password = "pass3";

        string json = JsonUtility.ToJson(register);

        Debug.Log(json);

        StartCoroutine(Post("https://localhost:7131/api/Login/register", json, null));
    }


    private MatchRestApi CreateSampleAddMatchRequest() {

        MatchRestApi matchRequest = new MatchRestApi();

        matchRequest.gameType = "Deathmatch";
        matchRequest.startTime = DateTime.Now.ToString();
        matchRequest.endTime = DateTime.Now.ToString();
        matchRequest.completed = true;
        matchRequest.winnerNickname = "user1";

        matchRequest.users = new UserRestApi[2];
        matchRequest.users[0] = (CreateUserSample());
        matchRequest.users[1] = (CreateUserSample());

        return matchRequest;
    }

    private UserRestApi CreateUserSample() {

        UserRestApi user = new UserRestApi();

        user.nickname = "user1";
        user.team = "None";
        user.completed = true;

        user.score = CreateUserScoreSample();

        return user;
    }

    private ScoreRestApi CreateUserScoreSample() {

        ScoreRestApi score = new ScoreRestApi();

        score.kills = 5;
        score.deaths = 4;
        score.assists = 0;
        score.score = 100;

        return score;
    }
}
