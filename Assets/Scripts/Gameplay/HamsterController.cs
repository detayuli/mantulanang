using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HamsterController : MonoBehaviour
{
    [Header("References")]
    public HamsterTrajectory trajectory;

    [Header("Stats")]
    public float hamsterDamage = 5f, hamsterHP = 40f, allyDamage = 3f;

    [Header("Movement")]
    public float initialSpeed = 7f;
    [Range(0.1f, 1f)] public float minDragPercent = 0.1f;
    public float maxDragDistance = 3f;

    [Header("Aim Zone")]
    public float cancelRadius = 0.3f;
    public Color aimZoneColor = new(1f, 1f, 1f, 0.2f);

    [Header("Collision Reduction")]
    public float hamsterCollideReduction = 0.5f, borderCollideReduction = 0.8f, obstacleCollideReduction = 0.95f;

    [Header("Speed Decay")]
    public float speedDecayRate = 0.1f;

    [Header("Stop Threshold")]
    public float stopThreshold = 0.05f;

    public Rigidbody2D rb;
    private Vector2 dragStartPos;
    public bool isDragging, isLaunched;
    private float currentSpeed;
    private SpriteRenderer aimZoneRenderer;
    public int playerID; // 1 = Player1, 2 = Player2

    [HideInInspector] public bool canControl = true;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.drag = 0;
        rb.angularDrag = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.StartAsleep;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Buat lingkaran area cancel
        var aimZone = new GameObject("AimZoneVisual");
        aimZone.transform.SetParent(transform);
        aimZone.transform.localPosition = Vector3.zero;
        aimZoneRenderer = aimZone.AddComponent<SpriteRenderer>();
        aimZoneRenderer.sprite = CreateCircleSprite();
        aimZoneRenderer.color = aimZoneColor;
        aimZoneRenderer.sortingOrder = -1;
        aimZone.transform.localScale = Vector3.one * cancelRadius * 2f;
        aimZone.SetActive(false);
    }

    private void Update()
    {
        if (canControl && !isLaunched) HandleInput();

        HandleSpeedDecay();
        HardStopIfTooSlow(stopThreshold);
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            aimZoneRenderer.gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 dragPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragVector = dragPos - dragStartPos;
            float dragDist = Mathf.Clamp(dragVector.magnitude, 0f, maxDragDistance);
            Vector2 launchDir = -dragVector.normalized;

            trajectory?.ShowTrajectory(transform.position, launchDir, dragDist);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector2 dragEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragVector = dragEnd - dragStartPos;
            float dragDist = dragVector.magnitude;

            if (dragDist < cancelRadius)
            {
                trajectory?.HideTrajectory();
                ResetDrag();
                return;
            }

            float dragPercent = Mathf.Max(Mathf.Clamp01(dragDist / maxDragDistance), minDragPercent);
            currentSpeed = initialSpeed * dragPercent;

            rb.velocity = -dragVector.normalized * currentSpeed;
            trajectory?.HideTrajectory();

            isLaunched = true;
            ResetDrag();
        }
    }

    private void ResetDrag()
    {
        isDragging = false;
        aimZoneRenderer.gameObject.SetActive(false);
    }

    private void HandleSpeedDecay()
    {
        if (!isLaunched) return;

        currentSpeed = Mathf.Max(0f, currentSpeed - speedDecayRate * Time.deltaTime * 10f);

        // Sedikit drag tambahan agar melambat halus
        rb.drag = Mathf.Lerp(0f, 2f, 1f - (rb.velocity.magnitude / 10f));

        if (currentSpeed < 0.3f)
            currentSpeed = 0f;

        rb.velocity = rb.velocity.normalized * currentSpeed;
    }

    private void HardStopIfTooSlow(float threshold)
    {
        if (rb.velocity.magnitude < threshold && rb.angularVelocity < threshold * 10f)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.Sleep();
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.contacts.Length == 0) return;
        var contact = col.contacts[0];
        Vector2 normal = contact.normal;
        Vector2 vBefore = rb.velocity;
        Vector2 reflect = Vector2.Reflect(vBefore, normal).normalized;
        float newSpeed = vBefore.magnitude;

        switch (col.collider.tag)
        {
            case "Border":
                newSpeed *= borderCollideReduction;
                rb.velocity = reflect * newSpeed + normal * 0.25f;
                transform.position += (Vector3)(normal * 0.02f);
                break;

            case "Hamster":
                newSpeed *= hamsterCollideReduction;
                rb.velocity = reflect * newSpeed;

                // Ambil hamster lain yang ditabrak
                HamsterController otherHamster = col.collider.GetComponent<HamsterController>();
                if (otherHamster != null)
                {
                    // Hanya musuh yang kena damage
                    if (HamsterTurnManager.Instance != null && 
                        HamsterTurnManager.Instance.CurrentPlayer == playerID)
                    {
                        otherHamster.TakeDamage(hamsterDamage);
                        Debug.Log($"💥 Player {playerID} menyerang Player {otherHamster.playerID} (-{hamsterDamage} HP)");
                    }

                    // Dorong sedikit hamster lawan
                    Rigidbody2D otherRb = col.collider.attachedRigidbody;
                    if (otherRb != null)
                    {
                        Vector2 opposite = -normal * (newSpeed * 0.4f);
                        otherRb.velocity += opposite;
                    }
                }
                break;

            case "Obstacle":
                newSpeed *= obstacleCollideReduction;
                rb.velocity = reflect * newSpeed;
                break;
        }

        if (rb.velocity.magnitude < 0.05f)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Debug.DrawRay(contact.point, rb.velocity.normalized * 1.5f, Color.cyan, 0.3f);
    }


    private Sprite CreateCircleSprite()
    {
        Texture2D tex = new(128, 128);
        float r = 64f;
        Vector2 c = new(r, r);

        for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float d = Vector2.Distance(new(x, y), c);
                tex.SetPixel(x, y, (d > r - 2f && d < r + 2f) ? Color.white : new Color(1, 1, 1, 0));
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
    }

    public bool IsTurnFinished()
    {
        if (!isLaunched) return false;
        if (rb.velocity.magnitude == 0f)
        {
            rb.velocity = Vector2.zero;
            isLaunched = false;
            return true;
        }
        return false;
    }

    public bool IsCompletelyStopped(float minVelocity)
    {
        if (isLaunched && rb.velocity.magnitude <= minVelocity)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            isLaunched = false;
            return true;
        }
        return false;
    }

    private void SpawnBounceEffect(Vector2 pos)
    {
        Debug.DrawRay(pos, Vector2.up * 0.5f, Color.yellow, 0.3f);
    }

    public void TakeDamage(float amount)
    {
        hamsterHP -= amount;
        if (hamsterHP <= 0)
        {
            hamsterHP = 0;
            Debug.Log($"💀 Player {playerID} mati!");
            Destroy(gameObject);
        }
    }

}
