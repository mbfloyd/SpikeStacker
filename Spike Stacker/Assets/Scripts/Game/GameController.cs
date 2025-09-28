using UnityEngine.SceneManagement;

public class GameController : IGameController
{

    public void Start()
    {
        SceneManager.LoadScene("GamePlay");
    }
}
