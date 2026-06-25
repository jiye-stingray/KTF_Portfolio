using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Define;

public class GachaTap : UITabBase
{
    [Header("Description")]
    [SerializeField] protected TMP_Text _titleText;
    [SerializeField] protected TMP_Text _descriptionText;
    [SerializeField] protected TMP_Text _nextAnnouncementText;
    
    [Header("Image Rotation")]
    [SerializeField] private Image _currentImage;
    [SerializeField] private Image _nextImage;

    private const float FadeDuration = 0.4f;
    private CancellationTokenSource _imageRotationCts;
    private int _currentImageIndex;

    [Header("Button")]
    [SerializeField] protected GachaCostButton _adGachaCostBtn;
    [SerializeField] protected GachaCostButton _oneGachaCostBtn;
    [SerializeField] protected GachaCostButton _tenGachaCostBtn;
    [SerializeField] private GameObject _wishButton;
    [SerializeField] private GameObject _characterDetailButton;
    
    [SerializeField] protected UITimer _timer;
    [SerializeField] private Slider _announcementSlider;
    
    protected UnitData _pickupCharacter;
    protected GachaGroup _gachaGroupData;
    protected GachaItemData _gachaItemData;
    
    protected EGachaType _gachaType;

    protected void Init(EGachaType gachaType)
    {
        _gachaType = gachaType;
        _gachaGroupData = ClientLocalDB_Simple.GetData<GachaGroup>(DBKey.GachaGroup, gachaType);
        _pickupCharacter = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter,
            ClientLocalDB_Simple.GetData<GachaSetting>(DBKey.GachaSetting, "PickUpCharacter").Value);
        _gachaItemData = UserInfoData._dicGachaItemData[gachaType];

        _titleText.text = _gachaGroupData.GachaName;
        _timer.gameObject.SetActive(_gachaType == EGachaType.PickUp);
        _characterDetailButton.SetActive(_gachaType != EGachaType.General);
        _wishButton.SetActive(_gachaType != EGachaType.PickUp);
        _adGachaCostBtn.gameObject.SetActive(gachaType != EGachaType.Celestial);
    }

    protected void StartImageRotation()
    {
        StopImageRotation();
        if (_gachaGroupData.ImageRotaion == null || _gachaGroupData.ImageRotaion.Length == 0)
            return;

        _currentImageIndex = 0;
        _imageRotationCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        ImageRotationLoop(_imageRotationCts.Token).Forget();
    }

    protected void StopImageRotation()
    {
        _imageRotationCts?.Cancel();
        _imageRotationCts?.Dispose();
        _imageRotationCts = null;
        _currentImage?.DOKill();
        _nextImage?.DOKill();
    }

    private async UniTaskVoid ImageRotationLoop(CancellationToken ct)
    {
        _currentImage.gameObject.SetActive(false);
        Sprite firstSprite = await AddressableLoader.LoadCachedAssetAsync<Sprite>(_gachaGroupData.ImageRotaion[_currentImageIndex]);
        if (ct.IsCancellationRequested || !gameObject.activeInHierarchy || firstSprite == null) return;

        _currentImage.sprite = firstSprite;
        _currentImage.gameObject.SetActive(true);
        _currentImage.DOFade(1f, FadeDuration);
        _nextImage.color = new Color(1f, 1f, 1f, 0f);

        while (!ct.IsCancellationRequested)
        {
            await UniTask.Delay(3000, cancellationToken: ct);
            if (ct.IsCancellationRequested || !gameObject.activeInHierarchy) break;

            int nextIndex = (_currentImageIndex + 1) % _gachaGroupData.ImageRotaion.Length;
            await CrossFadeTransition(nextIndex, ct);
            _currentImageIndex = nextIndex;
        }
    }

    private async UniTask CrossFadeTransition(int nextIndex, CancellationToken ct)
    {
        _nextImage.gameObject.SetActive(false);
        Sprite nextSprite = await AddressableLoader.LoadCachedAssetAsync<Sprite>(_gachaGroupData.ImageRotaion[nextIndex]);
        if (ct.IsCancellationRequested || !gameObject.activeInHierarchy || nextSprite == null) return;

        float width = _currentImage.rectTransform.rect.width;

        _nextImage.sprite = nextSprite;
        _nextImage.color = new Color(1f, 1f, 1f, 0f);
        _nextImage.rectTransform.anchoredPosition = new Vector2(width, 0f);
        _nextImage.gameObject.SetActive(true);

        _currentImage.rectTransform.anchoredPosition = Vector2.zero;

        // 좌측 슬라이드 + 크로스페이드 동시 실행
        _currentImage.rectTransform.DOAnchorPosX(-width, FadeDuration).SetEase(Ease.InOutQuad);
        _currentImage.DOFade(0f, FadeDuration);
        _nextImage.rectTransform.DOAnchorPosX(0f, FadeDuration).SetEase(Ease.InOutQuad);
        _nextImage.DOFade(1f, FadeDuration);

        await UniTask.Delay((int)(FadeDuration * 1000), cancellationToken: ct);
        if (ct.IsCancellationRequested || !gameObject.activeInHierarchy) return;

        (_currentImage, _nextImage) = (_nextImage, _currentImage);
        _currentImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    public override void Refresh()
    {
        if(_gachaType != EGachaType.Celestial)
            _adGachaCostBtn.Init((EGachaType)_gachaGroupData.GroupID, EGachaCountType.Ad);
        
        _oneGachaCostBtn.Init((EGachaType)_gachaGroupData.GroupID, EGachaCountType.One);
        _tenGachaCostBtn.Init((EGachaType)_gachaGroupData.GroupID, EGachaCountType.Ten);
        _nextAnnouncementText.text = $"{_gachaItemData._count}/{_gachaGroupData.CeilingCount}";
        _announcementSlider.value = (float)_gachaItemData._count / _gachaGroupData.CeilingCount;
        base.Refresh();
    }
}
