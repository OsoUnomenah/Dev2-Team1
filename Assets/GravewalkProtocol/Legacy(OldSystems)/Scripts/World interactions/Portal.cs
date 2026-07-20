using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            sceneLoader.LoadScene(nextSceneName);
        }
    }
}