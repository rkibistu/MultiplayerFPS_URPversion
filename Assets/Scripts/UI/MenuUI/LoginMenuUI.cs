using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 
/// Pe menu-ul InfoSetupScreen -> pentru a actualiza profilul clientului
/// 
/// </summary>

public class LoginMenuUI : MonoBehaviour {

    public TMP_InputField _nicknameInput;
    public TMP_InputField _passwordInput;
    public TextMeshProUGUI _errorMessage;
    public Button _confirmButton;


    private void Start() {

        Context.Instance.RestApi.LoginResponded += OnLoginResponded;

        // modifica in unity Prefs atunci cand clientul isi schimba numele
        _nicknameInput.onValueChanged.AddListener(x => ClientInfo.Username = x);
        _nicknameInput.onValueChanged.AddListener(x => {
            // disallows empty usernames to be input
            _confirmButton.interactable = !string.IsNullOrEmpty(x);
        });

        //preia numele din Unity Prefs si seteaza-l (Ultimul setat de cleint practic)
        _nicknameInput.text = ClientInfo.Username;

        //TO DELETE LATER, for faster debug
        _nicknameInput.text = "user1";
        _passwordInput.text = "pass1";
    }

    private void OnDestroy() {

        if (Context.Instance != null) {
            Context.Instance.RestApi.LoginResponded -= OnLoginResponded;
        }
    }


    //Called by confirm button from LoginMenu
    public void CheckCredentials() {


        LoginRequest loginData = new LoginRequest();
        loginData.nickname = _nicknameInput.text;
        loginData.password = _passwordInput.text;
        Debug.Log("check credentials!" + loginData.nickname + "  " + loginData.password);

        Context.Instance.RestApi.SendLoginRequest(loginData);
    }

    private void OnLoginResponded(RestResponse response) {

        if(response.code != 200) {

            _errorMessage.SetActive(true);
            return;
        }

        UIScreen.Focus(InterfaceManager.Instance.mainMenu);
    }

    public void AssertProfileSetup() {
        // daca nu e setat numele in Unity Prefs -> activeaza ecranul de setat numele

        //eventual aici verifica jwt-ul din Unity Prefs si daca nu e expirat, foloseste-l pe acelasi
        //  poate nu e o idee buna ca as prefera sa am mereu unul nou cand icnep sesiunea de joc

        UIScreen.Focus(GetComponent<UIScreen>());
    }
}