using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RoomPlayer;
using static UnityEditor.Experimental.GraphView.GraphView;

public struct TeamInfo {

    public TeamInfo(int x = 0) {
        BlueScore = 0;
        RedScore = 0;
    }

    public int BlueScore;
    public int RedScore;
}

public class TeamDeathmatchGameplay : Gameplay {
    // PUBLIC MEMBERS

    public float ReviveDelay = 3f;
    public int GoalToWin = 3;

    public override TeamInfo? TeamInfo { get => _teamInfo; protected set {; } }

    // PRIVATE MEMBERS
    private bool _isReviveExecuting = false;

    private SpawnPoint[] _spawnPointsBlue;
    private SpawnPoint[] _spawnPointsRed;

    private int _playersBlueCount = 0;
    private int _playersRedCount = 0;

    private TeamInfo _teamInfo;
    private ETeams _winnerTeam;

    // GameplayController INTERFACE

    public override void FocusScoreScreen() {
        base.FocusScoreScreen();

        UIScreen.Focus(InterfaceManager.Instance.teamsScoreScreen);
    }
    protected override void OnSpawned() {
        base.OnSpawned();

        Debug.Log("Sunt initial: " + Players.Count + " players");

        //Find all spawn points
        FindSpawnPoints();

        _teamInfo.BlueScore = 0;
        _teamInfo.RedScore = 0;
    }

    protected override void OnPlayerJoin(RoomPlayer player) {
        base.OnPlayerJoin(player);

        //Choose team
        //SelectTeam(player);
        if (_playersBlueCount == _playersRedCount) {

            if (UnityEngine.Random.Range(1, 10) < 5) {

                player.Team = RoomPlayer.ETeams.Blue;
                _playersBlueCount++;
            }
            else {

                player.Team = RoomPlayer.ETeams.Red;
                _playersRedCount++;
            }
        }
        else {
            if (_playersRedCount < _playersBlueCount) {

                player.Team = RoomPlayer.ETeams.Red;
                _playersRedCount++;
            }
            else {

                player.Team = RoomPlayer.ETeams.Blue;
                _playersBlueCount++;
            }
        }
        Debug.Log("Set team " + player.Team + " to player " + player.Username);
    }


    public override void OnPlayerAgentSpawned(AgentStateMachine agent) {
        base.OnPlayerAgentSpawned(agent);

        if (!HasStateAuthority)
            return;

        SetPositionToSpawnPoint(agent);
    }

    protected override void OnFatalHitTaken(HitData hitData) {
        base.OnFatalHitTaken(hitData);

        if (HasStateAuthority) {
            GameObject agent = hitData.Target.GameObject;

            StartCoroutine(RevivePlayerWithDelay(agent, ReviveDelay));
        }

        //increment score of the instigator team
        IncrementScore(hitData.InstigatorRef);
        if (CheckForWin() == true) {

            OnRoundEnd();
            _timer.StopAndDeleteEvenets();
        }
    }

    //invoked when round tiemr expired
    protected override void OnRoundEnd() {
        base.OnRoundEnd();

        Debug.Log("Round ended, timer expired");

        //focus to the screen with win/lose animation
        UIScreen.Focus(InterfaceManager.Instance.resultScreen);

        //In functie de numarul de killuri -> play win/lose aniamtion
        _winnerTeam = (TeamInfo.Value.BlueScore > TeamInfo.Value.RedScore) ? ETeams.Blue : ETeams.Red;
        if (TeamInfo.Value.BlueScore == TeamInfo.Value.RedScore)
            _winnerTeam = ETeams.None;

        if (_winnerTeam == LocalRoomPlayer.Team) {

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
                _matchRestApi.users[i].completed = true;
                //_matchRestApi.users[i].team = Enum.GetName(typeof(ETeams), RoomPlayer.Players[i].Team);
                _matchRestApi.users[i].score.kills = player.PlayerScore.Kills;
                _matchRestApi.users[i].score.assists = player.PlayerScore.Assists;
                _matchRestApi.users[i].score.deaths = player.PlayerScore.Deaths;
                _matchRestApi.users[i].score.score = player.PlayerScore.Score;
            }
        }
        _matchRestApi.winnerNickname = null; //a team wins, not a player

        //winner team. None for a TIE
        if( TeamInfo.Value.BlueScore > TeamInfo.Value.RedScore) {
            _matchRestApi.winnerTeam = ETeams.Blue.ToString();
            _matchRestApi.winnerScore = TeamInfo.Value.BlueScore;
            _matchRestApi.loserScore = TeamInfo.Value.RedScore;
        }
        else if(TeamInfo.Value.BlueScore < TeamInfo.Value.RedScore){
            _matchRestApi.winnerTeam = ETeams.Red.ToString();
            _matchRestApi.winnerScore = TeamInfo.Value.RedScore;
            _matchRestApi.loserScore = TeamInfo.Value.BlueScore;
        }
        else {
            _matchRestApi.winnerTeam = ETeams.None.ToString();
            _matchRestApi.winnerScore = TeamInfo.Value.RedScore;
            _matchRestApi.winnerScore = TeamInfo.Value.RedScore;
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

    //Find all spawnpoint in this scene
    private void FindSpawnPoints() {
        _spawnPointsBlue = GameObject.FindObjectsOfType<SpawnPointBlue>();
        if (_spawnPointsBlue.Length == 0) {
            Debug.LogError("No blue spawn points in this scene. Can't spawn!");
        }
        _spawnPointsRed = GameObject.FindObjectsOfType<SpawnPointRed>();
        if (_spawnPointsRed.Length == 0) {
            Debug.LogError("No red spawn points in this scene. Can't spawn!");
        }
    }

    // Alege random din unul de punctele de spawn si muta playerul acolo
    private void SetPositionToSpawnPoint(AgentStateMachine agent) {

        Transform spawnPoint = RandomSpawnPoint(agent.Owner.Team);
        agent.MoveTo(spawnPoint.position);
    }

    //Get 1 spawnpoint from spawnpoint list depending on team
    private Transform RandomSpawnPoint(ETeams team) {

        int index;
        if (team == ETeams.Blue) {
            index = UnityEngine.Random.Range(0, _spawnPointsBlue.Length);
            return _spawnPointsBlue[index].transform;
        }
        else if (team == ETeams.Red) {
            index = UnityEngine.Random.Range(0, _spawnPointsRed.Length);
            return _spawnPointsRed[index].transform;
        }
        else {

            Debug.LogError("This gametype should ntot accept another team values for player!. Team for the player is bad set!");
            return transform;
        }
    }

    // Called when a player joins the gameplay to select the team with fewer people
    private void SelectTeam(RoomPlayer player) {
        if (_playersBlueCount == _playersRedCount) {

            if (UnityEngine.Random.Range(1, 10) < 5) {

                player.Team = RoomPlayer.ETeams.Blue;
                _playersBlueCount++;
            }
            else {

                player.Team = RoomPlayer.ETeams.Red;
                _playersRedCount++;
            }
        }
        else {
            if (_playersRedCount < _playersBlueCount) {

                player.Team = RoomPlayer.ETeams.Red;
                _playersRedCount++;
            }
            else {

                player.Team = RoomPlayer.ETeams.Blue;
                _playersBlueCount++;
            }
        }
    }

    //check the team of the isntigator -> increment the team score
    private void IncrementScore(PlayerRef instigatorRef) {
        RoomPlayer instigator = Players[instigatorRef];
        if (instigator != null) {

            if (instigator.Team == ETeams.Blue) {
                _teamInfo.BlueScore++;
            }
            else if (instigator.Team == ETeams.Red) {
                _teamInfo.RedScore++;
            }
            else {
                Debug.LogError("This is a team gameplay. Can't have another option to player team! We can't have " + instigator.Team);
            }
        }
    }

    private bool CheckForWin() {

        return ((TeamInfo.Value.BlueScore == GoalToWin) || (TeamInfo.Value.RedScore == GoalToWin));
    }

}
