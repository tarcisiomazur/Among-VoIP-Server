using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace VoIP_Server
{
    public class MixingProvider
    {
        private readonly IList<JoinStream> inputs;
        private readonly WaveFormat waveFormat;
        private readonly int outputs;
        private readonly bool[,] mappings;
        private float[] inputBuffer;
        private int BytesPerSend;
        private int SendsPerSecond;
        public float[] receiveTest;
        public float[] sendTest;
        
        private List<VoipMessage> ret = new List<VoipMessage>();

        public MixingProvider(IList<JoinStream> inputs,
            int outputs,WaveFormat waveFormat = null)
        {
            this.outputs = outputs;
            this.waveFormat = waveFormat ?? VoIP.MyFormat;
            this.inputs = inputs;
            mappings = new bool[this.inputs.Count, outputs];
            SendsPerSecond = 10;
            BytesPerSend = this.waveFormat.AverageBytesPerSecond / SendsPerSecond;
            receiveTest = new float[8820];
            sendTest = new float[8820];
            for (var i = 0; i < outputs; i++)
            {
                ret.Add(new VoipMessage(BytesPerSend*2));
            }
        }

        public WaveFormat WaveFormat => this.waveFormat;

        public bool this[int input, int output]
        {
            get => mappings[input, output];
            set => mappings[input, output] = value;
        }

        public List<VoipMessage> Read(float[] buffer)
        {
            var size = buffer.Length;
            var i = -1;
            foreach (var msg in ret)
            {
                var arr = msg.FloatArray;
                var end = msg.FloatOffset + msg.FloatSize;
                msg.ListenerCount = 0;
                for (var y = msg.FloatOffset; y < end; y++)
                {
                    arr[y] = 0;
                }
                msg.Send = false;
            }
            foreach (var input in inputs)
            {
                i++;
                if (input == null || !input.ToRead) continue;
                var readed = input.Read(buffer, 0, size);
                for (var o = 0; o < outputs; o++)
                {
                    if (!mappings[i,o] || inputs[o] == null) continue;
                    float sum=0;
                    ret[o].AddListener((byte) i);
                    var myOutput = ret[o].FloatArray;
                    int j, k;
                    for (j = 0, k = ret[o].FloatOffset; j < readed; j++, k++)
                    {
                        myOutput[k] += buffer[j];
                        sum += buffer[j];
                    }

                    Console.WriteLine(sum);
                    while (k < ret[o].FloatSize+ret[o].FloatOffset)
                    {
                        myOutput[k++] = 0;
                    }
                }

            }

            return ret;
        }

        public void SetOutputs(int input, byte[] msgListeners)
        {
            var l = new bool[10];
            foreach (var listener in msgListeners)
            {
                l[listener] = true;
            }
            for (var i = 0; i < 10; i++)
            {
                if (mappings[input, i] != l[i])
                {
                    Console.WriteLine($"Agora o player {i} {(l[i]?"está":"não está")} ouvindo {input}");
                    mappings[input, i] = l[i];
                }
                
            };
            
        }
    }
}