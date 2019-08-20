using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// Similar to NativeSlice but blittable. Provides a "view"
    /// into a contiguous array of memory. Used to interop with C.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeView<T> where T : struct
    {
        void* m_Ptr;
        int m_Length;

        public NativeView(void* ptr, int length)
        {
            m_Ptr = ptr;
            m_Length = length;
        }

        public NativeView(NativeArray<T> array)
        {
            m_Ptr = array.GetUnsafePtr();
            m_Length = array.Length;
        }

        public NativeView(NativeSlice<T> slice)
        {
            m_Ptr = slice.GetUnsafePtr();
            m_Length = slice.Length;
        }

        public static implicit operator NativeView<T>(NativeArray<T> array) => new NativeView<T>(array);
        public static implicit operator NativeView<T>(NativeSlice<T> slice) => new NativeView<T>(slice);
    }
}
