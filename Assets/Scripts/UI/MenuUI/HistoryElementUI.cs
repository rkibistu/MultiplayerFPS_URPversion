using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 
/// ElementUI ce reprezinta un meci
/// Trebuie modificat. Avem toate infromatiile in MatchRestAPi. Modificam cand schimbam front end
/// 
/// </summary>

public class HistoryElementUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _dateText;
    [SerializeField] private TextMeshProUGUI _killsText;
    [SerializeField] private TextMeshProUGUI _deathsText;
    [SerializeField] private TextMeshProUGUI _assistsText;

    public string Nickname { get => _nicknameText.text; private set {; } }

    private MatchRestApi _match;
    private UserRestApi _localUser;
    
    public void Init(MatchRestApi match)
    {
        _match = match;
        SetLocalUser();

        SetAllUI();
    }

    private void SetLocalUser() {

        foreach(var user in _match.users) {

            if(user.nickname == ClientInfo.Username) {

                _localUser = user;
                break;
            }
        }
    }

    private void SetKills() {
        _killsText.text = "K: " + _localUser.score.kills;
    }
    private void SetDeaths() {
        _deathsText.text = "D: " + _localUser.score.deaths;
    }
    private void SetAssists() {
        _assistsText.text = "A: " + _localUser.score.assists;
    }
    private void SetNickname() {

        _nicknameText.text = ClientInfo.Username;
    }

    private void SetDate() {

        _dateText.text = _match.startTime;
    }
    private void SetAllUI() {

        SetKills();
        SetDeaths();
        SetAssists();
        SetNickname();
        SetDate();
    }

   
}
