using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatchRestApi
{
    public string gameType;
    public string startTime;
    public string endTime;
    public bool completed;
    public int? winnerScore;
    public int? loserScore;
    public string winnerTeam;
    public string winnerNickname;
    public UserRestApi[] users;
}

public  static partial class  PrettyPrint {

    
}
