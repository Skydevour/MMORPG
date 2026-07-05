namespace MMORPG.Framework.Pooling
{
    public interface IPoolable
    {
        void OnSpawnedFromPool();

        void OnDespawnedToPool();
    }
}
