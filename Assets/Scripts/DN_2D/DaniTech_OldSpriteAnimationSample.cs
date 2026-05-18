using System.Collections.Generic;
using UnityEngine;

public class SimpleSpriteAnimator : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private List<Sprite> animationFrames; // 재생할 프레임 리스트
    [SerializeField] private float frameDuration = 1.0f;  // 프레임 전환 간격 (초)
    [SerializeField] private bool loop = true;            // 반복 재생 여부

    private SpriteRenderer spriteRenderer;
    private int currentFrameIndex = 0;
    private float timer = 0f;

    void Start()
    {
        // 이미지를 표시할 컴포넌트 참조
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 시작 시 첫 번째 프레임 설정
        if (animationFrames.Count > 0)
        {
            spriteRenderer.sprite = animationFrames[0];
        }
    }

    void Update()
    {
        // 프레임이 없거나 반복이 끝났으면 실행 중지
        if (animationFrames.Count == 0 || (!loop && currentFrameIndex >= animationFrames.Count - 1))
            return;

        // 시간 측정
        timer += Time.deltaTime;

        // 설정한 시간이 지나면 다음 프레임으로 교체
        if (timer >= frameDuration)
        {
            timer = 0f;
            NextFrame();
        }
    }

    void NextFrame()
    {
        currentFrameIndex++;

        // 마지막 프레임에 도달했을 때의 처리
        if (currentFrameIndex >= animationFrames.Count)
        {
            if (loop)
            {
                currentFrameIndex = 0; // 처음으로 되돌리기
            }
            else
            {
                currentFrameIndex = animationFrames.Count - 1; // 마지막 프레임 유지
                return;
            }
        }

        // 실제 Sprite 이미지 교체
        spriteRenderer.sprite = animationFrames[currentFrameIndex];
    }
}