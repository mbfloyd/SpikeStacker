using UnityEngine;

public class SceneDIContainerHolder : MonoBehaviour
{
    public SceneDIContainer Container { get; private set; }

    void Awake()
    {
        // Scene container inherits from the global container
        Container = new SceneDIContainer(DIContainer.Instance);
    }
}
