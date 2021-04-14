using System;
using System.Collections.Generic;
using NAudio.Utils;
using NAudio.Wave;

namespace VoIP_Server
{
  public class MyBufferedSampleProvider
  {
    private MyCircularBuffer circularBuffer;
    private readonly WaveFormat waveFormat;

    public MyBufferedSampleProvider(WaveFormat waveFormat)
    {
      this.waveFormat = waveFormat;
      this.BufferLength = (waveFormat.AverageBytesPerSecond / 4) * 5;
      this.circularBuffer = new MyCircularBuffer(this.BufferLength);
      this.ReadFully = true;
      FloatPerRead = waveFormat.AverageBytesPerSecond / 20;
    }

    public bool ReadFully { get; set; }

    public int BufferLength { get; set; }

    public bool DiscardOnBufferOverflow { get; set; }

   
    public WaveFormat WaveFormat => this.waveFormat;

    public void AddSamples(float[] buffer, int offset, int count)
    {
      if (this.circularBuffer == null)
        this.circularBuffer = new MyCircularBuffer(this.BufferLength);
      circularBuffer.Write(buffer, offset, count);
      if (circularBuffer.Count > FloatPerRead * 5)
        ToRead = true;
    }

    public bool ToRead { get; set; }
    
    public int FloatPerRead { get; set; }

    public int Read(float[] buffer, int offset, int count)
    {
      int num = 0;
      if (this.circularBuffer != null)
      {
        num = this.circularBuffer.Read(buffer, offset, count);
        if (circularBuffer.Count == 0)
          ToRead = false;
      }
      if (this.ReadFully && num < count)
      {
        Array.Clear(buffer, offset + num, count - num);
        num = count;
      }

      return num;
    }

    public void ClearBuffer()
    {
      if (this.circularBuffer == null)
        return;
      this.circularBuffer.Reset();
    }
  }
}
