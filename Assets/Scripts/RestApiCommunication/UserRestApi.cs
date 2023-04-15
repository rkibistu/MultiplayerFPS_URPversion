using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[System.Serializable]
public class UserRestApi
{
    public string nickname;
    public string team;
    public bool completed;
    public ScoreRestApi score;
}
