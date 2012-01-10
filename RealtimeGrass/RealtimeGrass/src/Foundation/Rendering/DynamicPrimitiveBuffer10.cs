using System;
using System.Collections.Generic;

using D3D = SlimDX.Direct3D10;
using SlimDX;

namespace RealtimeGrass.Rendering
{
    /// <summary>
    /// An automatically-resizing buffer of primitive data, implemented using Direct3D10.
    /// </summary>
    public class DynamicPrimitiveBuffer10<T> : DynamicPrimitiveBuffer<T> where T : struct
    {
        #region Public Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPrimitiveBuffer10"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public DynamicPrimitiveBuffer10(D3D.Device device)
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
        internal D3D.Buffer UnderlyingBuffer
        {
            get
            {
                return buffer;
            }
        }

        #endregion
        #region Implementation Detail

        D3D.Device device;
        D3D.Buffer buffer;

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

            buffer = new D3D.Buffer(device, new D3D.BufferDescription
            {
                BindFlags = D3D.BindFlags.VertexBuffer,
                CpuAccessFlags = D3D.CpuAccessFlags.Write,
                OptionFlags = D3D.ResourceOptionFlags.None,
                SizeInBytes = sizeInBytes,
                Usage = D3D.ResourceUsage.Dynamic
            });
        }

        /// <summary>
        /// In a derived class, implements logic to fill the buffer with vertex data.
        /// </summary>
        /// <param name="vertices">The vertex data.</param>
        protected override void FillBuffer(List<T> vertices)
        {
            DataStream stream = buffer.Map(D3D.MapMode.WriteDiscard, D3D.MapFlags.None);
            try
            {
                stream.WriteRange(vertices.ToArray());
            }
            finally
            {
                buffer.Unmap();
            }
        }

        #endregion
    }
}
