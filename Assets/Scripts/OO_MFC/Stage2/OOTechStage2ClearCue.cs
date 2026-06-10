// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechStage2ClearCue.cs
// - 역할: Stage2 연출, 대사, 보상, 카메라 큐를 분리해 처리합니다.
// - 유지보수: 큐시트 데이터와 씬 배치 오브젝트가 함께 맞아야 하므로 데이터 ID와 역할 오브젝트를 같이 확인합니다.
// =============================================================================
using System.Collections;
using UnityEngine;

/// <summary>
/// Stage2 클리어 퇴장 연출을 담당합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage2ClearCue : MonoBehaviour
{
    /// <summary>
    /// GreedyDuck이 EntryPoint_E로 이동할 동안 임시 콜리더를 켜고, 퇴장 후 다시 끕니다.
    /// </summary>
    public IEnumerator RequestPlayGreedyDuckExitRoutine(OOTechStageActorMotion greedyDuckActor, Transform escapeTransform, GameObject tempColliderObject, float escapeSpeed, float timeoutSeconds, float animationSpeed)
    {
        if (greedyDuckActor == null)
            yield break;

        Vector3 escapePosition = escapeTransform != null ? escapeTransform.position : greedyDuckActor.transform.position + Vector3.right * 500f;
        RequestSetTempColliderActive(tempColliderObject, true);
        greedyDuckActor.RequestSwitchToDynamicPhysics();
        yield return RequestRunExitMoveWithTimeoutRoutine(greedyDuckActor, escapePosition, escapeSpeed, timeoutSeconds, animationSpeed);
        yield return RequestBlinkAndHideRoutine(greedyDuckActor.gameObject);
        RequestSetTempColliderActive(tempColliderObject, false);
    }

    /// <summary>
    /// GreedyDuck이 EntryPoint_E에 도착하지 못해도 지정 시간이 지나면 퇴장 큐를 계속 진행합니다.
    /// 영화로 치면 배우가 출구까지 정확히 못 갔더라도, 컷 시간을 넘기면 조명을 꺼서 다음 장면으로 넘기는 안전장치입니다.
    /// </summary>
    private IEnumerator RequestRunExitMoveWithTimeoutRoutine(OOTechStageActorMotion greedyDuckActor, Vector3 escapePosition, float escapeSpeed, float timeoutSeconds, float animationSpeed)
    {
        if (greedyDuckActor == null)
            yield break;

        float safeTimeoutSeconds = Mathf.Max(0.25f, timeoutSeconds);
        Coroutine moveCoroutine = StartCoroutine(greedyDuckActor.MoveToWorldPositionRoutine(escapePosition, escapeSpeed, "GreedyDuck_isBurning", animationSpeed));
        float elapsedTime = 0f;

        while (elapsedTime < safeTimeoutSeconds && greedyDuckActor != null && greedyDuckActor.gameObject.activeInHierarchy)
        {
            if (Vector2.Distance(greedyDuckActor.transform.position, escapePosition) <= 1f)
                break;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        if (elapsedTime >= safeTimeoutSeconds)
            Debug.LogWarning("[OOTechStage2ClearCue] GreedyDuck exit timeout. Cue continues without waiting for EntryPoint_E.");
    }

    /// <summary>
    /// 임시 발판 Collider를 켜거나 끕니다.
    /// </summary>
    public void RequestSetTempColliderActive(GameObject tempColliderObject, bool isActive)
    {
        if (tempColliderObject == null)
            return;

        tempColliderObject.SetActive(isActive);
        Collider2D[] colliderArray = tempColliderObject.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D collider in colliderArray)
        {
            if (collider != null)
                collider.enabled = isActive;
        }
    }

    /// <summary>
    /// 배우를 몇 번 깜빡인 뒤 무대에서 숨깁니다.
    /// </summary>
    public IEnumerator RequestBlinkAndHideRoutine(GameObject targetObject)
    {
        if (targetObject == null)
            yield break;

        SpriteRenderer renderer = targetObject.GetComponentInChildren<SpriteRenderer>(true);

        for (int index = 0; index < 4; index++)
        {
            if (renderer != null)
                renderer.enabled = !renderer.enabled;

            yield return new WaitForSeconds(0.12f);
        }

        if (renderer != null)
            renderer.enabled = false;

        targetObject.SetActive(false);
    }
}
