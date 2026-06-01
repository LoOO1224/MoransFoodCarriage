using System.Collections.Generic;
using UnityEngine;

public class DaniTech_2DFieldObject : MonoBehaviour
{
    [SerializeField] private int _fieldObjectInstanceId;
    [SerializeField] private string _fieldObjectDataId;
    [SerializeField] private string _fieldObjectName;

    public void InitFieldObjectInfoOnCreated(int instanceId, string fieldObjectDataId)
    {
        var fieldObjectData = DaniTechGameDataManager.Instance.GetDNFieldObjectData(fieldObjectDataId);
        if (fieldObjectData == null) 
        {
            Debug.LogWarning($"유효하지 않은 필드 오브젝트 데이터 입니다! {fieldObjectDataId}");
            return;
        }

        _fieldObjectInstanceId = instanceId;
        _fieldObjectDataId = fieldObjectDataId;
    }

    public string GetFieldObjectDataId()
    {
        return _fieldObjectDataId;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") == true)
        {
            // 플레이어와 충돌했을때 이제 GameManager에 아이템을 저장해준다던가 등등 처리
            // 필요에 따라 역할을 다하면 지우거나 비활성화 해주자 = 아래 둘 중 하나 사용
            // this.gameObject.SetActive(false);
            // Destroy(this.gameObject);

            // 채집과 드랍 1-0) 내가 상호작용한 필드 오브젝트의 타입에 따라 처리를 추가해봅시다
            var fieldObjectData = DaniTechGameDataManager.Instance.GetDNFieldObjectData(_fieldObjectDataId);
            if (fieldObjectData == null)
            {
                Debug.LogWarning($"유효하지 않은 필드 오브젝트 데이터 입니다! {_fieldObjectDataId}");
                return;
            }

            // 채집과 드랍 1-1) 내가 상호작용한 필드 오브젝트가 채집물이거나 드랍아이템 유형인지 확인 (Enum으로 바꿔서 쓰면 더 좋다)
            if(fieldObjectData.FieldObjectType == "Harvest"  ||  fieldObjectData.FieldObjectType == "DropItem")
            {
                if (string.IsNullOrEmpty(fieldObjectData.DropItemDataId))
                {
                    // 드랍 아이템이 없다면 더이상 처리하지 않는다
                    return;
                }

                // 채집과 드랍 1-2) 채집물이나 드랍이 맞으면 "아이템 정보를 찾아서" 인벤토리에 추가해주자
                var itemData = DaniTechGameDataManager.Instance.GetDNItemData(fieldObjectData.DropItemDataId);
                if(itemData == null)
                {
                    Debug.LogWarning($"유효하지 않은 아이템 데이터 입니다! {_fieldObjectDataId}");
                    return;
                }

                // 채집과 드랍 1-3) 아이템 드랍 수를 FieldObject 데이터의 DropCountRange 범위 내에서 랜덤으로 가져오자
                List<int> dropCountRange = fieldObjectData.DropCountRange;
                int finalDropItemCount = 1;

                if (dropCountRange != null && dropCountRange.Count > 0)
                {
                    // 다항 연산자로 카운트가 2개 이상이면 첫번째와 두번째 수를 최소, 최대로 랜덤값을 구해오고, 1개만 기입되어 있다면 그 수량을 무조건 획득
                    finalDropItemCount = dropCountRange.Count > 1 ? Random.Range(dropCountRange[0], dropCountRange[1]) : dropCountRange[0];
                }

                // 채집과 드랍 1-4) 게임 매니저에게 "어디서든"(현재는 채집물이 충돌된 시점) 편하게 아이템 추가를 요청한다!
                DaniTechGameManager.Inst.AddItem(itemData.Id, finalDropItemCount);


                // 채집과 드랍 1-5) 추가 완료 되었다면 이 오브젝트를 비활성화 또는 제거하자 (우리는 제거를 선택)
                DaniTechGameObjectManager.Inst.RequestDestroyFieldObject(_fieldObjectInstanceId);
            }
        }
    }
}
