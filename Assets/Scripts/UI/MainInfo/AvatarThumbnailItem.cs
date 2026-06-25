using UnityEngine;
using UnityEngine.UI;

public class AvatarThumbnailItem : MonoBehaviour
{
    public Image thumbnailImage;
    public Button button;
    public GameObject selectedMark;

    protected AvatarThumbnailPanel owner;

    public int id;

    void Reset()
    {
        if (!thumbnailImage) thumbnailImage = GetComponent<Image>();
        if (!button) button = GetComponent<Button>();
    }

    public virtual void Init(int id, AvatarThumbnailPanel o, bool selected)
    {
        this.id = id;
        owner  = o;

        if (thumbnailImage)
        {
            thumbnailImage.preserveAspect = true;

            // ★ 가장 중요: overrideSprite 초기화 후 sprite 지정
            thumbnailImage.overrideSprite = null;
            thumbnailImage.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.CharacterIconAtlas,
                $"Thum_SD_Cr_{id.ToString("000")}");
        }

        // Button이 SpriteSwap이면 전부 같은 스프라이트로 보일 수 있음
        if (button)
        {
            button.transition = Selectable.Transition.ColorTint;
            button.spriteState = default;
            button.targetGraphic = thumbnailImage;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => owner.OnItemClicked(this));
        }

        SetSelected(selected);

#if UNITY_EDITOR
        MyLogger.Log($"[ThumbItem] {name} <- {thumbnailImage.sprite?.name} (id:{thumbnailImage.sprite?.GetInstanceID()})");
#endif
    }


    public void SetSelected(bool on)
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        if (selectedMark) selectedMark.SetActive(on);
    }
}