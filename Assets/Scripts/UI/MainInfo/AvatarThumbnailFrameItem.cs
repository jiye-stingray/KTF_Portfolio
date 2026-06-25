using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarThumbnailFrameItem : AvatarThumbnailItem
{
    public override void Init(int id, AvatarThumbnailPanel o, bool selected)
    {
        this.id = id;
        owner = o;

        if (thumbnailImage)
        {
            thumbnailImage.preserveAspect = true;

            // ★ 가장 중요: overrideSprite 초기화 후 sprite 지정
            thumbnailImage.overrideSprite = null;
            thumbnailImage.sprite = Managers.Instance.GetAtlasManager().GetSprite(Define.EAtlasType.FrameAtlas,
                $"FrameImg_{id.ToString("000")}");
        }

        // Button이 SpriteSwap이면 전부 같은 스프라이트로 보일 수 있음
        if (button)
        {
            button.transition = Selectable.Transition.ColorTint;
            button.spriteState = default;
            button.targetGraphic = thumbnailImage;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => owner.OnFrameItemClicked(this));
        }

        SetSelected(selected);
    }
}
