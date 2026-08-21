using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;

    Item[] items;

    // =========================================================
    // 처음부터 해금되어 있는 아이템 개수
    // =========================================================

    [Header("처음부터 해금할 아이템 개수")]
    [SerializeField]
    private int startingUnlockedItemCount = 4;


    // =========================================================
    // 특수 아이템 해금 대상
    // =========================================================

    [Header("특수 아이템 해금")]
    [Tooltip("Boss 1 처치 후 해금할 용기의 권능")]
    [SerializeField]
    private Item couragePowerItem;

    [Tooltip("2Boss 1페이지 처치 후 해금할 블러드 히트")]
    [SerializeField]
    private Item bloodHitItem;

    [Tooltip("2Boss 2페이지 처치 후 해금할 희망의 호프")]
    [SerializeField]
    private Item hopeOfHopeItem;


    // =========================================================
    // 음료수
    // =========================================================

    [Header("최대 레벨 아이템 대체용")]
    [Tooltip("최대 레벨 아이템 대신 등장시킬 음료수")]
    [SerializeField]
    private Item healItem;


    // =========================================================
    // 현재 해금된 아이템
    // =========================================================

    private HashSet<Item> unlockedItems =
        new HashSet<Item>();


    // =========================================================
    // 현재 남은 선택 횟수
    // =========================================================

    private int remainingCount = 0;


    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        rect =
            GetComponent<RectTransform>();

        items =
            GetComponentsInChildren<Item>(true);


        // -----------------------------------------------------
        // 처음부터 해금된 아이템 등록
        // -----------------------------------------------------

        for (int i = 0;
             i < items.Length;
             i++)
        {
            Item item = items[i];

            if (item == null)
                continue;

            if (item == healItem)
                continue;

            if (item == couragePowerItem)
                continue;

            if (item == bloodHitItem)
                continue;

            if (item == hopeOfHopeItem)
                continue;

            if (i < startingUnlockedItemCount)
            {
                unlockedItems.Add(item);
            }
        }


        // -----------------------------------------------------
        // 음료수는 항상 사용 가능
        // -----------------------------------------------------

        if (healItem != null)
        {
            unlockedItems.Add(healItem);
        }


        // -----------------------------------------------------
        // 특수 아이템은 처음에는 잠금
        // -----------------------------------------------------

        if (couragePowerItem != null)
        {
            couragePowerItem.gameObject.SetActive(false);
        }

        if (bloodHitItem != null)
        {
            bloodHitItem.gameObject.SetActive(false);
        }

        if (hopeOfHopeItem != null)
        {
            hopeOfHopeItem.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // 일반 레벨업
    // =========================================================

    public void Show()
    {
        remainingCount = 1;

        Next();

        rect.localScale =
            Vector3.one;

        GameManager.instance.Stop();

        AudioManager.instance.PlaySfx(
            AudioManager.Sfx.LevelUp
        );

        AudioManager.instance.EffectBgm(true);
    }


    // =========================================================
    // Boss 1 보상
    // =========================================================

    public void ShowBossReward()
    {
        remainingCount = 5;

        Next();

        rect.localScale =
            Vector3.one;

        GameManager.instance.Stop();

        AudioManager.instance.PlaySfx(
            AudioManager.Sfx.LevelUp
        );

        AudioManager.instance.EffectBgm(true);
    }


    // =========================================================
    // Boss 2 페이즈 보상
    // =========================================================

    public void ShowBossPhaseReward()
    {
        remainingCount = 1;

        Next();

        rect.localScale =
            Vector3.one;

        GameManager.instance.Stop();

        AudioManager.instance.PlaySfx(
            AudioManager.Sfx.LevelUp
        );

        AudioManager.instance.EffectBgm(true);

        Debug.Log(
            "<color=cyan>" +
            "[Boss2 보상]</color> " +
            "아이템 1개를 선택하세요."
        );
    }


    // =========================================================
    // UI 닫기 / 다음 선택
    // =========================================================

    public void Hide()
    {
        remainingCount--;

        if (remainingCount > 0)
        {
            Next();

            AudioManager.instance.PlaySfx(
                AudioManager.Sfx.LevelUp
            );

            return;
        }


        remainingCount = 0;

        rect.localScale =
            Vector3.zero;

        GameManager.instance.Resume();

        AudioManager.instance.PlaySfx(
            AudioManager.Sfx.Select
        );

        AudioManager.instance.EffectBgm(false);
    }


    // =========================================================
    // 아이템 선택
    // =========================================================

    public void Select(int index)
    {
        if (index < 0 ||
            index >= items.Length)
        {
            Debug.LogWarning(
                $"⚠️ 잘못된 아이템 인덱스: {index}"
            );

            return;
        }

        Item item = items[index];

        if (item == null ||
            !item.gameObject.activeSelf)
        {
            return;
        }

        item.OnClick();
    }


    // =========================================================
    // 랜덤 아이템 생성
    // =========================================================

    void Next()
    {
        // -----------------------------------------------------
        // 모든 선택지 숨기기
        // -----------------------------------------------------

        foreach (Item item in items)
        {
            if (item == null)
                continue;

            item.gameObject.SetActive(false);
        }


        // -----------------------------------------------------
        // 선택 가능한 아이템 찾기
        // -----------------------------------------------------

        List<Item> availableItems =
            new List<Item>();


        foreach (Item item in unlockedItems)
        {
            if (item == null)
                continue;

            // 특수 아이템이 아직 해금되지 않았다면 제외
            if (!IsItemUnlocked(item))
                continue;

            // 최대 레벨 아이템 제외
            if (IsMaxLevel(item))
                continue;

            availableItems.Add(item);
        }


        // -----------------------------------------------------
        // 최대 레벨이 아닌 아이템 중 랜덤 3개
        // -----------------------------------------------------

        Shuffle(availableItems);


        int showCount =
            Mathf.Min(3, availableItems.Count);


        for (int i = 0;
             i < showCount;
             i++)
        {
            ShowItem(availableItems[i]);
        }


        // -----------------------------------------------------
        // 선택 가능한 아이템이 3개보다 적으면
        // 음료수를 추가해서 3개를 맞춘다.
        // -----------------------------------------------------

        if (showCount < 3 &&
            healItem != null)
        {
            int needCount =
                3 - showCount;

            for (int i = 0;
                 i < needCount;
                 i++)
            {
                ShowHealIfPossible();
            }
        }


        // -----------------------------------------------------
        // 최종 확인
        // -----------------------------------------------------

        int visibleCount = 0;

        foreach (Item item in items)
        {
            if (item != null &&
                item.gameObject.activeSelf)
            {
                visibleCount++;
            }
        }


        Debug.Log(
            $"🎲 레벨업 선택지 생성 완료: " +
            $"{visibleCount}개"
        );
    }


    // =========================================================
    // 아이템 표시
    // =========================================================

    void ShowItem(Item item)
    {
        if (item == null)
            return;

        item.gameObject.SetActive(true);

        Button button =
            item.GetComponent<Button>();

        if (button != null)
        {
            button.interactable = true;
        }

        item.UpdateUI();
        item.UpdateButtonState();
    }


    // =========================================================
    // 음료수 표시
    // =========================================================

    void ShowHealIfPossible()
    {
        if (healItem == null)
            return;

        // 이미 표시되어 있다면 중복 표시하지 않음
        if (healItem.gameObject.activeSelf)
            return;

        healItem.gameObject.SetActive(true);

        Button button =
            healItem.GetComponent<Button>();

        if (button != null)
        {
            button.interactable = true;
        }

        healItem.UpdateUI();
    }


    // =========================================================
    // 최대 레벨 확인
    // =========================================================

    bool IsMaxLevel(Item item)
    {
        if (item == null)
            return true;

        if (item.data == null)
            return true;

        // 음료수는 레벨이 없음
        if (item.data.itemType ==
            ItemData.ItemType.Heal)
        {
            return false;
        }

        return item.level >=
               item.data.damages.Length;
    }


    // =========================================================
    // 아이템 해금 여부
    // =========================================================

    bool IsItemUnlocked(Item item)
    {
        return unlockedItems.Contains(item);
    }


    // =========================================================
    // 아이템 해금
    // =========================================================

    public void UnlockItem(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning(
                "⚠️ 해금하려는 Item이 없습니다."
            );

            return;
        }


        if (unlockedItems.Contains(item))
        {
            Debug.Log(
                $"⚠️ 이미 해금된 아이템입니다: " +
                $"{item.data.itemName}"
            );

            return;
        }


        unlockedItems.Add(item);


        // 해금된 아이템은 다시 활성화 가능하도록 설정
        item.gameObject.SetActive(true);


        Debug.Log(
            $"🔓 <color=green>" +
            $"[아이템 해금 완료]</color> " +
            $"{item.data.itemName}"
        );
    }


    // =========================================================
    // 🛡️ Boss 1 처치
    // → 용기의 권능 해금
    // =========================================================

    public void UnlockCouragePower()
    {
        if (couragePowerItem == null)
        {
            Debug.LogWarning(
                "⚠️ 용기의 권능 Item이 지정되지 않았습니다."
            );

            return;
        }

        UnlockItem(couragePowerItem);
    }


    // =========================================================
    // 🩸 2Boss 1페이지 처치
    // → 블러드 히트 해금
    // =========================================================

    public void UnlockBloodHit()
    {
        if (bloodHitItem == null)
        {
            Debug.LogWarning(
                "⚠️ 블러드 히트 Item이 지정되지 않았습니다."
            );

            return;
        }

        UnlockItem(bloodHitItem);
    }


    // =========================================================
    // ❤️‍🔥 2Boss 2페이지 처치
    // → 희망의 호프 해금
    // =========================================================

    public void UnlockHopeOfHope()
    {
        if (hopeOfHopeItem == null)
        {
            Debug.LogWarning(
                "⚠️ 희망의 호프 Item이 지정되지 않았습니다."
            );

            return;
        }

        UnlockItem(hopeOfHopeItem);
    }


    // =========================================================
    // 랜덤 섞기
    // =========================================================

    void Shuffle(List<Item> list)
    {
        for (int i = list.Count - 1;
             i > 0;
             i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            Item temp =
                list[i];

            list[i] =
                list[randomIndex];

            list[randomIndex] =
                temp;
        }
    }


    // =========================================================
    // 게임 재시작용 초기화
    // =========================================================

    public void ResetItemUnlocks()
    {
        unlockedItems.Clear();


        // 처음부터 해금된 아이템 복구
        for (int i = 0;
             i < items.Length;
             i++)
        {
            Item item = items[i];

            if (item == null)
                continue;

            if (item == couragePowerItem)
                continue;

            if (item == bloodHitItem)
                continue;

            if (item == hopeOfHopeItem)
                continue;

            if (i < startingUnlockedItemCount ||
                item == healItem)
            {
                unlockedItems.Add(item);
            }
        }


        // 특수 아이템 잠금
        if (couragePowerItem != null)
        {
            couragePowerItem.gameObject.SetActive(false);
        }

        if (bloodHitItem != null)
        {
            bloodHitItem.gameObject.SetActive(false);
        }

        if (hopeOfHopeItem != null)
        {
            hopeOfHopeItem.gameObject.SetActive(false);
        }


        // 특수 효과도 초기화
        Item.ResetSpecialItemEffects();


        Debug.Log(
            "🔄 아이템 해금 상태 초기화 완료"
        );
    }
}