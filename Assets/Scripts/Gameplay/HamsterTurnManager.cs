using UnityEngine;
using System.Collections.Generic;

public class HamsterTurnManager : MonoBehaviour
{
    public static HamsterTurnManager Instance;
    
    // Events for UI
    public delegate void TurnChangedHandler(int newPlayerTurn);
    public event TurnChangedHandler OnTurnChanged;
    
    public delegate void ScoreChangedHandler(int player1Score, int player2Score);
    public event ScoreChangedHandler OnScoreChanged;

    [Header("Players")]
    public List<HamsterController> player1Hamsters;
    public List<HamsterController> player2Hamsters;

    [Header("Spawn Points")]
    public Transform player1Spawn;
    public Transform player2Spawn;

    [Header("Game Settings")]
    public int maxScore = 4;
    public float endTurnDelay = 1f;
    public float minVelocityToSwitch = 0.1f;

    private int player1Score = 0;
    private int player2Score = 0;
    private int currentPlayer = 1;
    // Keep per-player selected hamster indices so each player can choose independently
    private int currentHamsterIndexP1 = 0;
    private int currentHamsterIndexP2 = 0;
    private bool switchingTurn = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Pastikan semua hamster tahu playerID-nya
        foreach (var h in player1Hamsters)
            h.playerID = 1;

        foreach (var h in player2Hamsters)
            h.playerID = 2;

        // Aktifkan giliran awal (set hanya hamster yang dipilih untuk bisa dikontrol)
        ActivatePlayerHamsters(1, true);
        ActivatePlayerHamsters(2, false);
    }

    private void Update()
    {
        if (switchingTurn) return;

        HamsterController activeHamster = GetActiveHamster();
        if (activeHamster != null && activeHamster.IsCompletelyStopped(minVelocityToSwitch))
        {
            StartCoroutine(SwitchTurn(currentPlayer == 1 ? 2 : 1));
        }
    }

    // Allow external selection of a hamster (e.g., by clicking it).
    public void SelectHamster(HamsterController hamster)
    {
        if (hamster == null) return;
        if (hamster.isDead)
        {
            Debug.Log("Cannot select a dead hamster.");
            return;
        }
        if (hamster.playerID != currentPlayer)
        {
            Debug.Log("Cannot select a hamster that is not on the current player's side.");
            return;
        }

        var list = (currentPlayer == 1) ? player1Hamsters : player2Hamsters;
        int idx = list.IndexOf(hamster);
        if (idx < 0) return;

        if (currentPlayer == 1) currentHamsterIndexP1 = idx;
        else currentHamsterIndexP2 = idx;

        // Refresh control flags
        ActivatePlayerHamsters(1, currentPlayer == 1);
        ActivatePlayerHamsters(2, currentPlayer == 2);

        Debug.Log($"Selected hamster {hamster.name} (index {idx}) for Player {currentPlayer}");
    }

    private System.Collections.IEnumerator SwitchTurn(int nextPlayer)
    {
        switchingTurn = true;
        Debug.Log($"⏳ Player {currentPlayer} selesai, ganti ke Player {nextPlayer}...");

        // Tunggu UI turn indicator selesai
        GameUIManager.Instance.ShowTurnIndicator($"Player {nextPlayer}'s Turn");
        yield return new WaitForSeconds(endTurnDelay);

        currentPlayer = nextPlayer;

        ActivatePlayerHamsters(1, currentPlayer == 1);
        ActivatePlayerHamsters(2, currentPlayer == 2);

        OnTurnChanged?.Invoke(currentPlayer);
        Debug.Log($"🎯 Sekarang giliran Player {currentPlayer}");
        switchingTurn = false;
    }

    private void ActivatePlayerHamsters(int playerID, bool isActive)
    {
        var list = (playerID == 1) ? player1Hamsters : player2Hamsters;
        int selectedIndex = (playerID == 1) ? currentHamsterIndexP1 : currentHamsterIndexP2;

        for (int i = 0; i < list.Count; i++)
        {
            // hanya hamster yang dipilih saja yang dapat dikontrol saat giliran aktif
            bool shouldControl = isActive && (i == selectedIndex);
            list[i].canControl = shouldControl;
        }
    }

    public HamsterController GetActiveHamster()
    {
        try
        {
            if (currentPlayer == 1)
            {
                if (player1Hamsters == null || player1Hamsters.Count == 0) return null;
                int idx = Mathf.Clamp(currentHamsterIndexP1, 0, player1Hamsters.Count - 1);
                return player1Hamsters[idx];
            }
            else
            {
                if (player2Hamsters == null || player2Hamsters.Count == 0) return null;
                int idx = Mathf.Clamp(currentHamsterIndexP2, 0, player2Hamsters.Count - 1);
                return player2Hamsters[idx];
            }
        }
        catch
        {
            return null;
        }
    }

    public void OnHamsterDeath(HamsterController hamster)
    {
        if (hamster == null) return;
        if (hamster.isDead == false) return;

        // Tambah skor lawan
        if (hamster.playerID == 1) player2Score++;
        else player1Score++;

        Debug.Log($"⚔️ Player {hamster.playerID} hamster mati! Skor: P1={player1Score} P2={player2Score}");

        // Update UI hearts
        OnScoreChanged?.Invoke(player1Score, player2Score);
        GameUIManager.Instance?.UpdatePlayerHearts(1, maxScore - player1Score);
        GameUIManager.Instance?.UpdatePlayerHearts(2, maxScore - player2Score);

        if (player1Score >= maxScore || player2Score >= maxScore)
        {
            EndGame();
            return;
        }

        Transform spawn = (hamster.playerID == 1) ? player1Spawn : player2Spawn;

        // Respawn di posisi awal
        hamster.transform.position = spawn.position;
        hamster.transform.rotation = Quaternion.identity;
        audiomanager.Instance.PlaySFX(audiomanager.Instance.HamsterRespawn);

        // Reset fisika
        hamster.rb.velocity = Vector2.zero;
        hamster.rb.angularVelocity = 0f;
        hamster.rb.Sleep();

        // Revive hamster
        hamster.hamsterHP = hamster.maxHP; // ✅ HP balik penuh
        hamster.isDead = false;
        hamster.isLaunched = false;
        hamster.canControl = false;

        Debug.Log($"💫 {hamster.name} dihidupkan kembali dengan HP {hamster.hamsterHP}/{hamster.maxHP}");
    }




    private void EndGame()
    {
        Debug.Log("🏆 Game Over!");
        foreach (var h in player1Hamsters) h.canControl = false;
        foreach (var h in player2Hamsters) h.canControl = false;
        Time.timeScale = 0f;
    }

    public int CurrentPlayer => currentPlayer;
}
