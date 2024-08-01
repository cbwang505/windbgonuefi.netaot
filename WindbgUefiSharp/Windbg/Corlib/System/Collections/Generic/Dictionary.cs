using System.Collections;
using System.Collections.Generic;

namespace System.Collections.Generic
{
    public class Dictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, System.Collections.IEnumerable
    {
        public int _Count = 0;
        public Dictionary(int initsize = 256)
        {
            Keys = new List<TKey>(initsize);
            Values = new List<TValue>(initsize);
        }
        public TValue this[TKey key]
        {
            get
            {
                return Values[Keys.IndexOf(key)];
            }
            set
            {
                Values[Keys.IndexOf(key)] = value;
            }
        }

        public int Count
        {
            get
            {
                return _Count;
            }
            set
            {
                _Count = value;
            }
        }
        public void Remove(TKey key)
        {
            Values.Remove(Values[Keys.IndexOf(key)]);
            Keys.Remove(key);
            Count--;
        }




        public bool ContainsKey(TKey key)
        {
            if (Count == 0)
            {
                return false;
            }
            return Keys.IndexOf(key) != -1;
        }

        public bool ContainsValue(TValue value)
        {
            return Values.IndexOf(value) != -1;
        }

        public void Add(TKey key, TValue value)
        {
            Keys.Add(key);
            Values.Add(value);
            Count++;
            
        }

        public void Clear()
        {
            Keys.Clear();
            Values.Clear();
            Count = 0;
        }

        public override void Dispose()
        {
            Keys.Clear();
            Values.Clear();
            Values.Dispose();
            Keys.Dispose();
            base.Dispose();
        }
        public System.Collections.IEnumerator GetEnumerator()
        {
            return new EnumeratorDictionary<TKey, TValue>(this);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new EnumeratorDictionary<TKey, TValue>(this);
        }
        public List<TKey> Keys;
        public List<TValue> Values;
    }
    public class KeyValuePair<TKey, TValue>
    {
        private TKey key;
        private TValue value;

        public KeyValuePair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        public TKey Key
        {
            get { return key; }
        }

        public TValue Value
        {
            get { return value; }
        }

        public override string ToString()
        {
            string s = "";
            s += ('[');
            if (Key != null)
            {
                s += (Key.ToString());
            }
            s += (", ");
            if (Value != null)
            {
                s += (Value.ToString());
            }
            s += (']');
            return s;
        }
    }




    public class EnumeratorDictionary<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>, System.Collections.IEnumerator
    {
        private List<TKey> Keys;
        private List<TValue> Values;

        private int index;
        private int version;
        private KeyValuePair<TKey, TValue> current;
        private int _Count = 0;
        public EnumeratorDictionary(Dictionary<TKey, TValue> dic)
        {
            this.Keys = dic.Keys;
            this.Values = dic.Values;
            index = 0;
            _Count = dic.Count;

        }

        public int Count
        {
            get
            {
                return _Count;
            }
        }


        public bool MoveNext()
        {


            if (((uint)index < (uint)Count))
            {
                current = new KeyValuePair<TKey, TValue>(Keys[index], Values[index]);
                index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {

            return false;
        }

        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                return current;
            }
        }

        Object System.Collections.IEnumerator.Current
        {
            get
            {

                return Current;
            }
        }

        void System.Collections.IEnumerator.Reset()
        {


            index = 0;

        }

    }
}