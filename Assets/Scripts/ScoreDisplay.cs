using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Image[] digits; // Assign 6 Image components in inspector, left-to-right or right-to-left
    public Sprite[] numberSprites; // 0-9 sprites

    void Update()
    {
        UpdateScore(ScoreManager.Instance.score);
    }

    void UpdateScore(int score)
    {
        string scoreStr = score.ToString().PadLeft(digits.Length, '0'); // ensures 6 digits

        for (int i = 0; i < digits.Length; i++)
        {
            int digit = scoreStr[i] - '0'; // convert char '0'-'9' to int
            digits[i].sprite = numberSprites[digit];
        }
    }
}
