using System;
using System.Collections.Generic;

namespace Compiler {
    public class MyEnumerator<T> : IEnumerator<T> {
        private readonly IEnumerator<T> _enumerator;
        private readonly List<T> _buffer;
        private int _index;
        object System.Collections.IEnumerator.Current => Current;


        public void Reset() {
            _enumerator.Reset();
            _buffer.Clear();
            _index--;
        }

        public bool MoveNext() {
            if (_index < _buffer.Count - 1) {
                _index++;
                return true;
            }

            if (!_enumerator.MoveNext()) return false;
            _buffer.Add(_enumerator.Current);
            _index++;
            return true;
        }

        public bool MovePrev() {
            if (_index <= 0) {
                return false;
            }

            _index--;
            return true;
        }

        public T Current {
            get {
                if (_index < 0 || _index >= _buffer.Count)
                    throw new InvalidOperationException();

                return _buffer[_index];
            }
        }

        void IDisposable.Dispose() {
            _enumerator.Dispose();
        }

        public MyEnumerator(IEnumerator<T> enumerator) {
            _enumerator = enumerator ?? throw new ArgumentNullException();
            _buffer = new List<T>();
            _index = -1;
        }
    }
}