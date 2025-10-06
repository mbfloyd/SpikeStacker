using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoadCheck : MonoBehaviour
{
    void Awake()
    {
        try
        {
            IGameController gameController = DIContainer.Instance.Resolve<IGameController>();
        }
        catch
        {
            SceneManager.LoadScene("GameLoad");
        }
        Destroy(gameObject);

    }
}
