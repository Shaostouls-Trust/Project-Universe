namespace Impact.Utility.ObjectPool
{
    public interface IPoolable
    {
        void Retrieve();
        void MakeAvailable();

        bool IsAvailable();
    }
}
