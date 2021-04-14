using System;
using System.Linq;

namespace VoIP_Server
{
    public class MyCircularBuffer
    {
        public readonly float[] buffer;
        public readonly object lockObject;
        private int writePosition;
        private int readPosition;
        private int floatCount;

        public MyCircularBuffer(int size)
        {
            buffer = new float[size];
            lockObject = new object();
        }

        public int Write(float[] data, int offset, int count)
        {
            lock (lockObject)
            {
                count = Math.Min(count, buffer.Length);
                var right = Math.Min(buffer.Length - writePosition, count);
                Buffer.BlockCopy(data, offset*4, buffer, writePosition*4, right*4);
                writePosition = (writePosition + right) % buffer.Length;
                var left = count - right;
                
                if (left > 0)
                {
                    Buffer.BlockCopy(data, (offset + right)*4, buffer, writePosition*4, left*4);
                    writePosition += left;
                }
                floatCount += count;
                if (floatCount > buffer.Length)
                {
                    readPosition = writePosition;
                    floatCount = buffer.Length;
                }
                
                return count;
            }
        }

        public int Read(float[] data, int offset, int count)
        {
            lock (this.lockObject)
            {
                if (count > this.floatCount)
                    count = this.floatCount;
                int num1 = 0;
                int length = Math.Min(this.buffer.Length - this.readPosition, count);
                Buffer.BlockCopy(buffer, readPosition*4,  data, offset*4, length*4);
                int num2 = num1 + length;
                this.readPosition += length;
                this.readPosition %= this.buffer.Length;
                if (num2 < count)
                {
                    Buffer.BlockCopy(buffer, readPosition*4, data, (offset + num2)*4, (count - num2)*4);
                    this.readPosition += count - num2;
                    num2 = count;
                }
                this.floatCount -= num2;
                return num2;
            }
        }
        
        public int Read(byte[] data, int offset, int ByteCount)
        {
            byte[] b;
            int i;
            lock (lockObject)
            {
                for (i = offset; i < ByteCount && floatCount>0; i+=2, floatCount--, readPosition = (readPosition + 1) % MaxLength)
                {
                    b = BitConverter.GetBytes((short) (buffer[readPosition]*32768f));
                    data[i] = b[0];
                    data[i + 1] = b[1];
                }
            } 
            return i*2;
        }

        public int MaxLength => this.buffer.Length;

        public int Count
        {
            get
            {
                lock (this.lockObject)
                    return this.floatCount;
            }
        }

        public void Reset()
        {
            lock (this.lockObject)
                this.ResetInner();
        }

        private void ResetInner()
        {
            this.floatCount = 0;
            this.readPosition = 0;
            this.writePosition = 0;
        }

        public void Advance(int count)
        {
            lock (this.lockObject)
            {
                if (count >= this.floatCount)
                {
                    this.ResetInner();
                }
                else
                {
                    this.floatCount -= count;
                    this.readPosition += count;
                    this.readPosition %= this.MaxLength;
                }
            }
        }
    }
}