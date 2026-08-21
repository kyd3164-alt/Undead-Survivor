using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;

    Item[] items;

    List<Item> displayedItems =
        new List<Item>();


    // =========================================================
    // 처음부터 해금
    // =========================================================

    [Header("처음부터 해금할 아이템 개수")]

    [SerializeField]
    private int startingUnlockedItemCount = 4;


    // =========================================================
    // 특수 아이템
    // =========================================================

    [Header("특수 아이템 해금")]

    [SerializeField]
    private Item couragePowerItem;

    [SerializeField]
    private Item bloodHitItem;

    [SerializeField]
    private Item hopeOfHopeItem;


    // =========================================================
    // Heal
    // =========================================================

    [Header("최대 레벨 대체 아이템")]

    [SerializeField]
    private Item healItem;


    // =========================================================
    // 해금 목록
    // =========================================================

    private HashSet<Item> unlockedItems =
        new HashSet<Item>();


    // =========================================================
    // 선택 횟수
    // =========================================================

    private int remainingCount;


    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        rect =
            GetComponent<RectTransform>();


        items =
            GetComponentsInChildren<Item>(true);


        ResetItemUnlocks();
    }


    // =========================================================
    // 일반 레벨업
    // =========================================================

    public void Show()
    {
        remainingCount = 1;

        Next();


        if (rect != null)
            rect.localScale =
                Vector3.one;


        if (GameManager.instance != null)
            GameManager.instance.Stop();


        PlayLevelUpSound();
    }


    // =========================================================
    // Boss1 보상
    // =========================================================

    public void ShowBossReward()
    {
        remainingCount = 5;

        Next();


        if (rect != null)
            rect.localScale =
                Vector3.one;


        if (GameManager.instance != null)
            GameManager.instance.Stop();


        PlayLevelUpSound();
    }


    // =========================================================
    // Boss2 페이즈 보상
    // =========================================================

    public void ShowBossPhaseReward()
    {
        remainingCount = 5;

        Next();


        if (rect != null)
            rect.localScale =
                Vector3.one;


        if (GameManager.instance != null)
            GameManager.instance.Stop();


        PlayLevelUpSound();


        Debug.Log(
            "<color=cyan>" +
            "[Boss2 보상]</color> " +
            "아이템 5회 선택"
        );
    }


    // =========================================================
    // 닫기
    // =========================================================

    public void Hide()
    {
        remainingCount--;


        if (remainingCount > 0)
        {
            Next();

            PlayLevelUpSound();

            return;
        }


        remainingCount = 0;


        ClearClones();


        if (rect != null)
            rect.localScale =
                Vector3.zero;


        if (GameManager.instance != null)
            GameManager.instance.Resume();


        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(
                AudioManager.Sfx.Select
            );


            AudioManager.instance.EffectBgm(
                false
            );
        }
    }


    // =========================================================
    // 선택
    // =========================================================

    public void Select(int index)
    {
        if (index < 0 ||
            index >= displayedItems.Count)
        {
            Debug.LogWarning(
                "잘못된 선택지 인덱스: " +
                index
            );

            return;
        }


        Item item =
            displayedItems[index];


        if (item == null)
            return;


        item.OnClick();

        Hide();
    }


    // =========================================================
    // Next
    // =========================================================

    void Next()
    {
        ClearClones();

        displayedItems.Clear();


        // -----------------------------------------------------
        // 전체 숨기기
        // -----------------------------------------------------

        foreach (Item item in items)
        {
            if (item == null)
                continue;


            item.gameObject.SetActive(
                false
            );
        }


        // -----------------------------------------------------
        // 후보
        // -----------------------------------------------------

        List<Item> availableItems =
            new List<Item>();


        foreach (Item item in unlockedItems)
        {
            if (item == null)
                continue;


            if (item == healItem)
                continue;


            if (!IsItemUnlocked(item))
                continue;


            if (IsMaxLevel(item))
                continue;


            availableItems.Add(
                item
            );
        }


        // -----------------------------------------------------
        // 랜덤
        // -----------------------------------------------------

        Shuffle(
            availableItems
        );


        // -----------------------------------------------------
        // 최대 3개
        // -----------------------------------------------------

        int showCount =
            Mathf.Min(
                3,
                availableItems.Count
            );


        for (int i = 0;
             i < showCount;
             i++)
        {
            Item item =
                availableItems[i];


            ShowItem(
                item
            );


            displayedItems.Add(
                item
            );
        }


        // -----------------------------------------------------
        // 부족한 칸은 Heal
        // -----------------------------------------------------

        while (
            displayedItems.Count < 3 &&
            healItem != null)
        {
            Item healClone =
                CreateHealClone();


            if (healClone == null)
                break;


            displayedItems.Add(
                healClone
            );
        }


        Debug.Log(
            "선택지 생성 완료: " +
            displayedItems.Count +
            " / 3"
        );
    }


    // =========================================================
    // Item 표시
    // =========================================================

    void ShowItem(
        Item item)
    {
        if (item == null)
            return;


        item.gameObject.SetActive(
            true
        );


        Button button =
            item.GetComponent<Button>();


        if (button != null)
            button.interactable = true;


        item.UpdateUI();

        item.UpdateButtonState();
    }


    // =========================================================
    // Heal Clone
    // =========================================================

    Item CreateHealClone()
    {
        if (healItem == null)
            return null;


        GameObject clone =
            Instantiate(
                healItem.gameObject,
                healItem.transform.parent
            );


        clone.name =
            healItem.gameObject.name +
            "_Clone";


        Item cloneItem =
            clone.GetComponent<Item>();


        if (cloneItem == null)
        {
            Destroy(
                clone
            );

            return null;
        }


        cloneItem.data =
            healItem.data;


        cloneItem.level =
            healItem.level;


        clone.SetActive(
            true
        );


        Button button =
            clone.GetComponent<Button>();


        if (button != null)
            button.interactable = true;


        cloneItem.UpdateUI();

        cloneItem.UpdateButtonState();


        return cloneItem;
    }


    // =========================================================
    // Clone 제거
    // =========================================================

    void ClearClones()
    {
        if (transform == null)
            return;


        for (
            int i =
            transform.childCount - 1;
            i >= 0;
            i--)
        {
            Transform child =
                transform.GetChild(i);


            Item item =
                child.GetComponent<Item>();


            if (item == null)
                continue;


            if (healItem != null &&
                item.gameObject ==
                healItem.gameObject)
            {
                continue;
            }


            if (item.name.Contains(
                "_Clone"))
            {
                Destroy(
                    item.gameObject
                );
            }
        }
    }


    // =========================================================
    // 최대 레벨
    // =========================================================

    bool IsMaxLevel(
        Item item)
    {
        if (item == null ||
            item.data == null)
        {
            return true;
        }


        return item.data.IsMaxLevel(
            item.level
        );
    }


    // =========================================================
    // 해금 여부
    // =========================================================

    bool IsItemUnlocked(
        Item item)
    {
        return
            unlockedItems.Contains(
                item
            );
    }


    // =========================================================
    // 해금
    // =========================================================

    public void UnlockItem(
        Item item)
    {
        if (item == null)
        {
            Debug.LogWarning(
                "해금할 Item이 없습니다."
            );

            return;
        }


        unlockedItems.Add(
            item
        );


        Debug.Log(
            "아이템 해금: " +
            (item.data != null
                ? item.data.itemName
                : item.name)
        );
    }


    // =========================================================
    // Boss1
    // =========================================================

    public void UnlockCouragePower()
    {
        UnlockItem(
            couragePowerItem
        );
    }


    // =========================================================
    // Boss2 Page1
    // =========================================================

    public void UnlockBloodHit()
    {
        UnlockItem(
            bloodHitItem
        );
    }


    // =========================================================
    // Boss2 Page2
    // =========================================================

    public void UnlockHopeOfHope()
    {
        UnlockItem(
            hopeOfHopeItem
        );
    }


    // =========================================================
    // 잠금
    // =========================================================

    void LockSpecialItem(
        Item item)
    {
        if (item != null)
        {
            item.gameObject.SetActive(
                false
            );
        }
    }


    // =========================================================
    // 랜덤
    // =========================================================

    void Shuffle(
        List<Item> list)
    {
        for (
            int i =
            list.Count - 1;
            i > 0;
            i--)
        {
            int randomIndex =
                Random.Range(
                    0,
                    i + 1
                );


            Item temp =
                list[i];


            list[i] =
                list[randomIndex];


            list[randomIndex] =
                temp;
        }
    }


    // =========================================================
    // 초기화
    // =========================================================

    public void ResetItemUnlocks()
    {
        if (items == null)
            return;


        ClearClones();

        unlockedItems.Clear();


        for (
            int i = 0;
            i < items.Length;
            i++)
        {
            Item item =
                items[i];


            if (item == null)
                continue;


            if (item == couragePowerItem ||
                item == bloodHitItem ||
                item == hopeOfHopeItem)
            {
                continue;
            }


            if (item == healItem)
            {
                unlockedItems.Add(
                    item
                );

                continue;
            }


            if (i <
                startingUnlockedItemCount)
            {
                unlockedItems.Add(
                    item
                );
            }
        }


        LockSpecialItem(
            couragePowerItem
        );


        LockSpecialItem(
            bloodHitItem
        );


        LockSpecialItem(
            hopeOfHopeItem
        );


        Item.ResetSpecialItemEffects();


        Debug.Log(
            "아이템 해금 상태 초기화 완료"
        );
    }


    // =========================================================
    // Sound
    // =========================================================

    void PlayLevelUpSound()
    {
        if (AudioManager.instance == null)
            return;


        AudioManager.instance.PlaySfx(
            AudioManager.Sfx.LevelUp
        );


        AudioManager.instance.EffectBgm(
            true
        );
    }
}