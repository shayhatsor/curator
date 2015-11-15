using System.Threading;
using org.apache.utils;

namespace org.apache.curator
{
    internal abstract class Atomic<T>
    {
        private readonly Fenced<T> value;

        public Atomic(T newValue)
        {
            value = new Fenced<T>(newValue);
        }

        public T get()
        {
            return value.Value;
        }

        public void set(T newValue)
        {
            value.Value = newValue;
        }
    }

    internal class AtomicBoolean:Atomic<bool>
    {
        public AtomicBoolean(bool newValue) : base(newValue)
        {
        }
    }


    internal class AtomicLong : Fenced<long>
    {
        public AtomicLong(long value=0)
            : base(value)
        {
        }

        public long get()
        {
            return Value;
        }

        public void set(long newValue)
        {
            Value = newValue;
        }

        public long incrementAndGet()
        {
            return Interlocked.Increment(ref m_Value);
        }
    }

    internal class AtomicReference<T> : Atomic<T>
    {
        public AtomicReference(T newValue = default(T)) : base(newValue)
        {
        }
    }
}
