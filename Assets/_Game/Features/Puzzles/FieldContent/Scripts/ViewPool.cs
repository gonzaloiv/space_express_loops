using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Lazy slots over an authored prototype: the one place pooled views are instantiated.
    public class ViewPool<T> where T : Component
    {
        private readonly T prototype;
        private readonly Transform parent;
        private readonly string namePrefix;
        private readonly Action<T> onCreated;
        private readonly List<T> views = new();

        public ViewPool(T prototype, Transform parent, string namePrefix, Action<T> onCreated)
        {
            this.prototype = prototype;
            this.parent = parent;
            this.namePrefix = namePrefix;
            this.onCreated = onCreated;
        }

        public IReadOnlyList<T> Views => views;

        public void ForEachFrom(int index, Action<T> action)
        {
            for (int i = Mathf.Max(index, 0); i < views.Count; i++)
                action(views[i]);
        }

        public T EnsureSlot(int index)
        {
            while (index >= views.Count)
                views.Add(Create());

            return views[index];
        }

        private T Create()
        {
            T created = UnityEngine.Object.Instantiate(prototype, parent);
            created.name = namePrefix + " " + (views.Count + 1);
            onCreated?.Invoke(created);
            return created;
        }
    }
}
