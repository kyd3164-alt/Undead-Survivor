using UnityEngine;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        // UI 자식으로 붙어있는 Item 컴포넌트들을 자동으로 다 긁어옵니다.
        items = GetComponentsInChildren<Item>(true);
    }

    // 🎯 일반 경험치 레벨업용 함수 (딱 1번 고르고 닫힘)
    public void Show()
    {
        Next(); // 아이템 3개 새로고침

        rect.localScale = Vector3.one; // UI 창 켜기
        GameManager.instance.Stop();   // 게임 일시정지

        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    // 닫기 함수
    public void Hide()
    {
        rect.localScale = Vector3.zero; // UI 창 끄기
        GameManager.instance.Resume();  // 게임 다시 시작

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
    }

    // UI 버튼에서 인덱스 번호를 넣어 호출하는 함수
    public void Select(int index)
    {
        items[index].OnClick();
    }

    // 무작위로 중복 없이 아이템 3개를 뽑아 화면에 띄우는 핵심 로직
    void Next()
    {
        // 1. 일단 모든 아이템 슬롯을 숨깁니다.
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        int[] ran = new int[3];

        // 2. 중복되지 않는 랜덤 인덱스 3개 뽑기
        while (true)
        {
            ran[0] = Random.Range(0, items.Length);
            ran[1] = Random.Range(0, items.Length);
            ran[2] = Random.Range(0, items.Length);

            // 세 숫자가 모두 다르면 무한루프 탈출
            if (ran[0] != ran[1] && ran[1] != ran[2] && ran[0] != ran[2])
                break;
        }

        // 3. 뽑힌 아이템 배치 및 만렙 예외 처리
        for (int index = 0; index < ran.Length; index++)
        {
            Item ranItem = items[ran[index]];

            // 만약 무작위로 뽑힌 아이템이 이미 만렙이라면?
            if (ranItem.level == ranItem.data.damages.Length)
            {
                items[4].gameObject.SetActive(true); // 만렙 대체 아이템(보통 골드/체력) 활성화
            }
            else
            {
                ranItem.gameObject.SetActive(true); // 일반 아이템 활성화
            }
        }
    }
}
