using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using Internal.Runtime.CompilerHelpers;


namespace System.Collections.Generic
{
    public class List<T> : IEnumerable<T>, System.Collections.IEnumerable
    {
        public T[] _value;

        public int _Count = 0;
        public int Capaciity = 0;
        private bool copymode = true;
        private bool disposed = false;
        public List(int initsize = 256)
        {
            Capaciity = initsize;
            _value = new T[initsize];
        }


        public List(T[] t,bool orgmode=true)
        {
            _value = t;
            Count = t.Length;
            Capaciity = t.Length;
            copymode= orgmode;
           

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
        int System.Collections.IEnumerable.Count
        {
            get
            {
                return Count;
            }

        }
        public ref T this[int index]
        {
            get
            {
                return ref _value[index];
            }
            /*set
            {
                _value[index] = value;
            }*/
        }

        public void Add(T t)
        {

            AutoExpand(Count);
            _value[Count] = t;
            Count++;
        }

        public void AutoExpand(int newlen)
        {
            if (newlen + 1 >= Capaciity)
            {
                if (!copymode)
                {
                    IntPtr ptrthat = this;
                    Console.WriteLine("AutoExpand Overflow this:=>" + ptrthat.ToString("x") + " newlen:=>" + newlen.ToString("x")+ ",Capaciity:=>"+ Capaciity.ToString("x"));
                    Debug.Halt(true);
                }
                Capaciity = Capaciity * 2;


                T[] newvalue = new T[Capaciity];

                Array.Move<T>(_value, newvalue, Count);
                _value.Dispose();
                _value = newvalue;
            }
            return;

        }
        public void Insert(int index, T item, bool internalMove = false)
        {
            //Broken
            //if (index == IndexOf(item)) return;
            AutoExpand(Count);
            if (!internalMove)
                Count++;

            if (internalMove)
            {
                int _index = IndexOf(item);
                for (int i = _index; i < Count - 1; i++)
                {
                    _value[i] = _value[i + 1];
                }
            }

            for (int i = Count - 1; i > index; i--)
            {
                _value[i] = _value[i - 1];
            }
            _value[index] = item;
        }

        public unsafe int ElementSize
        {
            get
            {
                return sizeof(T);
            }
        }

        public void Reset(T val)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] = val;
            }
        }

        public T[] ToArray()
        {
            T[] array = new T[Count];
            for (int i = 0; i < Count; i++)
            {
                array[i] = this[i];
            }
            return array;
        }
        public T[] GetArrayPtr()
        {
            return _value;
        }
        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                T first = this[i];
                T second = item;

                if (first.Equals(second))
                    return i;
            }

            return -1;
        }
        public bool Remove(T item)
        {
            int at = IndexOf(item);

            if (at < 0)
                return false;

            RemoveAt(at);

            return true;
        }

        public void RemoveAt(int index)
        {
            Count--;

            for (int i = index; i < Count; i++)
            {
                _value[i] = _value[i + 1];
            }

            _value[Count] = default(T);
        }
        public void RemoveRange(int index,int count)
        {
            
            Debug.Assert(index + count <= Count,"RemoveRange overflow");
              
            
           
           
            for (int i = index; i + count < Count; i++)
            {
                int j = i + count;

                _value[i] = _value[j];
                
            }
            Count-= count;
            _value[Count] = default(T);
        }

        public override void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed=true;
            if (copymode)
            {
                _value.Dispose();
            }


            base.Dispose();
        }

        public void Clear()
        {
            Count = 0;
        }

        public void Move(IntPtr dest, int startIndex, int destIndex, int count)
        {
            Array.Move(_value, dest, startIndex, destIndex, count);
        }

        public void Move(IntPtr dest, int startIndex, int count)
        {
            Array.Move(_value, dest, startIndex, 0, count);

        }
       

        public void Move(IntPtr dest,  int count)
        {
            Array.Move(_value, dest, 0, 0, count);
        }
        public void From(IntPtr src, int startIndex, int count)
        {
           From(src, startIndex, 0, count);
            
        }

        public void From(IntPtr src, int startIndex, int destIndex, int count)
        {
            Array.From(src, _value, startIndex, destIndex, count);
            Count = startIndex+ destIndex + count;
            Count = Count / ElementSize;
        }


        public void From(IntPtr src, int count)
        {
            From(src,  0, 0, count);
           
        }

       
        System.Collections.IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<T>(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator<T>(this);
        }
    }

    public class Enumerator : System.Collections.IEnumerator
    {
        private Array list;
        private int index;
        private int version;
        private object? current;
        private int _Count = 0;
        public Enumerator(Array arr)
        {
            this.list = arr;
            index = 0;
            _Count = arr.Length;

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
                current = list[index];
                index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {

            return false;
        }

        public object? Current
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

    public class Enumerator<T> : IEnumerator<T>, System.Collections.IEnumerator
    {
        private T[] list;
        private int index;
        private int version;
        private T current;
        private int _Count = 0;
        public Enumerator(List<T> list)
        {
            this.list = list._value;
            index = 0;
            _Count = list.Count;

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
                current = list[index];
                index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {

            return false;
        }

        public T Current
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

                return current;
            }
        }

        void System.Collections.IEnumerator.Reset()
        {


            index = 0;

        }

    }

    public class ByteList : List<byte>
    {
        public ByteList(int initsize = 256) : base(initsize)
        {
        }

        public ByteList(List<byte> t) : base(t._value)
        {

        }
        public ByteList(byte[] t, bool orgmode = true) : base(t, orgmode)
        {
        }

        public void Fill(int Length)
        {
            for (int i = 0; i < Length; i++)
            {
                this.Add(0);
            }
            return;
        }


        public void CopyTo(ByteList ls)
        {
            UInt32 len = this.Count;
            for (int i = 0; i < len; i++)
            {
                byte tmp = this[i];
                ls.Add(tmp);
            }
            return;
        }
        public void CopyTo(ByteList ls,int offset,int count)
        {
            if (offset > this.Count)
            {
                return;
            }
            UInt32 lenend = offset+ count;

            if (lenend > this.Count)
            {
                lenend=this.Count;
            }
            for (int i = offset; i < lenend; i++)
            {
                byte tmp = this[i];
                ls.Add(tmp);
            }
            return;
        }

        public ByteList TakePart(int count)
        {
            ByteList ret = new ByteList(count);

            for (int i = 0; i < count; i++)
            {
                ret.Add(this[i]);
            }
            return ret;
        }


        public void Write( int startIndex, UInt32 val)
        {

            int remain = 4;
           
            for (int i = 0; i < remain; i++)
            {
                if (startIndex + i >= Count)
                {
                    break;
                }

                byte bt=(byte)( (val << (i * 8))&0xff);
                this[startIndex+i]=bt;
            }

            return;
           

        }

        public UInt32 Read( int startIndex)
        {
             UInt32 val= 0;

            int remain = 4;

            for (int i = 0; i < remain; i++)
            {
                if (startIndex + i >= Count)
                {
                    break;
                }

                byte bt = this[startIndex + i];
                UInt32 tmp = bt;
                val|=(tmp << (i * 8)) ;
               
            }

            return val;

        }

        public void Reset()
        {
            Reset(0);
            return;
        }

    }

    public class ManualDropByteList : ByteList
    {
        public ManualDropByteList(byte[] t ) : base(t, false)
        {
        }
    }
}