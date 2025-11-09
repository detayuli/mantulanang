using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("Team Hearts UI")]
    public Transform player1HeartsContainer;
    public Transform player2HeartsContainer;
    public GameObject heartPrefab;
    private Image[] player1Hearts;
    private Image[] player2Hearts;

    [Header("Turn Indicator")]
    public CanvasGroup turnIndicatorPanel;
    public Image turnPlayer1Image;
    public Image turnPlayer2Image;
    public float indicatorShowDuration = 2f;
    public float indicatorFadeDuration = 0.5f;

    [Header("Win Screen (tanpa panel)")]
    public Image player1WinImage;
    public Image player2WinImage;
    public float winFadeDuration = 1f;

    [Header("Countdown UI")]
    public CanvasGroup countdownPanel;
    public TextMeshProUGUI countdownText;
    public float countdownFade = 0.3f;
    public float countdownInterval = 1f;

    [Header("Restart Button")]
    public Button restartButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeHearts();

        // Subscribe event turn
        HamsterTurnManager.Instance.OnTurnChanged += HandleTurnChanged;

        // Pastikan UI awal bersih
        player1WinImage.gameObject.SetActive(false);
        player2WinImage.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
    }

    private void InitializeHearts()
    {
        int maxHearts = HamsterTurnManager.Instance.maxScore;
        player1Hearts = new Image[maxHearts];
        player2Hearts = new Image[maxHearts];

        for (int i = 0; i < maxHearts; i++)
        {
            player1Hearts[i] = Instantiate(heartPrefab, player1HeartsContainer).GetComponent<Image>();
            player2Hearts[i] = Instantiate(heartPrefab, player2HeartsContainer).GetComponent<Image>();
        }
    }

    public void UpdatePlayerHearts(int playerID, int remainingHearts)
    {
        Image[] hearts = (playerID == 1) ? player1Hearts : player2Hearts;

        for (int i = 0; i < hearts.Length; i++)
        {
            Color c = hearts[i].color;
            c.a = (i < remainingHearts) ? 1f : 0.2f;
            hearts[i].color = c;
        }
    }

    private void HandleTurnChanged(int newPlayerTurn)
    {
        ShowTurnIndicator(newPlayerTurn);
    }

    public void ShowTurnIndicator(int playerTurn)
    {
        StartCoroutine(ShowTurnIndicatorSequence(playerTurn));
    }

    private IEnumerator ShowTurnIndicatorSequence(int playerTurn)
    {
        turnIndicatorPanel.alpha = 0f;
        turnIndicatorPanel.gameObject.SetActive(true);

        turnPlayer1Image.gameObject.SetActive(playerTurn == 1);
        turnPlayer2Image.gameObject.SetActive(playerTurn == 2);

        yield return DOTween.To(() => turnIndicatorPanel.alpha, x => turnIndicatorPanel.alpha = x, 1f, indicatorFadeDuration)
                            .WaitForCompletion();

        yield return new WaitForSeconds(indicatorShowDuration);

        yield return DOTween.To(() => turnIndicatorPanel.alpha, x => turnIndicatorPanel.alpha = x, 0f, indicatorFadeDuration)
                            .WaitForCompletion();
    }

    public void ShowWinScreen(int winnerPlayer)
    {
        StartCoroutine(ShowWinImageSequence(winnerPlayer));
    }

    private IEnumerator ShowWinImageSequence(int winnerPlayer)
    {
        // Aktifkan gambar sesuai pemenang
        if (winnerPlayer == 1)
        {
            player1WinImage.gameObject.SetActive(true);
            player1WinImage.color = new Color(1, 1, 1, 0);
            player1WinImage.DOFade(1f, winFadeDuration).SetUpdate(true);
        }
        else
        {
            player2WinImage.gameObject.SetActive(true);
            player2WinImage.color = new Color(1, 1, 1, 0);
            player2WinImage.DOFade(1f, winFadeDuration).SetUpdate(true);
        }

        // Tampilkan tombol restart
        yield return new WaitForSecondsRealtime(winFadeDuration);
        restartButton.gameObject.SetActive(true);

    }

    public IEnumerator ShowCountdown()
    {
        countdownPanel.alpha = 1f;
        countdownPanel.gameObject.SetActive(true);

        string[] steps = { "3", "2", "1", "START!" };

        foreach (var step in steps)
        {
            countdownText.text = step;
            countdownText.transform.localScale = Vector3.one;
            countdownText.transform.DOScale(2f, countdownFade).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(countdownInterval);

            if (step != "START!")
                countdownText.DOFade(0f, 1f).From(2f);
        }

        yield return new WaitForSeconds(0.2f);
        countdownPanel.DOFade(0f, 0.2f);
        yield return new WaitForSeconds(0.2f);
        countdownPanel.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
