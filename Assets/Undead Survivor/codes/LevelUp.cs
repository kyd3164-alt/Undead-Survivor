using UnityEngine;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;

    // ➕ [추가] 현재 랜덤 가챠에 등장할 수 있는 최대 아이템 수
    // (초기값: Item 6 제외를 위해 전체 개수 - 1 또는 기본 아이템 수)
    private int activeItemCount;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);

        // 💡 처음에는 마지막 아이템(Item 6)을 제외한 수만큼만 가챠에 등장시킵니다.
        // (만약 Canvas 하위에 Item 0~6까지 총 7개가 있다면 초기엔 6개만 뽑음)
        activeItemCount = items.Length - 1;
    }

    public void Show()
    {
        Next();
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
    }

    public void Select(int index)
    {
        items[index].OnClick();
    }

    void Next()
    {
        // 1. 모든 아이템 비활성화
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        // 2. 해금된 아이템 범위(activeItemCount) 내에서 랜덤 3개 아이템 활성화
        int[] ran = new int[3];
        while (true)
        {
            // 💡 items.Length 대신 activeItemCount 범위 내에서만 무작위 추출!
            ran[0] = Random.Range(0, activeItemCount);
            ran[1] = Random.Range(0, activeItemCount);
            ran[2] = Random.Range(0, activeItemCount);

            if (ran[0] != ran[1] && ran[1] != ran[2] && ran[0] != ran[2])
                break;
        }

        for (int index = 0; index < ran.Length; index++)
        {
            Item ranItem = items[ran[index]];

            // 3. 만렙 아이템의 경우는 소비아이템(Heal 등)으로 대체
            if (ranItem.level == ranItem.data.damages.Length)
            {
                // Heal 아이템 index에 맞게 선택 (기존 코드 유지)
                items[4].gameObject.SetActive(true);
            }
            else
            {
                ranItem.gameObject.SetActive(true);
            }
        }
    }

    // ➕ [추가] 보스 처치 시 호출할 아이템 해금 함수
    public void UnlockItem(int itemId)
    {
        // 전체 아이템 개수 범위까지 가챠 한도를 늘려 Item 6이 선택지에 나오게 함!
        activeItemCount = items.Length;
        Debug.Log($"🔓 [아이템 해금 완료] 이제 ID {itemId} 아이템이 레벨업 선택지에 등장합니다!");
    }
}