using System;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace VoIP_Server
{
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    public class MyWaveBuffer : IWaveBuffer
    {
        [FieldOffset(0)] public int numberOfBytes;
        [FieldOffset(8)] private byte[] byteBuffer;
        [FieldOffset(8)] private float[] floatBuffer;
        [FieldOffset(8)] private short[] shortBuffer;
        [FieldOffset(8)] private int[] intBuffer;

        public MyWaveBuffer(int sizeToAllocateInBytes)
        {
            int num = sizeToAllocateInBytes % 4;
            sizeToAllocateInBytes = num == 0 ? sizeToAllocateInBytes : sizeToAllocateInBytes + 4 - num;
            this.byteBuffer = new byte[sizeToAllocateInBytes];
            this.numberOfBytes = 0;
        }

        public MyWaveBuffer(byte[] bufferToBoundTo) => this.BindTo(bufferToBoundTo);
        
        public MyWaveBuffer(float[] bufferToBoundTo) => this.BindTo(bufferToBoundTo);

        public void BindTo(byte[] bufferToBoundTo)
        {
            this.byteBuffer = bufferToBoundTo;
            this.numberOfBytes = 0;
        }
        public void BindTo(float[] bufferToBoundTo)
        {
            this.floatBuffer = bufferToBoundTo;
            this.numberOfBytes = 0;
        }

        public static implicit operator byte[](MyWaveBuffer waveBuffer) => waveBuffer.byteBuffer;

        public static implicit operator float[](MyWaveBuffer waveBuffer) => waveBuffer.floatBuffer;

        public static implicit operator int[](MyWaveBuffer waveBuffer) => waveBuffer.intBuffer;

        public static implicit operator short[](MyWaveBuffer waveBuffer) => waveBuffer.shortBuffer;

        public byte[] ByteBuffer => this.byteBuffer;

        public float[] FloatBuffer => this.floatBuffer;

        public short[] ShortBuffer => this.shortBuffer;

        public int[] IntBuffer => this.intBuffer;

        public int MaxSize => this.byteBuffer.Length;

        public int ByteBufferCount
        {
            get => this.numberOfBytes;
            set => this.numberOfBytes = this.CheckValidityCount(nameof(ByteBufferCount), value, 1);
        }

        public int FloatBufferCount
        {
            get => this.numberOfBytes / 4;
            set => this.numberOfBytes = this.CheckValidityCount(nameof(FloatBufferCount), value, 4);
        }

        public int ShortBufferCount
        {
            get => this.numberOfBytes / 2;
            set => this.numberOfBytes = this.CheckValidityCount(nameof(ShortBufferCount), value, 2);
        }

        public int IntBufferCount
        {
            get => this.numberOfBytes / 4;
            set => this.numberOfBytes = this.CheckValidityCount(nameof(IntBufferCount), value, 4);
        }

        public void Clear() => Array.Clear((Array) this.byteBuffer, 0, this.byteBuffer.Length);

        public void Copy(Array destinationArray) =>
            Array.Copy((Array) this.byteBuffer, destinationArray, this.numberOfBytes);

        private int CheckValidityCount(string argName, int value, int sizeOfValue)
        {
            int num = value * sizeOfValue;
            if (num % 4 != 0)
                throw new ArgumentOutOfRangeException(argName,
                    string.Format("{0} cannot set a count ({1}) that is not 4 bytes aligned ", (object) argName,
                        (object) num));
            if (value < 0 || value > this.byteBuffer.Length / sizeOfValue)
                throw new ArgumentOutOfRangeException(argName,
                    string.Format("{0} cannot set a count that exceed max count {1}", (object) argName,
                        (object) (this.byteBuffer.Length / sizeOfValue)));
            return num;
        }
    }
}