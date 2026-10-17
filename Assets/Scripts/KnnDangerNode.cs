using UnityEngine;

/// <summary>
/// Bu objeyi KnnDangerZoneSystem'de bir tehlike noktası (düğüm) yapar.
/// Obje aktif olduğunda bölgeye otomatik eklenir, kapandığında veya yok edildiğinde çıkarılır.
/// Gaz senaryosunda gaz particle'ına (ve bulut boyunca dizilen alt objelere) eklenir:
/// gaz başlayınca (obje aktif olunca) düğüm olur, vana kapanınca (obje kapanınca) çıkar.
/// </summary>
public class KnnDangerNode : MonoBehaviour
{
    [Tooltip("Eklenecek bölgenin adı (KnnDangerZoneSystem'deki bölge adıyla birebir aynı olmalı).")]
    public string zoneName = "Gaz";

    [Tooltip("Obje bu ölçeğin altındayken düğüm sayılmaz (yangında büyüyen ateşler için). 0 = her zaman sayılır.")]
    [Min(0f)] public float minScale = 0f;

    bool isRegistered;
    KnnDangerZoneSystem registeredSystem;

    /// <summary>Objede KnnDangerNode yoksa ekler. Varsa dokunmaz.</summary>
    public static KnnDangerNode Ensure(GameObject go, string zoneName = "Gaz")
    {
        KnnDangerNode node = go.GetComponent<KnnDangerNode>();
        if (node == null)
        {
            node = go.AddComponent<KnnDangerNode>();
            node.zoneName = zoneName;
        }
        return node;
    }

    void Update()
    {
        // Sistem sonradan oluşabilir, obje büyüyüp küçülebilir: her karede durumu eşitle
        bool shouldRegister = KnnDangerZoneSystem.Instance != null && transform.localScale.x >= minScale;
        if (shouldRegister && !isRegistered) Register();
        else if (!shouldRegister && isRegistered) Unregister();
    }

    void OnDisable()
    {
        Unregister();
    }

    void Register()
    {
        registeredSystem = KnnDangerZoneSystem.Instance;
        registeredSystem.AddPoint(zoneName, transform);
        isRegistered = true;
    }

    void Unregister()
    {
        if (!isRegistered) return;
        if (registeredSystem != null) registeredSystem.RemovePoint(zoneName, transform);
        registeredSystem = null;
        isRegistered = false;
    }
}
