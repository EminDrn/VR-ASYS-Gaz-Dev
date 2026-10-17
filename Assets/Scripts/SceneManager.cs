using Unity.AI.Navigation;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public GameObject gasParticle;
    public static bool isGasStart;
    public static bool isGasEnd;
    public GameObject valve;
    public AudioSource gasAudio;
    public GameObject nextSceneUI;
    public CalculateExamSceneScore calculateExamSceneScore;

    private void Start()
    {
        valve.GetComponent<BoxCollider>().enabled = false;
        isGasEnd = false;
        gasParticle.SetActive(false);
        nextSceneUI.SetActive(false);
        
    }

    public void StartScene()
    {
        gasParticle.SetActive(true);
        isGasStart = true;
        valve.GetComponent<BoxCollider>().enabled = true;
        gasAudio.Play();
        calculateExamSceneScore.StartScoreCountdown();
    }

    public void EndScene()
    {
        calculateExamSceneScore.StopScoreCountdown(); // Gaz kapandı, süre cezası durur
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