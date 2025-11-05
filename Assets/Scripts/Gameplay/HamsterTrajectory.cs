using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HamsterTrajectory : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Trajectory Settings")]
    public int segmentCount = 30; // jumlah titik garis (semakin banyak semakin halus)
    public float maxLength = 3f;  // panjang maksimal garis
    public float curvature = 0.2f; // seberapa melengkung garisnya
    public float lineWidthStart = 0.15f;
    public float lineWidthEnd = 0.02f;
    public Gradient lineColor;

private void Awake()
{
    lineRenderer = GetComponent<LineRenderer>();
    lineRenderer.positionCount = 0;

    // Pengaturan umum
    lineRenderer.sortingOrder = 5;
    lineRenderer.textureMode = LineTextureMode.Stretch;
    lineRenderer.alignment = LineAlignment.View;
    lineRenderer.numCornerVertices = 5;
    lineRenderer.numCapVertices = 5;

    // Lebar garis
    lineRenderer.widthCurve = AnimationCurve.Linear(0, lineWidthStart, 1, lineWidthEnd);

    // Warna gradasi (fallback)
    if (lineColor == null)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.cyan, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        lineRenderer.colorGradient = g;
    }
    else
    {
        lineRenderer.colorGradient = lineColor;
    }
}

    /// <summary>
    /// Menampilkan trajectory statik seperti panah (tanpa efek gravitasi)
    /// </summary>
    public void ShowTrajectory(Vector2 startPos, Vector2 direction, float strength)
    {
        Vector3[] points = new Vector3[segmentCount];

        // Normalisasi & batasi panjang garis
        Vector2 dir = direction.normalized;
        float lineLength = Mathf.Clamp(strength, 0f, maxLength);

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 pos = startPos + dir * lineLength * t;

            // Tambahkan sedikit lengkungan agar mirip panah dinamis
            pos.y += Mathf.Sin(t * Mathf.PI) * curvature * lineLength * 0.3f;

            points[i] = pos;
        }

        lineRenderer.positionCount = segmentCount;
        lineRenderer.SetPositions(points);
    }

    public void HideTrajectory()
    {
        lineRenderer.positionCount = 0;
    }
}
