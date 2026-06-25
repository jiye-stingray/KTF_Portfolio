using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class UIButtonBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private GameObject grayObject;

    [Header("Events")]
    public UnityEvent onClick;
    public UnityEvent onGrayClick;

    public bool IsGray => isGray;
    private bool isGray = false;

    public bool IgnoreSoundFX = false;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(HandleClick);
    }

    protected virtual void HandleClick()
    {
        if (!button.interactable)
            return;

        if (isGray)
        {
            onGrayClick?.Invoke();
            return;
        }

        onClick?.Invoke();

        if (!IgnoreSoundFX)
        {
            // TODO: 사운드 재생
        }
    }

    public void SetGray(bool value)
    {
        isGray = value;

        if (grayObject != null)
            grayObject.SetActive(value);

        button.interactable = !value; // 핵심!
    }

    public void SetInteractable(bool value)
    {
        button.interactable = value;
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}