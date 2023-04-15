using System.Globalization;
using UnityEngine;

/// <summary>
/// 
/// Informatii specifice clientilor individuali.
/// Stocare in memoria persistenta oferita de unity
/// 
/// </summary>

public static class ClientInfo {

    private static string testUsername = "nickname";

    //public static string Username {
    //    get => PlayerPrefs.GetString("C_Username", string.Empty);
    //    set => PlayerPrefs.SetString("C_Username", value);
    //}


    //public static string LobbyName {
    //    get => PlayerPrefs.GetString("C_LastLobbyName", "");
    //    set => PlayerPrefs.SetString("C_LastLobbyName", value);
    //}   
    
    //folosesc aici o variablia in lco de Unity Prefs  pentru ca dac anu,
    //  nu pot sa ma conectez cu 2 conturi de pe acelasi PC
    //  si sa aibe nickname diferit
    public static string Username {
        get => testUsername;
        set => testUsername = value;
    }


    public static string LobbyName {
        get => PlayerPrefs.GetString("C_LastLobbyName", "");
        set => PlayerPrefs.SetString("C_LastLobbyName", value);
    }
}