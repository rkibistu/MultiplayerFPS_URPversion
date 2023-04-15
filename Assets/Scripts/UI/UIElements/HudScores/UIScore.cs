using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIScore : UIBehaviour {
    // PRIVATE MEMBERS

    [SerializeField]
    private UIValue _leftScore;
    [SerializeField]
    private UIValue _rightScore;



    // PUBLIC METHODS

    public void UpdateScore(TeamInfo? teamInfo) {

        if (teamInfo.HasValue) {
            {
                _leftScore.SetValue(teamInfo.Value.BlueScore);
                _rightScore.SetValue(teamInfo.Value.RedScore);
            }
        }
    }

    public void ResetScore() {

        _leftScore.SetValue(0);
        _rightScore.SetValue(0);
    }
}
