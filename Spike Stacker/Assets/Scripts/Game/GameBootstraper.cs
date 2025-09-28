using Unity.VisualScripting;
using UnityEngine;

public class GameBootstraper : MonoBehaviour
{
    void Awake()
    {
        SetupDependencies();
        GameStart();
    }

    private void SetupDependencies()
    {
        // Register as Singleton (default is Singleton if not specified)
        DIContainer.Instance.Register<ITypeEventManager, TypeEventManager>(Lifetime.Singleton);
        DIContainer.Instance.Register<IGameController, GameController>(Lifetime.Singleton);
    }

    private void GameStart()
    {
        IGameController gameController = DIContainer.Instance.Resolve<IGameController>();
        gameController.Start();
    }
}
