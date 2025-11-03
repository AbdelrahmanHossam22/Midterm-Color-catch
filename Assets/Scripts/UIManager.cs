using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text targetText;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;

    void Start()
    {
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.OnScoreChanged += UpdateScore;
            gm.OnTimeChanged += UpdateTimer;
            gm.OnTargetColorChanged += UpdateTargetColor;

            UpdateScore(gm.CurrentScore);
            UpdateTimer(gm.totalTime);
            UpdateTargetColor(gm.targetColor);
        }
        else
        {
            Debug.LogWarning("UIManager: GameManager.Instance is null");
            UpdateScore(0);
            UpdateTimer(0f);
            targetText.text = "";
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void OnDestroy()
    {
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.OnScoreChanged -= UpdateScore;
            gm.OnTimeChanged -= UpdateTimer;
            gm.OnTargetColorChanged -= UpdateTargetColor;
        }
    }

    void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            //  Display score even if negative
            scoreText.text = "Score: " + score.ToString();
        }
    }

    void UpdateTimer(float time)
    {
        if (timerText == null) return;
        int seconds = Mathf.CeilToInt(time);
        timerText.text = "Time: " + seconds;
    }

    void UpdateTargetColor(Collectible.ColorType color)
    {
        if (targetText == null) return;

        string colorName = color.ToString();
        targetText.text = "Target: " + colorName;

        switch (color)
        {
            case Collectible.ColorType.Red:
                targetText.color = Color.red;
                break;
            case Collectible.ColorType.Blue:
                targetText.color = Color.blue;
                break;
            case Collectible.ColorType.Green:
                targetText.color = Color.green;
                break;
            default:
                targetText.color = Color.white;
                break;
        }
    }

    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + finalScore;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("Quit Game triggered");
    }
}
