using System;
using System.Collections.Generic;

using D3D = SlimDX.Direct3D9;
using SlimDX;

namespace Uncut.Rendering
{
    /// <summary>
    /// An automatically-resizing buffer of primitive data, implemented using Direct3D9.
    /// </summary>
    public class DynamicPrimitiveBuffer9<T> : DynamicPrimitiveBuffer<T> where T : struct
    {
        #region Public Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPrimitiveBuffer9"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public DynamicPrimitiveBuffer9(D3D.Device device)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            this.device = device;
        }

        /// <summary>
        /// Disposes of object resources.
        /// </summary>
        /// <param name="disposeManagedResources">If true, managed resources should be
        /// disposed of in addition to unmanaged resources.</param>
        protected override void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                if (buffer != null)
                {
                    buffer.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the underlying buffer.
        /// </summary>
        public D3D.VertexBuffer UnderlyingBuffer
        {
            get
            {
                return buffer;
            }
        }

        #endregion
        #region Implementation Detail

        D3D.Device device;
        D3D.VertexBuffer buffer;

        /// <summary>
        /// In a derived class, implements logic to resize the buffer.
        /// During resize, the existing buffer contents need not be preserved.
        /// </summary>
        /// <param name="sizeInBytes">The new size, in bytes.</param>
        protected override void ResizeBuffer(int sizeInBytes)
        {
            if (buffer != null)
            {
                buffer.Dispose();
            }

            buffer = new D3D.VertexBuffer(
                device,
                sizeInBytes,
                D3D.Usage.Dynamic | D3D.Usage.WriteOnly,
                D3D.VertexFormat.None,
                D3D.Pool.Default
            );
        }

        /// <summary>
        /// In a derived class, implements logic to fill the buffer with vertex data.
        /// </summary>
        /// <param name="vertices">The vertex data.</param>
        protected override void FillBuffer(List<T> vertices)
        {
            DataStream stream = buffer.Lock(0, 0, D3D.LockFlags.Discard);
            try
            {
                stream.WriteRange(vertices.ToArray());
            }
            finally
            {
                buffer.Unlock();
            }
        }

        #endregion
    }
}
