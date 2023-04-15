using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Fusion.NetworkEvents;
using UnityEngine.EventSystems;
using UnityEngineInternal;
using Fusion;
using System.Linq;
using Fusion.StatsInternal;
using UnityEngine.UI;

public class UIScreenEffects : UIWidget {
    // PRIVATE METHODS

    [SerializeField]
    private CanvasGroup _hitGroup;
    [SerializeField]
    private CanvasGroup _bloodScreen1;
    [SerializeField]
    [Tooltip("a value in range 0-1, the screen is seen when health is lower")]
    [Range(0f, 1f)]
    private float _threshhold1 = 0.5f;
    [SerializeField]
    private CanvasGroup _bloodScreen2;
    [SerializeField]
    [Tooltip("a value in range 0-1, the screen is seen when health is lower")]
    [Range(0f, 1f)]
    private float _threshhold2 = 0.25f;
    [SerializeField]
    private UIBehaviour _deathGroup;
    [SerializeField]
    private GameObject _breakableScreen;

    [Header("Hit Direction Indicator")]
    [SerializeField]
    private RectTransform _hitDirectionArrowsParent;
    [SerializeField]
    private GameObject _hitDirectionArrowPrefab;
    [SerializeField]
    private float _hitIndicatorDuration = 0.5f;

    private List<HitDirectionalIndicator> _hitDirectionArrows;

    //IDEE -> onspawned -> creeaza cate 1 arrow pt fiecare player. Stim din Gameplay.Players cate avem. Fiecare inamic va actualiza doar 1 arrow, ne vom lua dupa idnexul sau din Gameplay.Players

    [Header("Animation")]
    [SerializeField]
    private float _hitFadeInDuratio = 0.1f;
    [SerializeField]
    private float _hitFadeOutDuration = 0.7f;

    [Header("Audio")]
    [SerializeField]
    private AudioSetup _hitSound;
    [SerializeField]
    private AudioSetup _deathSound;

    private AgentStateMachine _agent;

    // PUBLIC METHODS

    protected override void Awake() {
        CreateDirectionalHitIndicators();
    }

    private void Update() {
        UpdateHitDirectionalIndicators();

    }

    private void UpdateHitDirectionalIndicators() {
        for (int i = 0; i < _hitDirectionArrows.Count; i++) {

            if (_hitDirectionArrows[i]._indicator.gameObject.activeInHierarchy == false)
                continue;

            if (_hitDirectionArrows[i]._timer <= 0) {

                _hitDirectionArrows[i]._indicator.gameObject.SetActive(false);
            }
            else {
                _hitDirectionArrows[i]._timer -= Time.deltaTime;
            }
        }
    }

    //Create an hitIndicatorDirectionArrow for eeach player
    // Should be called when GameplayUI starts
    private void CreateDirectionalHitIndicators() {
        int arrowsCount = Context.Instance.Gameplay.Players.Count;
        _hitDirectionArrows = new List<HitDirectionalIndicator>(arrowsCount);
        for (int i = 0; i < arrowsCount; i++) {


            var arrow = Instantiate(_hitDirectionArrowPrefab, _hitDirectionArrowsParent).GetComponent<RectTransform>();
            arrow.anchorMin = new Vector2(0, 0);
            arrow.anchorMax = new Vector2(1, 1);
            arrow.offsetMin = Vector2.zero;
            arrow.offsetMax = Vector2.zero;

            arrow.SetActive(false);
            _hitDirectionArrows.Add(new HitDirectionalIndicator(arrow));
        }
    }

    public void OnHitTaken(HitData hit) {
        if (hit.Amount <= 0)
            return;


        List<KeyValuePair<PlayerRef, RoomPlayer>> myList = Context.Instance.Gameplay.Players.ToList();
        int index = myList.FindIndex(entry => entry.Value == RoomPlayer.LocalRoomPlayer);

        EnableHitDirectionalIndicator(_hitDirectionArrows[index]);
        PointHitDirectionalIndicator(_hitDirectionArrows[index]._indicator, RoomPlayer.LocalRoomPlayer.ActiveAgent.transform.forward, hit.Direction);

        if (hit.Action == EHitAction.Damage) {


            ShowHitEffects(hit);

            if (hit.IsFatal == true) {
                _deathGroup.SetActive(true);
                PlaySound(_deathSound, EForceBehaviour.ForceAny);
            }
        }
    }



    public void UpdateEffects(AgentStateMachine agent) {
        if (!_agent)
            _agent = agent;


        _deathGroup.SetActive(agent.Health.IsAlive == false);
        _breakableScreen.SetActive(agent.Health.IsAlive == false);
    }

    // MONOBEHAVIOUR

    protected override void OnVisible() {
        base.OnVisible();

        _hitGroup.SetActive(true);
        _hitGroup.alpha = 0f;

        _bloodScreen1.SetActive(true);
        _bloodScreen1.alpha = 0f;

        _bloodScreen2.SetActive(true);
        _bloodScreen2.alpha = 0f;

        _deathGroup.SetActive(false);
    }

    // PRIVATE METHODS

    private void ShowHit(CanvasGroup group, float targetAlpha) {
        DOTween.Kill(group);

        group.DOFade(targetAlpha, _hitFadeInDuratio);
        group.DOFade(0f, _hitFadeOutDuration).SetDelay(_hitFadeInDuratio);
    }

    private void ShowHitEffects(HitData hit) {
        float alpha = Mathf.Lerp(0, 1f, hit.Amount / 20f);

        ShowHit(_hitGroup, alpha);

        if (_agent) {

            if (_bloodScreen1 && (_agent.Health.CurrentHealth < _agent.Health.MaxHealth * _threshhold1))
                ShowHit(_bloodScreen1, alpha);
            if (_bloodScreen2 && (_agent.Health.CurrentHealth < _agent.Health.MaxHealth * _threshhold2))
                ShowHit(_bloodScreen2, alpha);
        }

        PlaySound(_hitSound, EForceBehaviour.ForceAny);
    }

   
    private void EnableHitDirectionalIndicator(HitDirectionalIndicator arrow) {

        if (arrow._indicator.gameObject.activeInHierarchy == false)
            arrow._indicator.SetActive(true);

       
        arrow._image.rectTransform.DOScaleY(2, 0f);
        arrow._image.rectTransform.DOScaleY(1, 0.3f);

        arrow._timer = _hitIndicatorDuration;
    }
    private void PointHitDirectionalIndicator(RectTransform arrow, Vector3 playerForward, Vector3 hitDirection) {
        float angle = Vector3.Angle(playerForward, hitDirection);
        Vector3 cross = Vector3.Cross(playerForward, hitDirection);
        if (cross.y > 0) {
            angle = -angle;
        }
        arrow.rotation = Quaternion.Euler(0, 0, angle);
    }



    private class HitDirectionalIndicator {

        public HitDirectionalIndicator(RectTransform indicator) {

            _indicator = indicator;
            _image = indicator.GetComponentInChildren<Image>();
            _timer = 0;
        }

        public void SetTimer(float x) {

            _timer = x;
        }

        public RectTransform _indicator;
        public Image _image;
        public float _timer;
    }
}

