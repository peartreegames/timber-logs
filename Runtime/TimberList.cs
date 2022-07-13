using UnityEngine;

namespace PeartreeGames.TimberLogs
{
    public class TimberList<T> where T : class
    {
        public int Count => Mathf.Min(_startIndex, _capacity);

        private readonly T[] _list;
        private readonly int _capacity;
        private int _startIndex;

        public TimberList(int capacity)
        {
            _capacity = capacity;
            _list = new T[_capacity];
        }

        public void Add(T item) => _list[_startIndex++ % _capacity] = item;
        public T this[int index]  {
            get
            {
                index += _startIndex;
                while (index < 0) index += Count;
                return _list[index % Count];
            }
            
        }
    }
}