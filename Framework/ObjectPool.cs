using System.Collections.Generic;
namespace MobaGame.Collision
{
    public class ObjectPool<T> where T : new()
    {
        private List<T> list;

        public ObjectPool()
        {
            list = new List<T>();
        }

        public T Create()
        {
            return new T();
        }

        public T Get()
        {
            if(list.Count > 0)
            {
                T a = list[0];
                list.RemoveAt(0);
                return a;
            }
            else
            {
                return Create();
            }
        }

        public void Release(T obj)
        {
            list.Add(obj);
        }
    }
}
