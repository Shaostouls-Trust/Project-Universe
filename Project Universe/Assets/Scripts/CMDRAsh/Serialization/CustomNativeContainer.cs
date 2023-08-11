using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace ProjectUniverse.Environment.Volumes
{
    [NativeContainer]
    public unsafe struct CustomNativeContainer<T> : IDisposable where T : unmanaged
    {
        // Raw pointers aren't usually allowed inside structures that are passed to jobs, but because it's protected
        // with the safety system, you can disable that restriction for it
        [NativeDisableUnsafePtrRestriction]
        internal void* m_Buffer;
        internal int m_Length;
        internal Allocator m_AllocatorLabel;

        // You should only declare and use safety system members with the ENABLE_UNITY_COLLECTIONS_CHECKS define.
        // In final builds of projects, the safety system is disabled for performance reasons, so these APIs aren't
        // available in those builds.
#if ENABLE_UNITY_COLLECTIONS_CHECKS

        // The AtomicSafetyHandle field must be named exactly 'm_Safety'.
        internal AtomicSafetyHandle m_Safety;

        // Statically register this type with the safety system, using a name derived from the type itself
        internal static readonly int s_staticSafetyId = AtomicSafetyHandle.NewStaticSafetyId<CustomNativeContainer<T>>();
#endif

        public CustomNativeContainer(Allocator allocator, params T[] initialItems)
        {
            m_Length = initialItems.Length;
            m_AllocatorLabel = allocator;

            // Calculate the size of the initial buffer in bytes, and allocate it
            int totalSize = UnsafeUtility.SizeOf<T>() * m_Length;
            m_Buffer = UnsafeUtility.MallocTracked(totalSize, UnsafeUtility.AlignOf<T>(), m_AllocatorLabel, 1);

            // Copy the data from the array into the buffer
            var handle = GCHandle.Alloc(initialItems, GCHandleType.Pinned);
            try
            {
                UnsafeUtility.MemCpy(m_Buffer, handle.AddrOfPinnedObject().ToPointer(), totalSize);
            }
            finally
            {
                handle.Free();
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Create the AtomicSafetyHandle and DisposeSentinel
            m_Safety = AtomicSafetyHandle.Create();

            // Set the safety ID on the AtomicSafetyHandle so that error messages describe this container type properly.
            AtomicSafetyHandle.SetStaticSafetyId(ref m_Safety, s_staticSafetyId);

            // Automatically bump the secondary version any time this container is scheduled for writing in a job
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);

            // Check if this is a nested container, and if so, set the nested container flag
            if (UnsafeUtility.IsNativeContainerType<T>())
                AtomicSafetyHandle.SetNestedContainer(m_Safety, true);
#endif
        }

        public int Length
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                // Check that you are allowed to read information about the container 
                // This throws InvalidOperationException if you aren't allowed to read from the native container,
                // or if the native container has been disposed
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return m_Length;
            }
        }

        public T this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                // Check that you can read from the native container right now.
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif

                // Read from the buffer and return the value
                return UnsafeUtility.ReadArrayElement<T>(m_Buffer, index);
            }

            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                // Check that you can write to the native container right now.
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                // Write the value into the buffer
                UnsafeUtility.WriteArrayElement(m_Buffer, index, value);
            }
        }

        public void Add(T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Check that you can modify (write to) the native container right now, and if so, bump the secondary version so that
            // any views are invalidated, because you are going to change the size and pointer to the buffer
            AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(m_Safety);
#endif

            // Replace the current buffer with a new one that has space for an extra element
            int newTotalSize = (m_Length + 1) * UnsafeUtility.SizeOf<T>();
            void* newBuffer = UnsafeUtility.MallocTracked(newTotalSize, UnsafeUtility.AlignOf<T>(), m_AllocatorLabel, 1);
            UnsafeUtility.MemCpy(newBuffer, m_Buffer, m_Length * UnsafeUtility.SizeOf<T>());
            UnsafeUtility.FreeTracked(m_Buffer, m_AllocatorLabel);
            m_Buffer = newBuffer;

            // Put the new element at the end of the buffer and increase the length
            UnsafeUtility.WriteArrayElement(m_Buffer, m_Length++, value);
        }

        public NativeArray<T> AsArray()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Check that it's safe for you to use the buffer pointer to construct a view right now.
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(m_Safety);

            // Make a copy of the AtomicSafetyHandle, and mark the copy to use the secondary version instead of the primary
            AtomicSafetyHandle handleForArray = m_Safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref handleForArray);
#endif

            // Create a new NativeArray which aliases the buffer, using the current size. This doesn't allocate or copy
            // any data, it just sets up a NativeArray<T> which points at the m_Buffer.
            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(m_Buffer, m_Length, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Set the AtomicSafetyHandle on the newly created NativeArray to be the one that you copied from your handle
            // and made to use the secondary version.
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, handleForArray);
#endif

            return array;
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckDeallocateAndThrow(m_Safety);
            AtomicSafetyHandle.Release(m_Safety);
#endif

            // Free the buffer
            UnsafeUtility.FreeTracked(m_Buffer, m_AllocatorLabel);
            m_Buffer = null;
            m_Length = 0;
        }
    }

}