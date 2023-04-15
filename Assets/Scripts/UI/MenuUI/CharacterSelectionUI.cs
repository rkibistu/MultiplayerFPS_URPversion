using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    public TMP_Dropdown _characterDropdown;
    public Image _characterImage;
    void Start()
    {
        var characters = ResourceManager.Instance.characterModels;
        var characterOptions = characters.Select(x => x.characterName).ToList();

        _characterDropdown.ClearOptions();
        _characterDropdown.AddOptions(characterOptions);
        _characterDropdown.value = RoomPlayer.LocalRoomPlayer.CharacterModelId;

        _characterDropdown.onValueChanged.AddListener(x => {
            var rm = RoomPlayer.LocalRoomPlayer;
            if (rm != null) rm.CharacterModelId = x;
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) {

            Debug.Log(RoomPlayer.LocalRoomPlayer.CharacterModelId);
        }   
    }
}
