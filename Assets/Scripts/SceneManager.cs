using Unity.AI.Navigation;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public GameObject gasParticle;
    public static bool isGasStart;
    public static bool isGasEnd;
    public GameObject valve;
    public AudioSource gasAudio;
    public GameObject nextSceneUI;
    public GameObject teleportingArea;

    private void Start()
    {
        valve.GetComponent<BoxCollider>().enabled = false;
        isGasEnd = false;
        gasParticle.SetActive(false);
        nextSceneUI.SetActive(false);
        teleportingArea.GetComponent<NavMeshSurface>().enabled = false;
    }

    public void StartScene()
    {
        gasParticle.SetActive(true);
        isGasStart = true;
        valve.GetComponent<BoxCollider>().enabled = true;
        gasAudio.Play();
    }

    public void EndScene()
    {
        nextSceneUI.SetActive(true);
        gasParticle.SetActive(false);
        isGasEnd = true;
        isGasStart = false;
        gasAudio.Stop();
    }

    // Sonraki sahneye geç
    public void NextScene()
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex + 1);
    }

    // Sahneyi yeniden başlat
    public void RestartScene()
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
    }

    // Uygulamadan çık
    public void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Editor'da çalışırken durdurur
#else
        Application.Quit(); // Build alınmış uygulamada kapatır
#endif
    }
}