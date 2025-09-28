using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class DIBehaviour : MonoBehaviour
{
    protected SceneDIContainer SceneContainer { get; private set; }

    protected virtual void Awake()
    {
        var holder = FindFirstObjectByType<SceneDIContainerHolder>();
        SceneContainer = holder?.Container;

        InjectDependencies();
    }

    private void InjectDependencies()
    {
        var container = (object)SceneContainer ?? DIContainer.Instance;

        // Field injection
        if (SceneContainer != null)
            SceneContainer.InjectFields(this);
        else
            DIContainer.Instance.InjectFields(this);

        // NOTE: Constructor injection only works if this MonoBehaviour
        // was created via DI (e.g., container.Resolve<EnemyController>())
    }
}
