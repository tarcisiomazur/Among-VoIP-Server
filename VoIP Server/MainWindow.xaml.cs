

using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;

namespace VoIP_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            AllocConsole();
            
            int port = 8124;

            Console.WriteLine($"WebSocket server port: {port}");
            Console.WriteLine($"WebSocket server website: http://localhost:{port}/chat/index.html");

            Console.WriteLine();

            // Create a new WebSocket server
            var server = new AmongServer(IPAddress.Any, port);
            Console.Write("Server starting...");
            server.Start();
            //server.AddStaticContent("C:\\Users\\tarci\\Downloads", "/chat");

            Console.WriteLine("Done!");

            InitializeComponent();
        }
        
        [DllImport("kernel32.dll",  
            EntryPoint = "AllocConsole",  
            SetLastError = true,  
            CharSet = CharSet.Auto,  
            CallingConvention = CallingConvention.StdCall)]  
        private static extern int AllocConsole();  
    }
}