using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HamsterPhysicsReflector : MonoBehaviour
{
    [Header("Physics Settings")]
    [Range(0f, 1f)] public float restitution = 0.8f; // e → elastisitas pantulan
    [Range(0f, 1f)] public float friction = 0.1f;    // μ → gesekan tangensial
    public float radius = 0.5f;                      // radius lingkaran (harus cocok dengan collider)
    public Vector2 arenaMin = new(-8f, -4.5f);       // batas bawah-kiri arena
    public Vector2 arenaMax = new(8f, 4.5f);         // batas atas-kanan arena

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 pos = rb.position;
        Vector2 vel = rb.velocity;

        // Simpan kecepatan awal untuk cek perubahan
        bool collided = false;
        Vector2 normal = Vector2.zero;

        // Cek dinding kiri
        if (pos.x - radius <= arenaMin.x)
        {
            pos.x = arenaMin.x + radius;
            normal = Vector2.right;
            collided = true;
        }
        // Cek dinding kanan
        else if (pos.x + radius >= arenaMax.x)
        {
            pos.x = arenaMax.x - radius;
            normal = Vector2.left;
            collided = true;
        }
        // Cek lantai (bawah)
        else if (pos.y - radius <= arenaMin.y)
        {
            pos.y = arenaMin.y + radius;
            normal = Vector2.up;
            collided = true;
        }
        // Cek langit-langit (atas)
        else if (pos.y + radius >= arenaMax.y)
        {
            pos.y = arenaMax.y - radius;
            normal = Vector2.down;
            collided = true;
        }

        if (collided)
        {
            rb.position = pos;

            // Rumus refleksi umum: v' = v − (1 + e)(v · n)n
            Vector2 v = vel;
            Vector2 vReflected = v - (1 + restitution) * Vector2.Dot(v, normal) * normal;

            // Terapkan gesekan (mengurangi kecepatan tangensial)
            Vector2 tangent = v - Vector2.Dot(v, normal) * normal;
            vReflected -= tangent * friction;

            rb.velocity = vReflected;

            Debug.DrawRay(pos, normal * 0.5f, Color.yellow, 0.3f);
        }
    }

    public static Vector2 ComputeBounce(Vector2 velocity, Vector2 normal, float restitution, float friction)
    {
        // Normalisasi normal
        normal.Normalize();

        // Komponen kecepatan normal & tangensial
        Vector2 vNormal = Vector2.Dot(velocity, normal) * normal;
        Vector2 vTangent = velocity - vNormal;

        // Balik arah normal dengan faktor elastisitas (restitusi)
        Vector2 vReflectedNormal = -restitution * vNormal;

        // Kurangi kecepatan tangensial dengan faktor gesekan
        Vector2 vReflectedTangent = vTangent * (1f - friction);

        return vReflectedNormal + vReflectedTangent;
    }
}
