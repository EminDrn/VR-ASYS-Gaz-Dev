
using UnityEngine;
using UnityEditor;

public class SceneManager : MonoBehaviour
{
    public GameObject gasParticle;
    public static bool isGasStart;
    public static bool isGasEnd;
    public GameObject valve;
    public AudioSource gasAudio;
    public GameObject nextSceneUI;
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
    }

    public void EndScene()
    {
        nextSceneUI.SetActive(true);
        gasParticle.SetActive(false);
        isGasEnd = true;
        isGasStart = false;
        gasAudio.Stop();
    }
 
}
