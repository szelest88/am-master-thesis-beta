﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
    class Server
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(
            String pipeName,
            uint dwOpenMode,
            uint dwPipeMode,
            uint nMaxInstances,
            uint nOutBufferSize,
            uint nInBufferSize,
            uint nDefaultTimeOut,
            IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(
            SafeFileHandle hNamedPipe,
            IntPtr lpOverlapped);

        public const uint DUPLEX = (0x00000003);
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public class Client
        {
            public SafeFileHandle handle;
            public FileStream stream;
        }

        public delegate void MessageReceivedHandler(Client client, string message);

        public event MessageReceivedHandler MessageReceived;
        public const int BUFFER_SIZE = 4096;

        string pipeName;
        public Thread listenThread;
        bool running;
        List<Client> clients;
        public Thread readThread;

        public string PipeName
        {
            get { return this.pipeName; }
            set { this.pipeName = value; }
        }

        public bool Running
        {
            get { return this.running; }
        }

        public Server()
        {
            this.clients = new List<Client>();
        }

        private void ListenForClients()
        {
            while (true)
            {
                SafeFileHandle clientHandle =
                    CreateNamedPipe(
                    this.pipeName,
                    DUPLEX | FILE_FLAG_OVERLAPPED,
                    0,
                    255,
                    BUFFER_SIZE,
                    BUFFER_SIZE,
                    0,
                    IntPtr.Zero
                    );
                if (clientHandle.IsInvalid)
                    return;
                int success = ConnectNamedPipe(clientHandle, IntPtr.Zero);

                if (success == 0)
                    return;

                Client client = new Client();
                client.handle = clientHandle;

                lock (clients)
                    this.clients.Add(client);

                readThread = new Thread(new ParameterizedThreadStart(Read));
                readThread.IsBackground = true;
                readThread.Start(client);
            }
        }

        public void Start()
        {
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
            listenThread.IsBackground = true;
            this.running = true;
        }

        private void Read(object clientObj)
        {
            Client client = (Client)clientObj;
            client.stream = new FileStream(client.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] buffer = new byte[BUFFER_SIZE];
            ASCIIEncoding encoder = new ASCIIEncoding();

            while (true)
            {
                int bytesRead = 0;

                try
                {
                    bytesRead = client.stream.Read(buffer, 0, BUFFER_SIZE);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                    break;

                if (this.MessageReceived != null)
                    this.MessageReceived(client, encoder.GetString(buffer, 0, bytesRead));

            }
            client.stream.Close();
            client.handle.Close();
            lock (this.clients)
                this.clients.Remove(client);
        }

        public void SendMessage(string message)
        {
            lock (this.clients)
            {
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] messageBuffer = encoder.GetBytes(message);
                foreach (Client client in this.clients)
                {
                    client.stream.Write(messageBuffer, 0, messageBuffer.Length);
                    client.stream.Flush();
                }
            }
        }
        

        

    }
}
