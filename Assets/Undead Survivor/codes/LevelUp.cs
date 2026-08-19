using UnityEngine;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;

    private int activeItemCount;

    // ➕ 이번에 고를 수 있는 남은 횟수 저장 변수
    private int remainingCount = 0;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
        activeItemCount = items.Length - 1;
    }

    // 🎯 [기존과 동일] 일반 경험치 레벨업용 함수 (인자값 없으므로 다른 소스코드 에러 안 남)
    public void Show()
    {
        remainingCount = 1; // 일반 레벨업은 딱 1번만 고르기

        Next();
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    // 🚨 [새로 추가] 보스 처치용 5연속 레벨업 전용 함수!
    public void ShowBossReward()
    {
        remainingCount = 5; // 보스 보상은 총 5번 고르기

        Next();
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    public void Hide()
    {
        // 아이템을 하나 골랐으므로 남은 횟수를 1 감소시킵니다.
        remainingCount--;

        // 만약 아직도 고를 횟수가 남아있다면 (예: 5번 중 1번 골라서 4번 남은 경우)
        if (remainingCount > 0)
        {
            Next(); // 새로운 아이템 3개로 리프레시
            AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
            return; // 창을 닫지 않고 유지
        }

        // 남은 횟수가 0 이하이면 정상적으로 창을 닫고 게임 재개
        remainingCount = 0;
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
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        int[] ran = new int[3];
        while (true)
        {
            ran[0] = Random.Range(0, activeItemCount);
            ran[1] = Random.Range(0, activeItemCount);
            ran[2] = Random.Range(0, activeItemCount);

            if (ran[0] != ran[1] && ran[1] != ran[2] && ran[0] != ran[2])
                break;
        }

        for (int index = 0; index < ran.Length; index++)
        {
            Item ranItem = items[ran[index]];

            if (ranItem.level == ranItem.data.damages.Length)
            {
                items[4].gameObject.SetActive(true);
            }
            else
            {
                ranItem.gameObject.SetActive(true);
            }
        }
    }

    public void UnlockItem(int itemId)
    {
        activeItemCount = items.Length;
        Debug.Log($"🔓 [아이템 해금 완료] 이제 ID {itemId} 아이템이 레벨업 선택지에 등장합니다!");
    }
}
