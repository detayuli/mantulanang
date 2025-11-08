using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

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
    public TextMeshProUGUI turnIndicatorText;
    public float indicatorShowDuration = 2f;
    public float indicatorFadeDuration = 0.5f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeHearts();

        // Subscribe to events
        HamsterTurnManager.Instance.OnTurnChanged += HandleTurnChanged;
    }

    private void InitializeHearts()
    {
        // Initialize hearts for both players
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
        ShowTurnIndicator($"Player {newPlayerTurn}'s Turn");
    }

    public void ShowTurnIndicator(string text)
    {
        StartCoroutine(ShowTurnIndicatorSequence(text));
    }

    private IEnumerator ShowTurnIndicatorSequence(string text)
    {
        turnIndicatorText.text = text;
        turnIndicatorPanel.alpha = 0f;
        turnIndicatorPanel.gameObject.SetActive(true);

        // Fade in
        yield return DOTween.To(() => turnIndicatorPanel.alpha, x => turnIndicatorPanel.alpha = x, 1f, indicatorFadeDuration)
                            .WaitForCompletion();

        // Tahan tampil
        yield return new WaitForSeconds(indicatorShowDuration);

        // Fade out (biar transparan aja, jangan di-disable)
        yield return DOTween.To(() => turnIndicatorPanel.alpha, x => turnIndicatorPanel.alpha = x, 0f, indicatorFadeDuration)
                            .WaitForCompletion();

        // Pastikan tetap aktif
        turnIndicatorPanel.gameObject.SetActive(true);
    }


}