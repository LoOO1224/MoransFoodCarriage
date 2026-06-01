using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DaniTechUIImageSequencer : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private Sprite[] SpriteArray_Sprite; // 순회할 스프라이트 배열
    [SerializeField] private float _sequenceInterval = 0.1f; // 프레임 간격 (초)
    [SerializeField] private bool _isLoop = true;    // 반복 여부

    private Image _image;
    private CancellationTokenSource _cancelToken;

    void Awake()
    {
        _image = this.GetComponent<Image>();
    }

    void OnEnable()
    {
        // 활성화될 때 알아서 애니메이션 시작
        PlayAnimation().Forget();
    }

    void OnDisable()
    {
        // 비활성화될 때 작업 취소
        StopAnimation();
    }

    public async UniTaskVoid PlayAnimation()
    {
        if (SpriteArray_Sprite == null || SpriteArray_Sprite.Length == 0) return;

        // 이전 작업이 있다면 취소 후 새로 생성
        StopAnimation();
        _cancelToken = new CancellationTokenSource();

        int currentIndex = 0;

        while (true)
        {
            // 이미지 교체
            _image.sprite = SpriteArray_Sprite[currentIndex];

            // 설정한 시간만큼 대기 (밀리초 단위로 변환)
            await UniTask.Delay(TimeSpan.FromSeconds(_sequenceInterval), cancellationToken: _cancelToken.Token);

            currentIndex++;

            // 마지막 프레임에 도달했을 때
            if (currentIndex >= SpriteArray_Sprite.Length)
            {
                if (_isLoop)
                {
                    currentIndex = 0; // 반복이면 처음으로
                }
                else
                {
                    break; // 한 번만 재생이면 루프 탈출
                }
            }
        }
    }

    public void StopAnimation()
    {
        if (_cancelToken != null)
        {
            _cancelToken.Cancel();
            _cancelToken.Dispose();
            _cancelToken = null;
        }
    }

    void OnDestroy()
    {
        StopAnimation();
    }
}
