
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static LocalizationService;

public class LanguageUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform content;      // ScrollView Content
    [SerializeField] private GameObject itemPrefab;  // 프리팹(자식에 TMP_Text/UGUI Button)
    [SerializeField] private Button closeButton;

    [Header("Behavior")]
    [SerializeField] private bool rebuildEveryOpen = true; // 열 때마다 재구성
    [SerializeField] private bool closeOnSelect = true;    // 선택 후 닫기
    [SerializeField] private float disabledAlpha = 0.5f;   // 현재 언어 항목 투명도

    // 에디터에서 후처리 연결용
    [Serializable] public class LanguageEvent : UnityEngine.Events.UnityEvent<Language> {}
    public LanguageEvent OnSelected;

    // 외부 콜백 (Show로 주입 가능)
    private Action<Language> _onSelected;

    // 버튼 ↔ 언어 매핑
    private readonly Dictionary<Button, Language> _map = new();
    private bool _built;

    // Show 오버라이드용 현재 언어 강조값
    private bool _useOverride;
    private Language _overrideCurrent;

    // -------- lifecycle --------
    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        LocalizationService.InitFromPrefsOnce();
        LocalizationService.OnLanguageChanged += HandleLanguageChanged;

        if (!_built || rebuildEveryOpen)
        {
            BuildList();
            _built = true;
        }

        RefreshButtons(); // 현재 언어 강조/클릭 상태 반영
    }

    private void OnDisable()
    {
        LocalizationService.OnLanguageChanged -= HandleLanguageChanged;
        // 임시 강조값은 한 번 표시용이라면 해제
        _useOverride = false;
    }

    // -------- public API --------
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Show(Action<Language> onSelected)
    {
        _onSelected = onSelected;
        gameObject.SetActive(true);
    }

    public void Show(Action<Language> onSelected, Language highlightAsCurrent)
    {
        _onSelected = onSelected;
        _overrideCurrent = highlightAsCurrent;
        _useOverride = true;          // 이번 오픈 동안만 highlight 적용
       
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    // -------- build & refresh --------
    private void BuildList()
    {
        ClearContent();
        _map.Clear();

        foreach (Language lang in Enum.GetValues(typeof(Language)))
        {
            var go = Instantiate(itemPrefab, content);

            // 라벨
            var text = go.GetComponentInChildren<TMP_Text>(true);
            if (text != null) text.text = LocalizationService.GetDisplay(lang);

            // 버튼
            var btn = go.GetComponentInChildren<Button>(true);
            if (btn == null)
            {
                Debug.LogError("LanguageUI: itemPrefab에 UGUI Button이 없습니다.");
                continue;
            }

            btn.onClick.RemoveAllListeners();
            var captured = lang;
            btn.onClick.AddListener(() =>
            {
                
                Debug.LogError("LocalizationService OnClick = " + captured);
                // 실제 언어 적용 + 저장
                LocalizationService.Set(captured);

                // 외부 콜백 & 유니티 이벤트
                _onSelected?.Invoke(captured);
                OnSelected?.Invoke(captured);

                if (closeOnSelect) Hide();
            });

            _map[btn] = captured;
        }
    }

    private void RefreshButtons()
    {
        var current = GetCurrentHighlight();

        foreach (var kv in _map)
        {
            var btn = kv.Key;
            var isCurrent = kv.Value.Equals(current);
            SetButtonClickable(btn, !isCurrent);
        }
    }

    private Language GetCurrentHighlight()
        => _useOverride ? _overrideCurrent : LocalizationService.Current;

    private void HandleLanguageChanged(Language _)
    {
        RefreshButtons(); // 외부에서 바뀌어도 바로 반영
    }

    // -------- visuals & input gating --------
    private void SetButtonClickable(Button btn, bool clickable)
    {
        if (btn == null) return;

        // 1) 입력 자체 차단
        btn.enabled = clickable;

        // // 2) 레이캐스트 차단/허용
        // var g = btn.targetGraphic as Graphic;
        // if (g != null) g.raycastTarget = clickable;

        // 3) 시각적 피드백(회색 처리)
        var cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = clickable ? 1f : disabledAlpha;

        // // 4) 입력 탐색(키보드/패드) 차단
        // var nav = btn.navigation;
        // nav.mode = clickable ? Navigation.Mode.Automatic : Navigation.Mode.None;
        // btn.navigation = nav;
    }

    private void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; --i)
            Destroy(content.GetChild(i).gameObject);
    }
}


