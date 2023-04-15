using Fusion;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// 
/// Entitate unica la nivel de aplicatie.
/// Este create de GameLauncher in momentul in care se conecteaza un player in modul HOST. (OnPlayerJoined method)
///             Stim sigur ca va exista un singur player cu acest mod.
/// 
/// 
/// Retine preferinte lobby-ului: harta, game mode, lobby name
/// Ofera posibilitati de a spawna caracterele jucatorilor si obiectul de tip Gameplay (tinand cont de preferintele lobbyului)
/// </summary>
public class GameManager : NetworkBehaviour {

    // PUBLIC MEMBERS
    public int ChooseCharacterTest = 0;
    public static GameManager Instance { get; private set; }

    public static event Action<GameManager> OnLobbyDetailsUpdated;
    public GameType GameType => ResourceManager.Instance.gameTypes[GameTypeId];
    public string MapName => ResourceManager.Instance.mapDefinitions[MapId].mapName;
    public string ModeName => ResourceManager.Instance.gameTypes[GameTypeId].ModeName;
    [Networked(OnChanged = nameof(OnLobbyDetailsChangedCallback))] public NetworkString<_32> LobbyName { get; set; }
    [Networked(OnChanged = nameof(OnLobbyDetailsChangedCallback))] public int MapId { get; set; }
    [Networked(OnChanged = nameof(OnLobbyDetailsChangedCallback))] public int GameTypeId { get; set; }
    [Networked(OnChanged = nameof(OnLobbyDetailsChangedCallback))] public int MaxUsers { get; set; }

    private static void OnLobbyDetailsChangedCallback(Changed<GameManager> changed) {
        OnLobbyDetailsUpdated?.Invoke(changed.Behaviour);
    }


    // PRIVATE MEMBERS
    private bool _initialized = false;
    public bool IsReady => (Runner.SceneManager.IsReady(Runner) && _initialized);


    public Dictionary<PlayerRef, bool> _playersConfirm;


    // MONOBEHAVIOUR INTERFACE
    private void Awake() {
        if (Instance) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // NETWORKBEHAVIOUR INTERFACE
    public override void Spawned() {
        base.Spawned();

        if (Object.HasStateAuthority) {
            LobbyName = ServerInfo.LobbyName;
            MapId = ServerInfo.MapId;
            GameTypeId = ServerInfo.GameMode;
            MaxUsers = ServerInfo.MaxUsers;
        }

        _initialized = true;

        _playersConfirm = new Dictionary<PlayerRef, bool>();
    }


    // PUBLIC METHODS

    //Spawn gameplay. Called by GamesceneLoader when scene is ready to be played

    public void SpawnGameplayObjects() {

        if (Context.Instance.Runner.IsServer) {
            //Host-ul spawneaza gameplay obejct + playerAgents

            //reset info from 1 round to another
            _playersConfirm.Clear();

            SpawnGameplay();
            //AddPlayersToActiveGameplay();
            //SpawnPlayersAgents();
        }
        //toti playeri blocheaza cursorul -> focus pe joc
        RoomPlayer.LocalRoomPlayer.Input.LockCursour();
    }


    public void DespawnGameplayObjects() {

        Debug.Log("Despawn Gameplay Objects");

        RemovePlayersFromActiveGameplay();
        Context.Instance.Gameplay.Timer._onTiemrExpired = null;
        DespawnPlayersAgents();
        DespawnGameplay();
    }


    // RoomPlayer.ActiveAgent se seteaza corect la inceputul unei runde
    // Dar ActiveAgent.Owner nu face asta. Avem nevoie sa apelam aceasta functie pentru a realiza asocierea.
    //      Functie apelata inainte sa se execute operatii folosindu-ne de Owner-ul unui agent, in cazul in care acesta e null
    public void SetOwnersForAllAgents() {

        foreach (var player in RoomPlayer.Players) {

            if (player.ActiveAgent == null) {

                //Debug.LogError("Active agent is null. Ai presupus ca asta nu se intampla");
                return;
            }
            Debug.Log("Tried to acces null owner. Set owner for the agent. Owner is player " + player.Username + ".   " + player.Object.InputAuthority);
            player.ActiveAgent.Owner = player;
        }
    }


    // Confirm from clients that things happened
    public void ConfirmGameplaySpawned() {

        ConfirmGameplaySpawned_Rpc(RoomPlayer.LocalRoomPlayer.Object.InputAuthority);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    private void ConfirmGameplaySpawned_Rpc(PlayerRef playerRef) {

        Debug.Log("Player  " + playerRef + " confirmed gameplay spawned.");
        _playersConfirm.Add(playerRef, true);


        if (_playersConfirm.Count == RoomPlayer.Players.Count) {
            Debug.Log("All players confirmed gameplay spawned.");

            //all players spawned gameplay -> let's add RoomPlayer to gameplay 
            AddPlayersToActiveGameplay();
            ResetPlayersConfirmations();

            SpawnPlayersAgents();
        }
    }

    public void ConfirmAgentSpawned(AgentStateMachine agent) {

        ConfirmAgentSpawned_Rpc(RoomPlayer.LocalRoomPlayer.Object.InputAuthority, agent);
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    private void ConfirmAgentSpawned_Rpc(PlayerRef playerRef, AgentStateMachine agent) {

        Debug.Log("Player  " + playerRef + " confirmed agent spawned.");

        var player = Context.Instance.Gameplay.Players[playerRef];
        player.AssignAgent(agent);

        //Seteaza NetworkObject asociat
        Runner.SetPlayerObject(player.Object.InputAuthority, player.Object);

        _playersConfirm[playerRef] = true;
        if (AllPlayersConfirmed()) {

            Debug.Log("All players confirmed agent spawned.");
            ResetPlayersConfirmations();

            foreach (var kvp in Context.Instance.Gameplay.Players) {

                OnPlayerAgentSpawned_Rpc(kvp.Value.ActiveAgent);
            }
            Context.Instance.Gameplay.StartRound();
        }
    }

    // PRIVATE METHODS
    private void SpawnGameplay() {

        if (Runner.IsServer && HasStateAuthority) {

            Debug.Log("Spawn gameplay");
            Gameplay gameplayPrefab = ResourceManager.Instance.gameTypes[this.GameTypeId].GameplayPrefab;
            Context.Instance.Runner.Spawn(gameplayPrefab);
        }
    }
    //Despawn Gameplay.
    private void DespawnGameplay(bool changeToLobby = true) {

        Debug.Log("Despawn gameplay");
        Context.Instance.Runner.Despawn(Context.Instance.Gameplay.GetComponent<NetworkObject>());

        //LevelManager.LoadMenu();
    }

    // Spawn agents for every player. Called by GamesceneLoader when scene is ready to be played
    private void SpawnPlayersAgents() {

        Debug.Log("Spawn players agents. Count: " + RoomPlayer.Players.Count);
        foreach (var player in RoomPlayer.Players) {

            //player.SpawnAgent();
            Debug.LogWarning("aici1");
            SpawnPlayerAgent(player);
        }
    }
    // Despawn agents for every player. 
    private void DespawnPlayersAgents() {

        Debug.Log("Despawn players agents. Count: " + RoomPlayer.Players.Count);
        foreach (var player in RoomPlayer.Players) {

            DespawnPlayerAgent(player);
        }
    }

    // Add players to active gameplay. Called by GamesceneLoader when scene is ready to be played
    private void AddPlayersToActiveGameplay() {

        Debug.Log("Add players to gameplay. Count: " + RoomPlayer.Players.Count);
        foreach (var player in RoomPlayer.Players) {

            if (Context.Instance.Gameplay != null) {

                Context.Instance.Gameplay.JoinGameplay(player);
            }
            else {
                Debug.LogWarning("Players couldn't join gameplay. Gameplay not spawned yet");
            }
        }
    }
    // Remove players from active gameplay.
    private void RemovePlayersFromActiveGameplay() {

        Debug.Log("Remove players from gameplay. Count: " + RoomPlayer.Players.Count);
        foreach (var player in RoomPlayer.Players) {

            if (Context.Instance.Gameplay != null) {

                Context.Instance.Gameplay.LeaveGameplay(player);
            }
            else {
                Debug.LogWarning("Couldn't remove player from Gameplay. Gameplay not spawned");
            }
        }
    }

    // Soawnewaza/despawnewaza agentul pt player + setari
    private void SpawnPlayerAgent(RoomPlayer player) {



        //despawneaza daca deja exista cumva un agent


        //spawneaza agent efectiv
        var agent = SpawnAgent(player.Object.InputAuthority, player.CharacterModelId) as AgentStateMachine;

        // coreleaza agentul spawnat cu RoomPlayer caruia ii apartine

        //player.AssignAgent(agent);

        ////Seteaza NetworkObject asociat
        //Runner.SetPlayerObject(player.Object.InputAuthority, player.Object);

        //dupa ce a fost spawnat si corelat cu RoomPlayerul -> realizeaza setarile necesare
        //  nu putem face asta in functia Spawned de la AgentStateMachine pt ca player.AssignAgent(agent) nu e obligatoriu apelata inainte de fct OnSpawned -> agent.Owner nu e setat
        //Context.Instance.Gameplay.OnPlayerAgentSpawned(agent);
        //OnPlayerAgentSpawned_Rpc(agent);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void OnPlayerAgentSpawned_Rpc(AgentStateMachine agent) {

        Context.Instance.Gameplay.OnPlayerAgentSpawned(agent);
    }



    private void DespawnPlayerAgent(RoomPlayer player) {
        if (player.ActiveAgent == null)
            return;

        //despawneaza agentul efectiv
        DespawnAgent(player.ActiveAgent);

        //decoreleaza agentul de RoomPlayerul asociat
        player.ClearAgent();
    }

    // spawneaza/despawneaza efectiv prefab-ul pt agent (nu face nici o alta setare)
    private AgentStateMachine SpawnAgent(PlayerRef inputAuthority, int characterModelId) {

        //Transform spawnPoint = RandomSpawnPoint();
        //var agent = Runner.Spawn(agentPrefab, spawnPoint.position, spawnPoint.rotation, inputAuthority);
        //var agent = Context.Instance.Runner.Spawn(agentPrefab, agentPrefab.transform.position, agentPrefab.transform.rotation, inputAuthority);
        AgentStateMachine agentPrefab = ResourceManager.Instance.characterModels[characterModelId].characterPrefab.GetComponent<AgentStateMachine>();
        var agent = Context.Instance.Runner.Spawn(agentPrefab, agentPrefab.transform.position, agentPrefab.transform.rotation, inputAuthority);
        return agent;
    }
    private void DespawnAgent(AgentStateMachine agent) {
        if (agent == null)
            return;

        Context.Instance.Runner.Despawn(agent.Object);
    }



    // PlayersConfirm dictionare operations
    private void ResetPlayersConfirmations() {

        foreach (PlayerRef player in _playersConfirm.Keys.ToList()) {
            _playersConfirm[player] = false;
        }
    }
    private bool AllPlayersConfirmed() {

        foreach (KeyValuePair<PlayerRef, bool> kvp in _playersConfirm) {
            if (kvp.Value == false) {
                return false;
            }
        }
        return true;
    }
}
