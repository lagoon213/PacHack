using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Image[] digits; // 6 digits
    public Sprite[] numberSprites; // 0-9 sprites
    public bool displayHighScore = false; // if true, show high score instead of current score

    void Update()
    {
        int value = displayHighScore ? ScoreManager.Instance.highScore : ScoreManager.Instance.score;
        UpdateScore(value);
    }

    void UpdateScore(int score)
    {
        string scoreStr = score.ToString().PadLeft(digits.Length, '0');

        for (int i = 0; i < digits.Length; i++)
        {
            int digit = scoreStr[i] - '0';
            digits[i].sprite = numberSprites[digit];
        }
    }
}
