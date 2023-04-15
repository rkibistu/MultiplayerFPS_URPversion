//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;

//public class FakeLoginInitiator : MonoBehaviour
//{


    

//    void Start()
//    {
//        Context.Instance.RestApi.LoginResponded += OnLoginResponded;
//        Context.Instance.RestApi.RegisterResponded += OnRegisterResponded;
//        Context.Instance.RestApi.AddMatchResponded += OnAddMatchResponded;
//        Context.Instance.RestApi.GetMatchesResponded += OnGetMathcesResponded; 
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Space)) {

//            createLoginRequestPost();
//        }
//        if (Input.GetKeyDown(KeyCode.R)) {

//            createRegisterRequestPost();
//        }
//        if (Input.GetKeyDown(KeyCode.M)) {

//            createPostMatchRequest();
//        }
//        if (Input.GetKeyDown(KeyCode.G)) {

//            createGetMatchesRequest();
//        }
//    }



//    private void createLoginRequestPost() {

//        LoginRequest login = new LoginRequest();
//        login.nickname = "user4";
//        login.password = "pass4";

//        Context.Instance.RestApi.SendLoginRequest(login);
//    }

//    private void createRegisterRequestPost() {

//        RegisterRequest register = new RegisterRequest();
//        register.nickname = "user4";
//        register.password = "pass4";

//        Context.Instance.RestApi.SendRegisterRequest(register);
//    }

//    private void createPostMatchRequest() {

//        MatchRestApi match = CreateSampleAddMatchRequest();

//        Context.Instance.RestApi.AddMatchRequest(match);    
//    }

//    private void createGetMatchesRequest() {

//        Context.Instance.RestApi.GetMatchesRequest();
//    }

//    private void OnLoginResponded(RestResponse response) {

//        Debug.Log($"[{response.code}]From Login: " + response.text);
//    }
//    private void OnRegisterResponded(RestResponse response) {

//        Debug.Log($"[{response.code}]From Register: " + response.text);
//    }
//    private void OnAddMatchResponded(RestResponse response) {

//        Debug.Log($"[{response.code}]From AddMatch: " + response.text);
//    }
//    private void OnGetMathcesResponded(RestResponse response) {

//        if (response.code != 200) {
//            Debug.Log($"[{response.code}]From AddMatch: " + response.text);
//        }
//        else {

//            MatchRestApi[] b;
//            b = JsonHelper.getJsonArray<MatchRestApi>(response.text);


//            foreach (MatchRestApi match in b) {
//                Debug.Log(match.startTime);
//                Debug.Log(match.winnerNickname);
//            }
//        }
//    }

//    private void OnDestroy() {
//        if (Context.Instance?.RestApi == null)
//            return;

//        Context.Instance.RestApi.LoginResponded -= OnLoginResponded;
//        Context.Instance.RestApi.RegisterResponded -= OnRegisterResponded;
//        Context.Instance.RestApi.AddMatchResponded -= OnAddMatchResponded;
//        Context.Instance.RestApi.GetMatchesResponded -= OnGetMathcesResponded;
//    }




//    // TO BE DELETED
//    private MatchRestApi CreateSampleAddMatchRequest() {

//        MatchRestApi matchRequest = new MatchRestApi();

//        matchRequest.gameType = "Deathmatch";
//        matchRequest.startTime = DateTime.Now.ToString();
//        matchRequest.endTime = DateTime.Now.ToString();
//        matchRequest.completed = true;
//        matchRequest.winnerNickname = "user1";

//        matchRequest.users = new UserRestApi[2];
//        matchRequest.users[0] = (CreateUserSample());
//        matchRequest.users[1] = (CreateUserSample());

//        return matchRequest;
//    }

//    private UserRestApi CreateUserSample() {

//        UserRestApi user = new UserRestApi();

//        user.nickname = "user1";
//        user.team = "None";
//        user.completed = true;

//        user.score = CreateUserScoreSample();

//        return user;
//    }

//    private ScoreRestApi CreateUserScoreSample() {

//        ScoreRestApi score = new ScoreRestApi();

//        score.kills = 5;
//        score.deaths = 4;
//        score.assists = 0;
//        score.score = 100;

//        return score;
//    }

//}

