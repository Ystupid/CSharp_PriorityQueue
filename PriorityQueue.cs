using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Collections.Generic
{
    /*  Insert [4] [3] [1] [2] [0]
     *  
     *              [0]0  ←--m_Head
     *              ╱ ╲
     *           1[1][3]2
     *            ╱ ╲ 
     *         3[4] [2]4  ←--m_Tail
     */
    public class PriorityQueue<T> : IEnumerable<T>, IEnumerable
    {
        private T[] m_Array;
        private int m_Head;
        private int m_Tail;
        private int m_Size;
        private int m_Version;
        private IComparer<T> m_Comparer;

        private static T[] EmptyArray = new T[0];

        public T this[int index] => m_Array[index];

        public PriorityQueue()
        {
            m_Array = EmptyArray;
            m_Comparer = Comparer<T>.Default;
        }
        public PriorityQueue(IComparer<T> comparer) : this() => m_Comparer = comparer;

        public int Count => m_Size;

        public void Enqueue(T element)
        {
            if (m_Size == m_Array.Length)
            {
                int capacity = (int)((long)m_Array.Length * 200L / 100L);
                if (capacity < m_Array.Length + 4)
                    capacity = m_Array.Length + 4;
                SetCapacity(capacity);
            }

            int index = 0;
            int parent = 0;

            index = m_Size;
            while (index > 0)
            {
                parent = (index + 1) / 2 - 1;

                if (m_Comparer.Compare(m_Array[parent], element) > 0)
                    m_Array[index] = m_Array[parent];
                else break;

                index = parent;
            }

            m_Array[index] = element;

            m_Tail = m_Size;
            ++m_Size;
            ++m_Version;
        }

        public T Peek()
        {
            if (m_Size <= 0)
                throw new InvalidOperationException();
            return m_Array[m_Head];
        }

        public T Dequeue()
        {
            if (m_Size <= 0)
                throw new InvalidOperationException();

            var element = m_Array[m_Head];
            RemoveAt(m_Head);
            return element;
        }

        public void RemoveAt(int index)
        {
            if (index < m_Head || index > m_Tail)
                throw new IndexOutOfRangeException();

            int child = index;

            while (index <= m_Size)
            {
                child = index * 2 + 1;

                if (child >= m_Size - 1) break;

                child += m_Comparer.Compare(m_Array[child], m_Array[child + 1]) >= 0 ? 1 : 0;

                if (m_Comparer.Compare(m_Array[child], m_Array[m_Tail]) >= 0)
                    break;

                m_Array[index] = m_Array[child];

                index = child;
            }

            m_Array[index] = m_Array[m_Tail];
            m_Array[m_Tail] = default(T);

            --m_Size;
            m_Tail = m_Size - 1;
            ++m_Version;
        }

        public bool Contains(T item)
        {
            if (m_Size <= 0) return false;

            var equalityComparer = EqualityComparer<T>.Default;

            for (int i = m_Head; i < m_Size; i++)
                if (equalityComparer.Equals(m_Array[i], item))
                    return true;

            return false;
        }

        public T[] ToInvertedOrderArray()
        {
            var size = m_Size;
            for (int i = 0; i < size; i++)
                m_Array[m_Tail] = Dequeue();

            return m_Array;
        }

        public int FindIndex(Predicate<T> match)
        {
            if (match == null) return -1;

            for (int i = 0; i < m_Size; i++)
                if (match(m_Array[i])) return i;

            return -1;
        }

        private void SetCapacity(int capacity)
        {
            T[] objArray = new T[capacity];
            if (m_Size > 0)
            {
                if (m_Head < m_Tail)
                    Array.Copy(m_Array, m_Head, objArray, 0, m_Size);
                else
                {
                    Array.Copy(m_Array, m_Head, objArray, 0, m_Array.Length - m_Head);
                    Array.Copy(m_Array, 0, objArray, m_Array.Length - m_Head, m_Tail);
                }
            }
            m_Array = objArray;
            m_Head = 0;
            m_Tail = m_Size == capacity ? 0 : m_Size;
            ++m_Version;
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        internal T GetElement(int i) => m_Array[(m_Head + i) % m_Array.Length];

        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private PriorityQueue<T> m_PriorityQueue;
            private int m_Index;
            private int m_Version;
            private T m_CurrentElement;

            internal Enumerator(PriorityQueue<T> priorityQueue)
            {
                m_PriorityQueue = priorityQueue;
                m_Version = m_PriorityQueue.m_Version;
                m_Index = -1;
                m_CurrentElement = default(T);
            }

            public void Dispose()
            {
                m_Index = -2;
                m_CurrentElement = default(T);
            }

            public bool MoveNext()
            {
                if (m_Version != m_PriorityQueue.m_Version)
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                if (m_Index == -2)
                    return false;
                ++m_Index;
                if (m_Index == m_PriorityQueue.m_Size)
                {
                    m_Index = -2;
                    m_CurrentElement = default(T);
                    return false;
                }
                m_CurrentElement = m_PriorityQueue.GetElement(m_Index);
                return true;
            }

            object IEnumerator.Current => Current;

            public T Current
            {
                get
                {
                    if (m_Index < 0)
                    {
                        if (m_Index == -1) throw new InvalidOperationException("InvalidOperation_EnumNotStarted");
                        else throw new InvalidOperationException("InvalidOperation_EnumEnded");
                    }
                    return m_CurrentElement;
                }
            }

            public void Reset()
            {
                if (m_Version != m_PriorityQueue.m_Version)
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");

                m_Index = -1;
                m_CurrentElement = default(T);
            }
        }
    }
}
