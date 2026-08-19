using System;
using System.Collections;
using UnityEngine;

public class AchiveManager : MonoBehaviour
{
    public GameObject[] lockCharacter;
    public GameObject[] unlockCharacter;
    public GameObject uiNotice;

    enum Achive { UnlockPotato, UnlockBean }
    Achive[] achives;
    WaitForSecondsRealtime wait;

    void Awake()
    {
        achives = (Achive[])Enum.GetValues(typeof(Achive));
        wait = new WaitForSecondsRealtime(5); 
        if (!PlayerPrefs.HasKey("MyData"))
        {
            Init();
        }
    }

    void Init()
    {
        PlayerPrefs.SetInt("MyData", 1);

        foreach (Achive achive in achives)
        {
            PlayerPrefs.SetInt(achive.ToString(), 0);
        }
    }

    void Start()
    {
        UnlockCharacter();
    }

    void UnlockCharacter()
    {
        // lockCharacter 배열 크기만큼 반복하되, achives 배열 크기를 초과하지 않도록 안전장치 추가
        for (int index = 0; index < lockCharacter.Length; index++)
        {
            // 🚨 혹시나 인스펙터 배열 크기가 업적 종류(2개)보다 크게 설정되었을 때 에러 방지
            if (index >= achives.Length) break;

            // 🚨 인스펙터 창에서 캐릭터 오브젝트를 깜빡하고 드래그 앤 드롭 안 했을 때 에러 방지
            if (lockCharacter[index] == null || unlockCharacter[index] == null) continue;

            string achiveName = achives[index].ToString();
            bool isUnlock = PlayerPrefs.GetInt(achiveName) == 1;

            lockCharacter[index].SetActive(!isUnlock);
            unlockCharacter[index].SetActive(isUnlock);
        }
    }


    // Update is called once per frame
    // Update is called once per frame
    void LateUpdate()
    {
        // 🚨 [수정] GameManager가 없거나(Null), 아직 게임이 시작되지 않았다면 검사를 건너뜁니다.
        if (GameManager.instance == null)
            return;

        foreach (Achive achive in achives)
        {
            CheckAckive(achive);
        }
    }



    void CheckAckive(Achive achive)
    {
        bool isAchive = false;


        switch (achive)
        {
            case Achive.UnlockPotato:
                isAchive = GameManager.instance.kill >= 10;
                break;
            case Achive.UnlockBean:
                isAchive = GameManager.instance.gameTime == GameManager.instance.maxGameTime;
                break;
        }

        if (isAchive && PlayerPrefs.GetInt(achive.ToString()) == 0)
        {
            PlayerPrefs.SetInt(achive.ToString(), 1);

            for (int index = 0; index < uiNotice.transform.childCount; index++)
            {
                bool isActive = index == (int)achive;
                uiNotice.transform.GetChild(index).gameObject.SetActive(isActive);
            }

            StartCoroutine(NoticeRoutine());
        }
    }

    IEnumerator NoticeRoutine()
    {
        uiNotice.SetActive(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);

        yield return wait;

        uiNotice.SetActive(false);
    }
}
