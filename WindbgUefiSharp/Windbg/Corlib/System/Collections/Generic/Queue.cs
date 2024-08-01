namespace System.Collections.Generic
{
    public class Queue<T> where T : struct
    {
        List<T> list;

        public Queue(int initsize = 256)
        {
            list = new List<T>(initsize);
        }

        public Nullable<T> Tail
        {
            get
            {
                if (Count == 0) return new Nullable<T>();
                else
                {
                    return list[Count - 1];
                }
            }
        }

        public Nullable<T> Head
        {
            get
            {
                if (Count == 0) return new Nullable<T>();
                else
                {
                    return list[0];
                }
            }
        }

        public int Count
        {
            get => list.Count;
            set => list.Count = value;
        }

        public void Enqueue(T item)
        {
            list.Add(item);
        }

        public Nullable<T> Dequeue()
        {
            if (Count == 0)
            {
                return new Nullable<T>();
            }

            T res = list[0];
            for (int i = 1; i < Count; i++)
            {
                list[i - 1] = list[i];
            }
            Count--;
            return new Nullable<T>(res);
        }
    }

    public class QueueClass<T> where T : class
    {
        List<T> list;

        public QueueClass(int initsize = 256)
        {
            list = new List<T>(initsize);
        }

        public T Tail
        {
            get
            {
                if (Count == 0) return null;
                else
                {
                    return list[Count - 1];
                }
            }
        }

        public T Head
        {
            get
            {
                if (Count == 0) return null;
                else
                {
                    return list[0];
                }
            }
        }

        public int Count
        {
            get => list.Count;
            set => list.Count = value;
        }

        public void Enqueue(T item)
        {
            list.Add(item);
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                return default(T);
            }

            T res = list[0];
            for (int i = 1; i < Count; i++)
            {
                list[i - 1] = list[i];
            }
            Count--;
            return res;
        }
    }
}