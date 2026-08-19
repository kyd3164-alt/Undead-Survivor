using UnityEngine;
using System.Collections;

public class BossUIController : MonoBehaviour
{
    public static BossUIController instance;

    [Header("# UI References")]
    public RectTransform noticeTextRect; // BossNoticeText의 RectTransform
    public GameObject bossHPBar;        // 경험치 밑에 배치한 보스 체력바 오브젝트
    public TMPro.TextMeshProUGUI bossNameText; // 보스 이름 텍스트

    [Header("# Animation Settings")]
    public float moveSpeed = 800f;       // 텍스트가 흘러가는 속도

    void Awake()
    {
        instance = this;
        // 게임 시작 시에는 숨겨둡니다.
        if (noticeTextRect != null) noticeTextRect.gameObject.SetActive(false);
        if (bossHPBar != null) bossHPBar.SetActive(false);
    }

    // 🚨 보스 등장 시 스포너에서 호출할 연출 코루틴
    public IEnumerator PlayBossAppearance(string bossName)
    {
        if (noticeTextRect == null) yield break;

        // 0. 보스 이름 먼저 세팅
        if (bossNameText != null) bossNameText.text = $"BOSS : {bossName}";

        // 1. 안내 텍스트를 화면 오른쪽 바깥(X: 1200)에 배치하고 활성화
        noticeTextRect.anchoredPosition = new Vector2(1200f, 0f);
        noticeTextRect.gameObject.SetActive(true);

        // 2. 텍스트가 화면 왼쪽 바깥(X: -1200)으로 완전히 지나갈 때까지 대기 (TimeScale이 0이어야 하므로 RealtimeDeltaTime 사용)
        while (noticeTextRect.anchoredPosition.x > -1200f)
        {
            float newX = noticeTextRect.anchoredPosition.x - (moveSpeed * Time.unscaledDeltaTime);
            noticeTextRect.anchoredPosition = new Vector2(newX, 0f);
            yield return null;
        }

        // 3. 연출 완료 후 경고 텍스트 끄고, 경험치 밑의 체력바 켜기
        noticeTextRect.gameObject.SetActive(false);
        if (bossHPBar != null) bossHPBar.SetActive(true);
    }
}
