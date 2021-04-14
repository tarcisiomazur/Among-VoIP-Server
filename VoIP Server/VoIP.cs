
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using NAudio.Wave;
using NetCoreServer;
using Buffer = System.Buffer;

namespace VoIP_Server
{
    public static class VoIP
    {
        public static readonly WaveFormat MyFormat = new WaveFormat(44100, 16, 2);
    }
    public class JoinStream : MyBufferedSampleProvider
    {
        public void AddSamples((float[], long, long) v)
        {
            base.AddSamples(v.Item1, (int) v.Item2, (int) v.Item3);
        }

        public WaveFormat WaveFormat { get; }

        public JoinStream() : base(VoIP.MyFormat)
        {
            DiscardOnBufferOverflow = true;
        }
        public JoinStream(WaveFormat waveBuffer) : base(waveBuffer)
        {
            DiscardOnBufferOverflow = true;
        }
    }
    
    public class Game
    {

        public static Dictionary<string,Game> games = new Dictionary<string,Game>();

        public AmongSession[] Sessions;
        public IList<JoinStream> InputStream;
        public int[] Buffers;
        public int ActiveSessions;
        public MixingProvider Output;
        public float[] Buffer;
        private int SizeBuffer = 8820;
         
        private bool exit;
        private Thread run;

        private string GameCode, GameName;

        private void VoIPThread(object o)
        {
            Buffer = new float[SizeBuffer];
            var go = o as Game;
            
            var last = DateTime.Now.Ticks;
            long time;
            int delay;
            var ticksPerLoop = TimeSpan.TicksPerMillisecond * 100;
            while (!exit)
            {
                time = DateTime.Now.Ticks;
                delay = (int) ((ticksPerLoop + last - time) / 10000);
                if (delay>0) Thread.Sleep(delay);
                last += ticksPerLoop;
                var read = Output.Read(Buffer);
                for (var i = 0; i < 10; i++)
                {
                    if (Sessions[i] != null && Sessions[i].IsConnected)
                    {
                        Sessions[i].SendTextAsync(read[i].ByteArray, 0, read[i].ByteArray.Length);
                    }
                }
                
            }
        }

        private Game(string gameCode, string gameName = "")
        {
            GameCode = gameCode;
            GameName = gameName;
            Sessions = new AmongSession[10];
            InputStream = new JoinStream[10];
            Buffers = new int [10];
            Output = new MixingProvider(InputStream, 10);
            
            run = new Thread(VoIPThread);
            games.Add(gameCode, this);
            run.Start(exit);
        }

        public static Game GetGame(string gameCode)
        {
            return games.ContainsKey(gameCode) ? games[gameCode] : new Game(gameCode);
        }

        public void setSession(int i, AmongSession amongSession)
        {
            Sessions[i] = amongSession;
            ActiveSessions++;
            InputStream[i] = new JoinStream();
        }

        public void removeSession(int i, AmongSession amongSession)
        {
            Sessions[i] = amongSession;
            ActiveSessions--;
            InputStream[i] = null;
            if (ActiveSessions == 0)
            {
                exit = true;
                games.Remove(GameCode);
            }
        }
    }
    public class AmongSession : WsSession
    {
        private static List<AmongSession> AllSessions = new List<AmongSession>();
        private Game game;
        
        public AmongSession(WsServer server) : base(server)
        {
            AllSessions.Add(this);
        }

        public override void OnWsConnected(HttpRequest request)
        {
            Console.WriteLine($"AmongSession WebSocket session with Id {Id} connected!");
            Console.WriteLine(request);
            for(var i=0; i<request.Headers; i++)
            {
                var (item1, item2) = request.Header(i);
                if (item1 != "Room") continue;
                var cea = JsonSerializer.Deserialize<ConnectEventArgs>(item2);
                game = Game.GetGame(cea.GameCode);
                game.setSession(cea.ID, this);
            }
        }

        
        public override void OnWsDisconnected()
        {
            game.removeSession(0, this);
            Console.WriteLine($"AmongSession WebSocket session with Id {Id} disconnected!");
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            if (size == 35292 && game != null)
            {
                AmongMessage.ProcessMessage(game, buffer, offset, size);
            }
            else
            {
                Console.WriteLine($"Discart packet with {size} bytes" );
            }
        }

        public override void OnWsError(SocketError error)
        {
            Console.WriteLine($"Chat WebSocket session caught an error with code {error}");
            base.OnWsError(error);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat WebSocket session caught an error with code {error}");
        }

        public void Destruct()
        {
            Console.WriteLine("Destroy");
            AllSessions.Remove(this);
        }
    }

    internal class AmongServer : WsServer
    {
        public AmongServer(IPAddress address, int port) : base(address, port) { }

        protected override TcpSession CreateSession() { return new AmongSession(this); }

        protected override void OnDisconnected(TcpSession session)
        {
            base.OnDisconnected(session);
            if (session is AmongSession ass)
            {
                ass.Destruct();
            }
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat WebSocket server caught an error with code {error}");
        }
    }
}