using UnityEngine;
using System.Collections.Generic;

public class HamsterTurnManager : MonoBehaviour
{
    public static HamsterTurnManager Instance;

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
    private int currentHamsterIndex = 0;
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

        // Aktifkan giliran awal
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

    private System.Collections.IEnumerator SwitchTurn(int nextPlayer)
    {
        switchingTurn = true;
        Debug.Log($"⏳ Player {currentPlayer} selesai, ganti ke Player {nextPlayer}...");

        yield return new WaitForSeconds(endTurnDelay);

        currentPlayer = nextPlayer;

        ActivatePlayerHamsters(1, currentPlayer == 1);
        ActivatePlayerHamsters(2, currentPlayer == 2);

        Debug.Log($"🎯 Sekarang giliran Player {currentPlayer}");
        switchingTurn = false;
    }

    private void ActivatePlayerHamsters(int playerID, bool isActive)
    {
        var list = (playerID == 1) ? player1Hamsters : player2Hamsters;

        for (int i = 0; i < list.Count; i++)
        {
            // hanya hamster aktif (sesuai index giliran) yang bisa dikontrol
            list[i].canControl = (i == currentHamsterIndex && isActive);
        }
    }

    public HamsterController GetActiveHamster()
    {
        return currentPlayer == 1
            ? player1Hamsters[currentHamsterIndex]
            : player2Hamsters[currentHamsterIndex];
    }

    public void OnHamsterDeath(HamsterController hamster)
    {
        if (hamster == null) return;
        if (hamster.isDead == false) return;

        // Tambah skor lawan
        if (hamster.playerID == 1) player2Score++;
        else player1Score++;

        Debug.Log($"⚔️ Player {hamster.playerID} hamster mati! Skor: P1={player1Score} P2={player2Score}");

        if (player1Score >= maxScore || player2Score >= maxScore)
        {
            EndGame();
            return;
        }

        Transform spawn = (hamster.playerID == 1) ? player1Spawn : player2Spawn;

        // Respawn di posisi awal
        hamster.transform.position = spawn.position;
        hamster.transform.rotation = Quaternion.identity;

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
