namespace System
{
    public struct Slice<T>
    {
        object  m_object; // A managed array/string; or null for native ptrs.
        UIntPtr m_offset; // An byte-offset into the array/string; or a native ptr.
        int     m_length; // The length of the slice.

        public Slice(T[] array)
        {
            Contract.Requires(array != null);
            m_object = array;
            m_offset = new UIntPtr((uint)SliceHelpers<T>.OffsetToArrayData);
            m_length = array.Length;
        }

        public Slice(T[] array, int start)
        {
            Contract.Requires(array != null);
            Contract.RequiresInInclusiveRange(start, array.Length);
            if (start < array.Length) {
                m_object = array;
                m_offset = new UIntPtr(
                    (uint)(SliceHelpers<T>.OffsetToArrayData + (start * PtrUtils.SizeOf<T>())));
                m_length = array.Length - start;
            }
            else {
                m_object = null;
                m_offset = UIntPtr.Zero;
                m_length = 0;
            }
        }

        public Slice(T[] array, int start, int end)
        {
            Contract.Requires(array != null);
            Contract.RequiresInInclusiveRange(start, array.Length);
            if (start < array.Length) {
                m_object = array;
                m_offset = new UIntPtr(
                    (uint)(SliceHelpers<T>.OffsetToArrayData + (start * PtrUtils.SizeOf<T>())));
                m_length = end - start;
            }
            else {
                m_object = null;
                m_offset = UIntPtr.Zero;
                m_length = 0;
            }
        }

        public unsafe Slice(void* ptr, int length)
        {
            Contract.Requires(length >= 0);
            Contract.Requires(length == 0 || ptr != null);
            m_object = null;
            m_offset = new UIntPtr(ptr);
            m_length = length;
        }

        internal Slice(object obj, UIntPtr offset, int length)
        {
            m_object = obj;
            m_offset = offset;
            m_length = length;
        }

        public int Length
        {
            get { return m_length; }
        }

        internal object Object
        {
            get { return m_object; }
        }

        internal UIntPtr Offset
        {
            get { return m_offset; }
        }

        public T this[int index]
        {
            get {
                Contract.RequiresInRange(index, Length);
                return PtrUtils.Get<T>(
                    m_object, m_offset + (index * PtrUtils.SizeOf<T>()));
            }
            set {
                Contract.RequiresInRange(index, Length);
                PtrUtils.Set<T>(
                    m_object, m_offset + (index * PtrUtils.SizeOf<T>()), value);
            }
        }

        public Slice<T> Sub(int start)
        {
            return Sub(start, Length);
        }

        public Slice<T> Sub(int start, int end)
        {
            Contract.RequiresInInclusiveRange(start, end, Length);
            return new Slice<T>(
                m_object, m_offset + (start * PtrUtils.SizeOf<T>()), end - start);
        }
    }
}


