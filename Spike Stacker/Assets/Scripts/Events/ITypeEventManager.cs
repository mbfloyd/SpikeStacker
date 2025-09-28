
using System;

namespace ALIyerEdon
{
    public interface ITypeEventManager
    {
        public void RegisterListener<T>(Action<T> listener) where T : GameEvent;
        public void UnregisterListener<T>(Action<T> listener) where T : GameEvent;
        public void TriggerEvent<T>(T gameEvent) where T : GameEvent;
        
    }
}
