using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LeapForward.IdleHelpers
{
    public class PatternMatchingValues : IDictionary<string, BigNumber>, INotifyPropertyChanged
    {
        readonly Dictionary<string, BigNumber> _values = new Dictionary<string, BigNumber>();

        public event PropertyChangedEventHandler PropertyChanged;

        public BigNumber this[string key]
        {
            get
            {
                if (!_values.TryGetValue(key, out var value))
                    return BigNumber.Zero;
                return value;
            }
            set
            {
                var existingValue = this[key];
                if (value != existingValue)
                {
                    _values[key] = value;
                    OnPropertyChanged(key);
                }
            }
        }

        public ICollection<string> Keys => ((IDictionary<string, BigNumber>)_values).Keys;

        public ICollection<BigNumber> Values => ((IDictionary<string, BigNumber>)_values).Values;

        public int Count => ((ICollection<KeyValuePair<string, BigNumber>>)_values).Count;

        bool ICollection<KeyValuePair<string, BigNumber>>.IsReadOnly => ((ICollection<KeyValuePair<string, BigNumber>>)_values).IsReadOnly;

        public void Add(string key, BigNumber value)
        {
            _values.Add(key, value);
        }

        void ICollection<KeyValuePair<string, BigNumber>>.Add(KeyValuePair<string, BigNumber> item)
        {
            ((ICollection<KeyValuePair<string, BigNumber>>)_values).Add(item);
        }

        public void Clear()
        {
            _values.Clear();
        }

        bool ICollection<KeyValuePair<string, BigNumber>>.Contains(KeyValuePair<string, BigNumber> item)
        {
            return ((ICollection<KeyValuePair<string, BigNumber>>)_values).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, BigNumber>)_values).ContainsKey(key);
        }

        void ICollection<KeyValuePair<string, BigNumber>>.CopyTo(KeyValuePair<string, BigNumber>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, BigNumber>>)_values).CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<string, BigNumber>> IEnumerable<KeyValuePair<string, BigNumber>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, BigNumber>>)_values).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, BigNumber>)_values).Remove(key);
        }

        public bool Remove(KeyValuePair<string, BigNumber> item)
        {
            return ((ICollection<KeyValuePair<string, BigNumber>>)_values).Remove(item);
        }

        public bool TryGetValue(string key, out BigNumber value)
        {
            return ((IDictionary<string, BigNumber>)_values).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_values).GetEnumerator();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}