using Fusion;
using Managers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RoomPlayer;

/// <summary>
/// 
/// Gameplay-ul efectiv. Este o clasa abstracta ce urmeaza sa fie implementata de fiecare tip diferit de gameplay (deathmath, teammatch, etc)
/// Ofera posibilitatea de seta ce sa se intamplee cand:
///     se conectwaza/deconecteaza un player; moare un player; se spawneaza/despawneaza caracterul unui player
/// 
/// Este spawnat de GamesceneLaoder la incarcarea hartii/scenei de lupta
/// Tine un dictionar cu toti playerii conectati -> <PlayerRef, RoomPlayer>
/// 
/// </summary>

[RequireComponent(typeof(GameTimer), typeof(ActivateUI))]
public abstract class Gameplay : NetworkBehaviour {

    // PUBLIC MEMBERS

    [Networked, HideInInspector, Capacity(200)]
    public NetworkDictionary<PlayerRef, RoomPlayer> Players { get; }

    [Tooltip("Durata rundei in secunde")]
    public int _roundDuration;
    public GameTimer Timer => _timer;
    public virtual TeamInfo? TeamInfo { get => null; protected set {; } }
    public bool IsPlaying { get; private set; }

    // PRIVATE MEMBERS
    [SerializeField]
    [Tooltip("Time to show Scoreboard at the end of the round before going back to lobby screen")]
    private float _endScoreTableDuration = 3f;

    // PROTECTED MEMBERS

    protected GameTimer _timer;
    protected ActivateUI _activateUI;

    //match to be added to the database at the end of the round
    protected MatchRestApi _matchRestApi;

    // PUBLIC METHODS

    // functie apelata cand un nou player se conecteza la gameplay (doar pe server)
    // adauga la dictionarul cu playeri + spawneaza agent(caracter) pentru player
    public void JoinGameplay(RoomPlayer player) {
        if (HasStateAuthority == false)
            return;

        Debug.Log($"Player {player.Username} joined gameplay");

        //se executa doar pe server. Dar toti vor vedea lista de playeri pt ca e marcata ca Networked
        AddToPlayerList(player);

        // can be override by childs if diffrent gameplays need diffrent actions
        OnPlayerJoin(player);
    }

    //functie apelata cand un player se deconecteaza de la gameplay (doar pe server)
    //  adica se revine la menu de lobby sau se deconecteaza din joc
    public void LeaveGameplay(RoomPlayer player) {
        if (HasStateAuthority == false)
            return;

        Debug.Log($"Player {player.Username} left gameplay");
        RemoveFromPlayerList(player);
        UnsubscribeFromHealthEvents(player);

        // can be override by childs if diffrent gameplays need diffrent actions
        OnPlayerLeft(player);
    }

    // This method should be called after end  of round animations to despawn gameplay objects and move to lobby screen
    //   This method despawn gameplay objects, you have to despawn everything that depends to them for all players
    //      Example: UI is going to have errors if you don't disable it on all players before calling this.
    //      We use end of round animation as a way to let time to all players to despawn UI and other objects that are dependent 
    public void EndRound() {

        //show scoreboard
        //UIScreen.Focus(InterfaceManager.Instance.scoreScreen);
        FocusScoreScreen();

        // despawn gameplay objects and go to lobby after some delay
        StartCoroutine(GoBackToLobby_Coroutine());
    }

    // Focus ScoreScreenUI. Called by PlayerLocaqlInput when pressing correct key
    public virtual void FocusScoreScreen() {; }

    // MONOBEHAVIOUR INTERFACE

    // Se apeleaza INAINTE de functiile de Render din Fusion.
    //             DUPA toate functiile de physiscs din Fusion (inclusiv OnLateFixedUpdate din Agent)
    private void Update() {
        if (HasStateAuthority == false)
            return;

        OnUpdate();
    }

    // Se apeleaza dupa Update
    private void LateUpdate() {
        if (HasStateAuthority == false)
            return;

        OnLateUpdate();
    }


    // NETWORK INTERFACE

    public override void Spawned() {
        // Register to context

        Debug.Log("Spawned gamepley. Register Gameplay to context");
        Context.Instance.Gameplay = this;

        //move from DontDestroyOnLoad
        SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetActiveScene());



        //Get timer and clear OnExpiredTime Action subscriptions
        _timer = GetComponent<GameTimer>();

        //_timer.StartTimer(Runner, _roundDuration);

        //activeaza UI specific gameplay
        _activateUI = GetComponent<ActivateUI>();

        // Reset Player Score
        foreach (var player in RoomPlayer.Players) {

            player.PlayerScore.ResetScore();
        }

        //Clear Cache from last round of the match
        // There are destroyed gameobjects that exist in cache when we spawn scenes. We don't want them
        Context.Instance.ObjectCache.ClearAll();

        //accept input
        IsPlaying = true;



        OnSpawned();

        GameManager.Instance.ConfirmGameplaySpawned();
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        Debug.Log("Gameplay despawned");
        Context.Instance.Gameplay = null;


        Context.Instance.RestApi.AddMatchResponded -= OnAddMatchResponded;
        //_timer._onTiemrExpired -= OnRoundEnd;
    }
    // Gameplay INTERFACE

    protected virtual void OnSpawned() {; }

    // spawn agent
    protected virtual void OnPlayerJoin(RoomPlayer player) {; }
    //despawn agent
    protected virtual void OnPlayerLeft(RoomPlayer player) {; }

    // setari pe agent dupa ce a fost spawnat si corelat cu RoomPlayer (pozitia de start, vizibil/invisibil, arma, etc. )
    // acestea difera in functie de tipul de gameplay  (deathmatch, teams, etc.)
    // Called by GameManager after spawning and setting roomplayer 
    public virtual void OnPlayerAgentSpawned(AgentStateMachine agent) {

        if (agent.Owner == null)
            GameManager.Instance.SetOwnersForAllAgents();

        Debug.Log("OnPlayerAgentSpawned settings for agent of the player " + agent.Owner.Username + ".   " + agent.Owner.Object.InputAuthority);
        SubscribeTohealthEvents(agent);
    }
    public virtual void OnPlayerAgentDespawned(AgentStateMachine agent) {; }

    //When this is called everything is setup: gameplay spawned, agents aspawned and confirmed
    public void StartRound() {

        StartRound_Rpc();

        //RestApiInitilaize
        // initialize class that is going to be added to database to record hsitory
        InitializeMatchRestApi();
        // subscribe to event that get the RestApi response after trying to add a match
        Context.Instance.RestApi.AddMatchResponded += OnAddMatchResponded;
    }
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void StartRound_Rpc() {

        Debug.Log("Start round timer");
        _timer._onTiemrExpired = null;
        _timer._onTiemrExpired += OnRoundEnd;
        _timer.StartTimer(Runner, _roundDuration);

        Debug.Log("Activate gameplay UI");
        _activateUI.Activate();
    }

    // apelata in Update()
    protected virtual void OnUpdate() {

        if (Input.GetKeyDown(KeyCode.K)) {

            Debug.Log(RoomPlayer.Players.Count + "   " + Players.Count);
        }
    }

    // apelata in LateUpdate
    protected virtual void OnLateUpdate() {; }

    // functie apelata de fiecare data cand un jucator moare (FatalHitTaken event din Health)
    protected virtual void OnFatalHitTaken(HitData hitData) {
    }

    // invoked when round timer ended
    protected virtual void OnRoundEnd() {

        //ADD MATCH TO DATABASE
        if (HasStateAuthority) {

            EndOfRoundCompleteMatchRestApi();
            Context.Instance.RestApi.AddMatchRequest(_matchRestApi);
        }

        // Seteaza starea jucatorilor to lobby deoarece acum ne vom muta in scena d elobby dintre runde
        foreach (var player in RoomPlayer.Players) {

            player.GameState = EGameState.EndingRound;

        }

        //set gameplay state
        IsPlaying = false;

        //dezactivate UI
        _activateUI.Dezactivate();
    }


    //Called at the start of the round, to initialize MatchRestApi class that is going to be sended to be added to database
    protected virtual void InitializeMatchRestApi() {
        Debug.Log("Initliaze Database class for current round");
    }

    //Called at the end of the round to update MatchRestApi class
    protected virtual void EndOfRoundCompleteMatchRestApi() {
        Debug.Log("Complete Database class for current round");
    }

    //Called to update the info of a single user from MatchRestApi class that is going to eba dded to database
    //      Important to use when a player disconnect to remeber their last status(score)
    public virtual void UpdateMatchRestApi(RoomPlayer playerToUpdate) {

        Debug.Log("Update Database class for current round for player " + playerToUpdate.Username + "   " + playerToUpdate.Object.InputAuthority);

        for (int i = 0; i < _matchRestApi.users.Length; i++) {

            if (_matchRestApi.users[i].nickname == playerToUpdate.Username) {

                _matchRestApi.users[i].score.kills = playerToUpdate.PlayerScore.Kills;
                _matchRestApi.users[i].score.assists = playerToUpdate.PlayerScore.Assists;
                _matchRestApi.users[i].score.deaths = playerToUpdate.PlayerScore.Deaths;
                _matchRestApi.users[i].score.score = playerToUpdate.PlayerScore.Score;

                break;
            }
        }
    }

    // PRIVATE METHODS

    // adauga/sterge din lista de playeri conectat
    private void AddToPlayerList(RoomPlayer player) {

        var playerRef = player.Object.InputAuthority;

        if (Players.ContainsKey(playerRef) == true) {
            Debug.LogError($"Player {playerRef} already joined");
            return;
        }

        Players.Add(playerRef, player);
    }
    private void RemoveFromPlayerList(RoomPlayer player) {

        if (Players.ContainsKey(player.Object.InputAuthority) == false)
            return;

        Players.Remove(player.Object.InputAuthority);
    }



    // aceasta functie va abona jucatorul la evenimentul FatalHitTaken din clasa Health
    // astfel incat sa executam cod atunci cand un player moare
    private void SubscribeTohealthEvents(AgentStateMachine agent) {

        //AgentStateMachine agent = player.ActiveAgent;
        if (agent == null) {

            Debug.LogError("Tried to subscribe to health events, but RoomPlayer has no activeAgent assosiated.");
            return;
        }

        agent.Health.FatalHitTaken += OnFatalHitTaken;
    }
    // dezabonam 
    private void UnsubscribeFromHealthEvents(RoomPlayer player) {

        AgentStateMachine agent = player.ActiveAgent;
        if (agent == null) {

            Debug.LogError("Tried to unsubscribe from health events, but RoomPlayer has no activeAgent assosiated.");
            return;
        }

        agent.Health.FatalHitTaken -= OnFatalHitTaken;
    }


    // next 2 methods are used to end the round
    private void DespawnGameplayAndGoToLobby() {

        if (!HasStateAuthority)
            return;

        GameManager.Instance.DespawnGameplayObjects();

        // Seteaza starea jucatorilor to lobby deoarece acum ne vom muta in scena d elobby dintre runde
        foreach (var player in RoomPlayer.Players) {

            player.GameState = EGameState.Lobby;
        }

        LevelManager.LoadTrack(ResourceManager.Instance.AfterRoundMenuScene);
    }

    private IEnumerator GoBackToLobby_Coroutine() {

        yield return new WaitForSeconds(_endScoreTableDuration);
        DespawnGameplayAndGoToLobby();
    }

    private void OnAddMatchResponded(RestResponse response) {

        Debug.Log($"[{response.code}]Add match: {response.text}");
    }
}
