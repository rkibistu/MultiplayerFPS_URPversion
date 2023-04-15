using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 
/// Atached to TeamScoreScreenUI to show players' score
///     It gets the data from RoomPlayer.Players array and it refresh the data every time we activate this screen
/// 
/// </summary>

public class TeamsScoreScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _listHolderUp;
    [SerializeField] private GameObject _listHolderDown;

    [SerializeField] private GameObject _playerScoreItemPrefabBlue;
    [SerializeField] private GameObject _playerScoreItemPrefabRed;

    [SerializeField] private Transform _footerUp;
    [SerializeField] private Transform _footerDown;

    private GameObject _currentPlayerScoreItemPrefab;
    

    private void Start() {

        Debug.Log("Start ScoreScreenUI");
        PopulateScoreList();
    }

    private void OnEnable() {

        OrderScoresByKills(_listHolderUp,_footerUp); 
        OrderScoresByKills(_listHolderDown,_footerDown); 
    }
    private void PopulateScoreList() {

        //clear lsit
        ClearScoreList(_listHolderUp);
        ClearScoreList(_listHolderDown);

        //add all current players to list
        foreach (var player in RoomPlayer.Players) {

            InstantiateScoreItem(player);
        }

        OrderScoresByKills(_listHolderUp, _footerUp);
        OrderScoresByKills(_listHolderDown, _footerDown);
    }

    // Create ScoreItemUI and add it to the specific listHolder depending on the player team
    private void InstantiateScoreItem(RoomPlayer player) {

        //TogglePlayerScoreitemPrefab();
        PlayerScoreItemUI obj = null;
        if (player.Team == RoomPlayer.ETeams.Blue) {

            obj = Instantiate(_playerScoreItemPrefabBlue, _listHolderUp.transform).GetComponent<PlayerScoreItemUI>();
        }
        else if (player.Team == RoomPlayer.ETeams.Red) {

            obj = Instantiate(_playerScoreItemPrefabRed, _listHolderDown.transform).GetComponent<PlayerScoreItemUI>();
        }
        else {
            Debug.LogError("Player has team: " + player.Team + ".  This team is not accepted in TeamsScoreScreen. Need to be Red or Blue team.");
            return;
        }
        obj.Init(player);
    }

    // Delete all elements in list holder
    //  first is header, alst is footer. Ignore them
    //Should be called always before popualting the listHolder
    private void ClearScoreList(GameObject listHolder) {
        if (listHolder.transform.childCount > 2) {
            for (int i = 1; i < listHolder.transform.childCount - 1; i++) {

                Destroy(listHolder.transform.GetChild(i));
            }
        }
    }

    // Order PlayerScoreItemUI care sunt aflate in lista de scoruri din listHolder
    //  Functia ar trb mereu apelata cand afisam score screen-ul
    private void OrderScoresByKills(GameObject listHolder, Transform footer) {

       
        var playersScoreUI = listHolder.GetComponentsInChildren<PlayerScoreItemUI>();
        
        for(int i=0;i<playersScoreUI.Length - 1; i++) {
            for (int j = i + 1; j < playersScoreUI.Length; j++) {

                if (playersScoreUI[i].PlayerScore.Kills < playersScoreUI[j].PlayerScore.Kills) {

                    var temp = playersScoreUI[i];
                    playersScoreUI[i] = playersScoreUI[j];
                    playersScoreUI[j] = temp;
                }
            }
        }

        int index = 1;
        foreach (var playerScore in playersScoreUI) {

            playerScore.transform.SetSiblingIndex(index++);
        }

        footer.SetAsLastSibling();
    }

    
    private void TogglePlayerScoreitemPrefab() {

        if(_currentPlayerScoreItemPrefab == null) {
            _currentPlayerScoreItemPrefab = _playerScoreItemPrefabBlue;
            return;
        }

        _currentPlayerScoreItemPrefab = (_currentPlayerScoreItemPrefab == _playerScoreItemPrefabBlue) ? _playerScoreItemPrefabRed : _playerScoreItemPrefabBlue;
    }
    

}
