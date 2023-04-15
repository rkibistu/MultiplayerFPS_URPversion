using Fusion;
using JetBrains.Annotations;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RoomPlayer;

/// <summary>
/// 
/// Extinde clasa Gameplay (vezi gameplay)
/// Setari specifice gameplayului de tip Deathmatch
/// 
/// 
/// Seteaza ca atunci cand un jucator moare, sa fie automat respawnat cu un mic delay.
/// 
/// </summary>

public class DeathmatchGameplay : Gameplay {

    // PUBLIC MEMBERS

    public float _reviveDelay = 3f;

    // PRIVATE MEMBERS
    private bool _isReviveExecuting = false;

    private SpawnPoint[] _spawnPoints;

    // GameplayController INTERFACE


    public override void FocusScoreScreen() {
        base.FocusScoreScreen();

        UIScreen.Focus(InterfaceManager.Instance.scoreScreen);
    }
    protected override void OnSpawned() {
        base.OnSpawned();

        Debug.Log("Sunt initial: " + Players.Count + " players");

        //Find all spawn points
        _spawnPoints = GameObject.FindObjectsOfType<SpawnPoint>();
    }
    protected override void OnPlayerJoin(RoomPlayer player) {
        base.OnPlayerJoin(player);

        player.Team = RoomPlayer.ETeams.None;
        Debug.Log("Set team " + player.Team + " to player " + player.Username);
    }
    public override void OnPlayerAgentSpawned(AgentStateMachine agent) {
        base.OnPlayerAgentSpawned(agent);

        SetPositionToSpawnPoint(agent);
    }
    protected override void OnFatalHitTaken(HitData hitData) {
        base.OnFatalHitTaken(hitData);

        GameObject agent = hitData.Target.GameObject;

        StartCoroutine(RevivePlayerWithDelay(agent, _reviveDelay));
    }

    //invoked when round tiemr expired
    protected override void OnRoundEnd() {
        base.OnRoundEnd();

        Debug.Log("Round ended, timer expired");

        //focus to the screen with win/lose animation
        UIScreen.Focus(InterfaceManager.Instance.resultScreen);

        //In functie de numarul de killuri -> play win/lose aniamtion
        if (RoomPlayer.LocalRoomPlayer.HasMostKills()) {

            InterfaceManager.Instance.resultScreen.GetComponent<GameResultScreenUI>().PlayVictoryAnimation();
        }
        else {

            InterfaceManager.Instance.resultScreen.GetComponent<GameResultScreenUI>().PlayLostAniamtion();
        }
    }

    // called OnSpawned Gameplay -> initilaize the variable that represent the match that will be ade dto database
    protected override void InitializeMatchRestApi() {
        base.InitializeMatchRestApi();

        _matchRestApi = new MatchRestApi();
        _matchRestApi.gameType = ResourceManager.Instance.gameTypes[GameManager.Instance.GameTypeId].ModeName;
        _matchRestApi.startTime = DateTime.Now.ToString();
        _matchRestApi.users = new UserRestApi[RoomPlayer.Players.Count];

        for (int i = 0; i < _matchRestApi.users.Length; i++) {

            UserRestApi user = new UserRestApi();
            user.completed = false;
            user.nickname = RoomPlayer.Players[i].Username.Value;
            user.team = RoomPlayer.Players[i].Team.ToString();
            user.score = new ScoreRestApi();
            user.score.kills = 0;
            user.score.assists = 0;
            user.score.deaths = 0;
            user.score.score = 0;

            _matchRestApi.users[i] = user;
        }

        _matchRestApi.winnerNickname = null; //a team wins, not a player
        _matchRestApi.winnerTeam = ETeams.None.ToString();
        _matchRestApi.winnerScore = 0;
        _matchRestApi.completed = false;
    }

    protected override void EndOfRoundCompleteMatchRestApi() {
        base.EndOfRoundCompleteMatchRestApi();

        _matchRestApi.endTime = DateTime.Now.ToString();
        _matchRestApi.completed = true;
        for (int i = 0; i < _matchRestApi.users.Length; i++) {

            // VERIFICA AICI CE RETURNEAZA GetPlayer cand usernamul nu exista in listas!!!
            RoomPlayer player = RoomPlayer.GetPlayerByUsername(_matchRestApi.users[i].nickname);
            if (player != null) {
                _matchRestApi.users[i].completed = true; // Check if is still connected
                //_matchRestApi.users[i].team = Enum.GetName(typeof(ETeams), RoomPlayer.Players[i].Team);
                _matchRestApi.users[i].score.kills = player.PlayerScore.Kills;
                _matchRestApi.users[i].score.assists = player.PlayerScore.Assists;
                _matchRestApi.users[i].score.deaths = player.PlayerScore.Deaths;
                _matchRestApi.users[i].score.score = player.PlayerScore.Score;
            }
        }
        RoomPlayer winner = GetWinner();
        if (winner != null) {

            _matchRestApi.winnerNickname = winner.Username.ToString();
            _matchRestApi.winnerScore = winner.PlayerScore.Kills;
        }
    }


    // PRIVATE METHODS

    // seteaza viata jucatorului la valoarea maxima dupa un delay de timp in secunde
    private IEnumerator RevivePlayerWithDelay(GameObject playerAgent, float delay) {

        if (_isReviveExecuting)
            yield break;

        _isReviveExecuting = true;
        yield return new WaitForSeconds(delay);

        var health = playerAgent.GetComponent<Health>();
        health.ResetHealth();



        SetPositionToSpawnPoint(playerAgent.GetComponent<AgentStateMachine>());
        _isReviveExecuting = false;
    }

    //Get 1 spawnpoint from spawnpoint list
    private Transform RandomSpawnPoint() {

        int index = UnityEngine.Random.Range(0, _spawnPoints.Length);
        return _spawnPoints[index].transform;
    }

    // Alege random din unul de punctele de spawn si muta playerul acolo
    private void SetPositionToSpawnPoint(AgentStateMachine agent) {

        Transform spawnPoint = RandomSpawnPoint();
        agent.MoveTo(spawnPoint.position);
    }

    // Get the winner player
    private RoomPlayer GetWinner() {

        foreach (var player in Players) {

            if (player.Value.HasMostKills())
                return player.Value;
        }
        return null;
    }
}
