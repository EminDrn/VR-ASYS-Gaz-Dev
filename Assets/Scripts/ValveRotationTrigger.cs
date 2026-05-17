using System;
using UnityEngine;

/// <summary>
/// pipe_handle objesinin kendi Transform rotasyonunu okur.
/// Z ekseni -150'ye ulaştığında hedef objeyi devre dışı bırakır ve tebrik sesi çalar.
/// </summary>
public class ValveRotationTrigger : MonoBehaviour
{
    public SceneManager sceneManager;
    public float triggerAngle = -150f; // bu değere ulaşınca tetikle
    public GameObject targetObject;   // kapatılacak obje
    public AudioSource audioSource;   // ses kaynağı
    public AudioClip congratsClip;    // tebrikler sesi
 
    private bool triggered = false;

    private void Start()
    {
        targetObject.SetActive(false);
    }

    private void Update()
    {
        float currentAngle = transform.localEulerAngles.z;
 
        if (currentAngle > 180)
            currentAngle -= 360;
 
        Debug.Log($"Valve Z Angle: {currentAngle:F1}");
 
        if (!triggered && currentAngle <= triggerAngle)
        {
            triggered = true;
 
            if (targetObject != null)
                targetObject.SetActive(false);
 
            if (audioSource != null && congratsClip != null)
                audioSource.PlayOneShot(congratsClip);
 
            Debug.Log("Vana -150'ye ulaştı! Tetiklendi.");
            sceneManager.EndScene();
        }
    }
}