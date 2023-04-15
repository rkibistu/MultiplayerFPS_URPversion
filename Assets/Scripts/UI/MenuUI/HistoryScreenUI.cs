using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Atasat de ecranul care arata istoricul meciurilor
/// </summary>

public class HistoryScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _listHolder;
    [SerializeField] private GameObject _historyElementPrefab;

    private MatchRequest _matchRequest;
    

    void Awake()
    {
        _matchRequest = new MatchRequest();
        _matchRequest.nickname = ClientInfo.Username;
        Context.Instance.RestApi.GetMatchesResponded += OnGetMatcesResponded;
    }

    private void OnEnable() {

        ClearLsitHolder();
        Context.Instance.RestApi.GetMatchesRequest(_matchRequest);
    }

    private void OnDestroy() {
        
        if(Context.Instance != null) {

            Context.Instance.RestApi.GetMatchesResponded -= OnGetMatcesResponded;
        }
    }

    private void OnGetMatcesResponded(RestResponse response) {

        if(response.code != 200) {

            Debug.LogError("bad request to get matches!");
            return;
        }

        MatchRestApi[] b;
        b = JsonHelper.getJsonArray<MatchRestApi>(response.text);

        foreach (MatchRestApi match in b) {

            var obj = Instantiate(_historyElementPrefab, _listHolder.transform).GetComponent<HistoryElementUI>();
            obj.Init(match);
        }
    }

    private void ClearLsitHolder() {
        if (_listHolder.transform.childCount > 1) {
            for (int i = 1; i < _listHolder.transform.childCount; i++) {

                Destroy(_listHolder.transform.GetChild(i).gameObject);
            }
        }
    }
}
