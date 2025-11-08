using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HamsterStatusUI : MonoBehaviour
{
    [Header("UI References")]
    public Image healthIcon;      // ikon hati
    public TextMeshProUGUI healthText; 
    public Image damageIcon;      // ikon pedang
    public TextMeshProUGUI damageText; 

    private HamsterController hamster;

    public void Initialize(HamsterController targetHamster)
    {
        hamster = targetHamster;
        UpdateUI();
    }

    private void Update()
    {
        if (hamster == null) return;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update angka Health dan Damage
        healthText.text = $"{Mathf.CeilToInt(hamster.hamsterHP)}";
        damageText.text = $"{Mathf.CeilToInt(hamster.hamsterDamage)}";

        // Sembunyikan UI kalau hamster mati
        gameObject.SetActive(!hamster.isDead);
    }
}
