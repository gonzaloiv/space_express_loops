using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public interface IPoolableNode
    {
        bool IsActive { get; }
        void Initialize();
        void Hide();
    }

    public static class NodePoolExtensions
    {
        public static void ForEachInPool<T>(this IList<T> pool, Action<T> action)
            where T : class, IPoolableNode
        {
            for (int i = 0; i < pool.Count; i++)
            {
                T node = pool[i];
                if (node != null)
                    action(node);
            }
        }

        public static bool IsAnyActive<T>(this IList<T> pool)
            where T : class, IPoolableNode
        {
            for (int i = 0; i < pool.Count; i++)
            {
                T node = pool[i];
                if (node != null && node.IsActive)
                    return true;
            }

            return false;
        }

        public static T AcquireInactive<T>(
            this List<T> pool,
            T prototype,
            Transform parent,
            Action<T> onInstantiated)
            where T : Component, IPoolableNode
        {
            for (int i = 0; i < pool.Count; i++)
            {
                T node = pool[i];
                if (node != null && !node.IsActive)
                    return node;
            }

            T created = UnityEngine.Object.Instantiate(prototype, parent);
            onInstantiated(created);
            pool.Add(created);
            return created;
        }

        public static T EnsureSlot<T>(
            this List<T> pool,
            int index,
            Transform parent,
            Action<T> onInstantiated)
            where T : Component, IPoolableNode
        {
            while (index >= pool.Count)
            {
                T created = UnityEngine.Object.Instantiate(pool[0], parent);
                onInstantiated(created);
                pool.Add(created);
            }

            return pool[index];
        }
    }
}
