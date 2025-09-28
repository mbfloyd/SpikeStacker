using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class TypeEventManager : ITypeEventManager
    {
        private Dictionary<Type, Action<GameEvent>> eventDictionary = new();
        private Dictionary<Delegate, Action<GameEvent>> listenerLookup = new();

        // Register a listener
        public void RegisterListener<T>(Action<T> listener) where T : GameEvent
        {
            if (listenerLookup.ContainsKey(listener))
                return; // already registered

            Action<GameEvent> wrapper = (e) => listener((T)e);
            listenerLookup[listener] = wrapper;

            Type eventType = typeof(T);
            if (eventDictionary.ContainsKey(eventType))
            {
                eventDictionary[eventType] += wrapper;
            }
            else
            {
                eventDictionary[eventType] = wrapper;
            }
        }

        // Unregister a listener
        public void UnregisterListener<T>(Action<T> listener) where T : GameEvent
        {
            if (!listenerLookup.TryGetValue(listener, out var wrapper))
                return; // not registered

            Type eventType = typeof(T);
            if (eventDictionary.ContainsKey(eventType))
            {
                eventDictionary[eventType] -= wrapper;
                if (eventDictionary[eventType] == null)
                    eventDictionary.Remove(eventType);
            }
            listenerLookup.Remove(listener);
        }

        // Trigger an event
        public void TriggerEvent<T>(T gameEvent) where T : GameEvent
        {
            Type eventType = typeof(T);

            if (eventDictionary.ContainsKey(eventType))
            {
                eventDictionary[eventType]?.Invoke(gameEvent);
            }
        }
    }
}
