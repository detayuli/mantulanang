using UnityEngine;
using System.Collections;

public class HamsterTurnManager : MonoBehaviour
{
    [Header("Player References")]
    public HamsterController player1;
    public HamsterController player2;

    [Header("Turn Settings")]
    public float minVelocityToSwitch = 0.1f; // kecepatan dianggap berhenti
    public float endTurnDelay = 1f;          // delay sebelum ganti turn
    public bool isGameOver = false;

    private int currentPlayer = 1;
    private bool isSwitchingTurn = false;
    public static HamsterTurnManager Instance { get; private set; }
    public int CurrentPlayer => currentPlayer;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ActivatePlayer(player1, true);
        ActivatePlayer(player2, false);

        Debug.Log("🎮 Game dimulai! Giliran Player 1");
    }

    private void Update()
    {
        if (isGameOver || isSwitchingTurn) return;

        CheckGameOver();

        // Cek giliran Player 1
        if (currentPlayer == 1 && player1 != null && player1.isLaunched)
        {
            if (player1.IsCompletelyStopped(minVelocityToSwitch))
            {
                StartCoroutine(SwitchTurn(2));
            }
        }
        // Cek giliran Player 2
        else if (currentPlayer == 2 && player2 != null && player2.isLaunched)
        {
            if (player2.IsCompletelyStopped(minVelocityToSwitch))
            {
                StartCoroutine(SwitchTurn(1));
            }
        }
    }

    private IEnumerator SwitchTurn(int nextPlayer)
    {
        isSwitchingTurn = true;
        Debug.Log($"⏳ Player {currentPlayer} selesai, ganti ke Player {nextPlayer}...");

        yield return new WaitForSeconds(endTurnDelay);

        currentPlayer = nextPlayer;

        ActivatePlayer(player1, currentPlayer == 1);
        ActivatePlayer(player2, currentPlayer == 2);

        Debug.Log($"🎯 Sekarang giliran Player {currentPlayer}");
        isSwitchingTurn = false;
    }

    private void ActivatePlayer(HamsterController player, bool active)
    {
        if (player == null) return;

        player.enabled = active;

        if (player.trajectory != null)
            player.trajectory.gameObject.SetActive(active);
    }

    private void CheckGameOver()
    {
        if (player1 == null || player2 == null) return;

        if (player1.hamsterHP <= 0 && !isGameOver)
        {
            Debug.Log("💀 Player 1 KALAH! Player 2 MENANG!");
            isGameOver = true;
            EndGame(2);
        }
        else if (player2.hamsterHP <= 0 && !isGameOver)
        {
            Debug.Log("💀 Player 2 KALAH! Player 1 MENANG!");
            isGameOver = true;
            EndGame(1);
        }
    }

    private void EndGame(int winner)
    {
        Debug.Log($"🏆 Permainan selesai! Player {winner} MENANG!");
        ActivatePlayer(player1, false);
        ActivatePlayer(player2, false);
    }
}
