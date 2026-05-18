using UnityEngine;
using UnityEngine.UI;

// + 그래픽 렌더링은 하지 않지만 레이캐스트는 받는 컴포넌트
    // 버튼의 자식이나, 버튼에 컴포넌트로 붙여서 쓴다
public class DaniTechUIEmptyRect : Graphic
{
    public override void SetAllDirty() { }
    public override void Rebuild(CanvasUpdate update) { }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        // 메쉬를 비워서 아무것도 그리지 않게 함
        vh.Clear();
    }
}
