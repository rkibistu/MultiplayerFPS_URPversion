using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _listHolder;
    [SerializeField] private GameObject _playerScoreItemPrefabBlue;
    [SerializeField] private GameObject _playerScoreItemPrefabRed;

    [SerializeField] private Transform _footer;

    private GameObject _currentPlayerScoreItemPrefab;
    

    private void Start() {

        Debug.Log("Start ScoreScreenUI");
        PopulateScoreList();
    }

    private void OnEnable() {

        OrderScoresByKills(); 
    }
    private void PopulateScoreList() {

        //clear lsit
        ClearScoreList();

        //add all current players to list
        foreach (var player in RoomPlayer.Players) {

            TogglePlayerScoreitemPrefab();

            var obj = Instantiate(_currentPlayerScoreItemPrefab, _listHolder.transform).GetComponent<PlayerScoreItemUI>();
            obj.Init(player);
        }

        OrderScoresByKills();
    }

    private void ClearScoreList() {
        if (_listHolder.transform.childCount > 2) {
            for (int i = 1; i < _listHolder.transform.childCount - 1; i++) {

                Destroy(_listHolder.transform.GetChild(i));
            }
        }
    }

    // Order PlayerScoreItemUI care sunt aflate in lista de scoruri din listHolder
    //  Functia ar trb mereu apelata cand afisam score screen-ul
    private void OrderScoresByKills() {

       
        var playersScoreUI = _listHolder.GetComponentsInChildren<PlayerScoreItemUI>();
        
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

        _footer.SetAsLastSibling();
    }

    private void TogglePlayerScoreitemPrefab() {

        if(_currentPlayerScoreItemPrefab == null) {
            _currentPlayerScoreItemPrefab = _playerScoreItemPrefabBlue;
            return;
        }

        _currentPlayerScoreItemPrefab = (_currentPlayerScoreItemPrefab == _playerScoreItemPrefabBlue) ? _playerScoreItemPrefabRed : _playerScoreItemPrefabBlue;
    }

}
