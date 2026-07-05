using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMORPG.Framework.Pooling
{
    public sealed class ComponentObjectPool<T> where T : Component
    {
        private readonly Stack<T> inactiveItems = new Stack<T>();
        private readonly Func<T> createFunc;
        private readonly Transform inactiveRoot;

        public ComponentObjectPool(Func<T> createFunc, Transform inactiveRoot = null, int preloadCount = 0)
        {
            this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            this.inactiveRoot = inactiveRoot;

            for (int i = 0; i < preloadCount; i++)
            {
                T item = CreateItem();
                Release(item);
            }
        }

        public int InactiveCount => inactiveItems.Count;

        public T Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            T item = inactiveItems.Count > 0 ? inactiveItems.Pop() : CreateItem();
            Transform itemTransform = item.transform;
            itemTransform.SetParent(parent, true);
            itemTransform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);

            if (item is IPoolable poolable)
            {
                poolable.OnSpawnedFromPool();
            }

            return item;
        }

        public void Release(T item)
        {
            if (item == null)
            {
                return;
            }

            if (item is IPoolable poolable)
            {
                poolable.OnDespawnedToPool();
            }

            item.gameObject.SetActive(false);
            if (inactiveRoot != null)
            {
                item.transform.SetParent(inactiveRoot, false);
            }

            inactiveItems.Push(item);
        }

        public void Clear()
        {
            while (inactiveItems.Count > 0)
            {
                T item = inactiveItems.Pop();
                if (item != null)
                {
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
        }

        private T CreateItem()
        {
            T item = createFunc();
            if (item == null)
            {
                throw new InvalidOperationException("对象池创建函数返回了空对象。");
            }

            if (inactiveRoot != null)
            {
                item.transform.SetParent(inactiveRoot, false);
            }

            return item;
        }
    }
}
