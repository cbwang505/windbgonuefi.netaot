using Internal.Runtime.CompilerHelpers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;


namespace System.Linq
{


    public class WhereEnumerator<TSource> : IEnumerator<TSource>, System.Collections.IEnumerator
    {
        private IEnumerator<TSource> list;
        private int index;
        private int version;
        private TSource current;
        private int _Count = 0;
        private Func<TSource, bool> match;
        public WhereEnumerator(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            this.list = source.GetEnumerator();
            index = 0;
            // _Count = list.Count;
            match = predicate;

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
                bool find = false;
                while (list.MoveNext())
                {
                    current = list.Current;
                    if (match(current))
                    {
                        find = true;
                        break;
                    }
                    index++;
                }
                return find;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {

            return false;
        }

        public TSource Current
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
  
    public class PartEnumerator<TSource> : IEnumerator<TSource>, System.Collections.IEnumerator
    {
        private IEnumerator<TSource> list;
        private int index;
        private int version;
        private TSource current;
        private int _Count = 0;
        private int takecount = 0;
        private int skipecount = 0;
        public PartEnumerator(IEnumerable<TSource> source, int count, bool istake)
        {
            this.list = source.GetEnumerator();
            index = 0;
            // _Count = list.Count;
            if (istake)
            {
                takecount = count;
            }
            else
            {
                skipecount = count;
            }

            init();
        }

        private void init()
        {
            if (skipecount > 0)
            {
                while (list.MoveNext())
                {

                    index++;
                    if (skipecount == index)
                    {
                        break;
                    }
                }
            }
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
                if (takecount != 0)
                {
                    if (skipecount == index)
                    {
                        return false;
                    }
                }

                if (list.MoveNext())
                {
                    current = list.Current;
                    index++;
                    return true;
                }

                return false;
            }

            return MoveNextRare();
        }

        private bool MoveNextRare()
        {

            return false;
        }

        public TSource Current
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
    public class EnumerableWraper<TSource> : IEnumerable<TSource>, System.Collections.IEnumerable
    {
        private Func<TSource, bool> match = null;
        private int _Count = 0;
        private int takecount = 0;
        private int skipecount = 0;
        public EnumerableWraper(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            match = predicate;
            _Count = source.Count;
        }

        public EnumerableWraper(IEnumerable<TSource> source, int count, bool istake)
        {
            if (istake)
            {
                takecount = count;
                _Count = count;
            }
            else
            {
                skipecount = count;
                _Count = source.Count - count;
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
        public System.Collections.IEnumerator GetEnumerator()
        {
            if (match != null)
            {
                return new WhereEnumerator<TSource>(this, match);

            }

            if (takecount != 0)
            {
                return new PartEnumerator<TSource>(this, takecount, true);
            }

            if (skipecount != 0)
            {
                return new PartEnumerator<TSource>(this, skipecount, false);
            }

            return null;
        }

        IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
        {
            if (match != null)
            {
                return new WhereEnumerator<TSource>(this, match);
            }

            if (takecount != 0)
            {
                return new PartEnumerator<TSource>(this, takecount, true);
            }

            if (skipecount != 0)
            {
                return new PartEnumerator<TSource>(this, skipecount, false);
            }

            return null;
        }
    }

    public static class Enumerable
    {
        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {

            return new EnumerableWraper<TSource>(source, predicate);

        }


        public static List<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            List<TResult> ret = new List<TResult>(source.Count + 1);
            if (selector == null)
            {
                Console.Write("Argument out scope selector");
                return ret;
            }


            foreach (TSource source1 in source)
            {
                if (source1 != null)
                {

                    TResult res = selector(source1);
                    ret.Add(res);
                }
            }
           
            return ret;
        }

        public static TResult[] SelectArray<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            TResult[] ret = new TResult[source.Count];
            if (selector == null)
            {
                Console.Write("Argument out scope selector");
                return ret;
            }

            int idx = 0;

            foreach (TSource source1 in source)
            {
                if (source1 != null)
                {

                    TResult res = selector(source1);
                    ret[idx] =(res);
                }

                idx++;
            }

            return ret;
        }


        public static IEnumerable<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {

            List<TSource> ret = new List<TSource>(source.Count);
            foreach (TSource source1 in source)
            {
                ret.Add(source1);

            }

            return ret;
        }

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        {

            return new EnumerableWraper<TSource>(source, count, true);

        }

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {

            return new EnumerableWraper<TSource>(source, count, false);
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            IEnumerator<TSource> firstenum = source.GetEnumerator();
            if (firstenum == null)
            {
                Console.WriteLine("firstenum == null");
                return default(TSource);
            }
           
            if (firstenum.MoveNext())
            {
              
                return firstenum.Current;
            }
            Console.WriteLine("FirstOrDefault default(TSource)");
            return default(TSource);
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source)
        {
            IEnumerator<TSource> firstenum = source.GetEnumerator();
            TSource ret = default(TSource);
            while (firstenum.MoveNext())
            {
                ret = firstenum.Current;
            }

            return ret;
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source,
            TSource defaultValue)
        {
            IEnumerator<TSource> firstenum = source.GetEnumerator();

            if (firstenum.MoveNext())
            {
                firstenum.Reset();
                return source;

            }
            List<TSource> ret = new List<TSource>();

            ret.Add(defaultValue);

            return ret;
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            IEnumerator<TSource> firstenum = first.GetEnumerator();
            IEnumerator<TSource> secondenum = second.GetEnumerator();
            while (firstenum.MoveNext())
            {
                if (secondenum.MoveNext())
                {
                    if (firstenum.Current != secondenum.Current)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public static List<TSource> OrderedEnumerable<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, bool reverse)
        {

            IEnumerator<TSource> firstenum = source.GetEnumerator();


            List<KeyValuePair<TSource, TKey>> _arrayNa = new List<KeyValuePair<TSource, TKey>>(source.Count);

            while (firstenum.MoveNext())
            {
                TKey tmp = keySelector(firstenum.Current);
                _arrayNa.Add(new KeyValuePair<TSource, TKey>(firstenum.Current, tmp));
            }

            for (int i = 0; i < _arrayNa.Count - 1; i++)
            {
                for (int j = 0; j < _arrayNa.Count - 1; j++)
                {
                    int k = j + 1;
                    if (_arrayNa[j].Value.Equals(_arrayNa[k].Value) == reverse) //判断是否满足交换条件
                    {
                        KeyValuePair<TSource, TKey> tmpj = _arrayNa[j];
                        KeyValuePair<TSource, TKey> tmpk = _arrayNa[k];

                        //交换位置
                        _arrayNa[j] = tmpk;
                        _arrayNa[k] = tmpj;

                    }
                }
            }


            return _arrayNa.Select(h => h.Key);
        }

        public static UInt32 Sum(this IEnumerable<UInt32> source)
        {
            UInt32 ret = 0;
            

            foreach (UInt32 source1 in source)
            {
                

                    UInt32 res = (source1);
                    ret += (res);
                
            }

            return ret;
        }

        public static bool AllSame<TSource>(this IEnumerable<TSource> source)
        {
           
            TSource cmp = source.FirstOrDefault();
            if (cmp == null)
            {
                return false;
            }
            
            foreach (TSource source1 in source)
            {
                if (source1 != null)
                {
                    if (cmp!=source1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static UInt32 Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, UInt32> selector)
        {
            UInt32 ret = 0;
            if (selector == null)
            {
                Console.Write("Argument out scope selector");
                return ret;
            }
            foreach (TSource source1 in source)
            {
                if (source1 != null)
                {
                   
                    UInt32 res = selector(source1);
                    ret +=(res);
                }
            }

            return ret;
        }

        public static List<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return OrderedEnumerable<TSource, TKey>(source, keySelector, false);
        }

        public static List<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return OrderedEnumerable<TSource, TKey>(source, keySelector, true);
        }

        public static List<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
        {
            IEnumerator<TSource> firstenum = source.GetEnumerator();
            List<TSource> ret = new List<TSource>(source.Count);
            int idx = source.Count;
            while (firstenum.MoveNext())
            {
                idx--;
                ret[idx] = firstenum.Current;
            }
            return ret;
        }


        public static List<int> Range(int start, int count)
        {

            List<int> ret = new List<int>();

            for (int i = start; i < start + count; i++)
            {
                ret.Add(i);
            }

            return ret;
        }

        public static List<TResult> Repeat<TResult>(TResult element, int count)
        {
            List<TResult> ret = new List<TResult>();


            for (int i = 0; i < count; i++)
            {
                ret.Add(element);
            }

            return ret;
        }

        public static TResult[] RepeatArray<TResult>(TResult element, int count)
        {
            TResult[] ret = new TResult[count];


            for (int i = 0; i < count; i++)
            {
                ret[i]=(element);
            }

            return ret;
        }
    }

    public class EnumerableWrapper<T,F> : IEnumerable<F>, System.Collections.IEnumerable where T: IEnumerator<F>
    {
        private T _emu;
        public EnumerableWrapper(T emu)
        {
            _emu = emu;
        }

        public int Count
        {
            get
            {
                return 0;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<F> GetEnumerator()
        {
            return _emu;
        }
    }
}