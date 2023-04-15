using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class AgentVisual : MonoBehaviour {
    // PUBLIC MEMBERS
    [Tooltip("Avatar that represents the caracter. A face photo or somethng")]
    public Sprite Avatar;

    // PRIVATE MEMBERS
    [SerializeField]
    [Tooltip("Character model mesh which need to be changed")]
    private SkinnedMeshRenderer _characterMeshToChange;
    [SerializeField]
    [Tooltip("Mesh used for proxies")]
    private Mesh _characterMesh;
    [SerializeField]
    [Tooltip("Mesh used for local player")]
    private Mesh _headlessCharacterMesh;
    [SerializeField]
    [Tooltip("Head object for characters that have separate head. We want to disable it for local player")]
    private GameObject _head;


    // PUBLIC METHODS

    public void SetVisibility(bool isVisible) {

        if (_characterMeshToChange != null) {

            _characterMeshToChange.sharedMesh = (isVisible == true) ? _characterMesh : _headlessCharacterMesh;
        }
        if(_head != null) {

            _head.SetActive(isVisible);
        }

        
    }

    public void SetLayers(bool isLocalPlayer) {

        if (isLocalPlayer == true) {
            SetGameLayerRecursive(gameObject, 3); //localagent layer
        }
        else {
            SetGameLayerRecursive(gameObject, 10); //agent layer
        }
    }

    private void SetGameLayerRecursive(GameObject _go, int _layer) {
        _go.layer = _layer;
        foreach (Transform child in _go.transform) {
            child.gameObject.layer = _layer;

            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                SetGameLayerRecursive(child.gameObject, _layer);

        }
    }



}
