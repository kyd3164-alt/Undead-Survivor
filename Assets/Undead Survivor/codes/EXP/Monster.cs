using UnityEngine;

public class Monster : MonoBehaviour
{
    public GameObject expOrbPrefab;
    public int rewardExp = 10;

    public void Die()
    {
        DropExperience();
        Destroy(gameObject);
    }

    private void DropExperience()
    {
        GameObject orb = Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
        ExpOrb expOrbScript = orb.GetComponent<ExpOrb>();
        if (expOrbScript != null)
        {
            expOrbScript.Init(rewardExp);
        }
    }
}
