using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace KinectFundamentals
{

    class Client
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            String pipeName,
            uint dwDesiredAccess,
            uint dwSharedMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplate);

        public const uint GENERIC_READ = (0x80000000);
        public const uint GENERIC_WRITE = (0x40000000);
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public delegate void MessageReceivedHandler(string message);
        public event MessageReceivedHandler MessageReceived;


        //public delegate void MessageReceivedHandler(BodyState message);
        //public event MessageReceivedHandler MessageReceived2;
        public const int BUFFER_SIZE = 4096;

        string pipeName;
        private FileStream stream;
        private SafeFileHandle handle;
        public Thread readThread;
        bool connected;

        public bool Connected
        {
            get { return this.connected; }
        }
        public string PipeName
        {
            get { return this.pipeName; }
            set { this.pipeName = value; }
        }

        public void Connect()
        {
            this.handle =
                CreateFile(
                this.pipeName,
                GENERIC_READ | GENERIC_WRITE,
                0,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_FLAG_OVERLAPPED,
                IntPtr.Zero
                );

            if (this.handle.IsInvalid)
                return;

            this.connected = true;

            this.readThread = new Thread(new ThreadStart(Read));
            this.readThread.Start();
        }

        public void Read()
        {
            this.stream = new FileStream(this.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] readBuffer = new byte[BUFFER_SIZE];
            ASCIIEncoding encoder = new ASCIIEncoding();
            while (true)
            {
                int bytesRead = 0;

                try
                {
                    bytesRead = this.stream.Read(readBuffer, 0, BUFFER_SIZE);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                    break;

                if (this.MessageReceived != null)
                    this.MessageReceived(encoder.GetString(readBuffer, 0, bytesRead));
                //                if (this.MessageReceived != null)
                //                {
                //                      MemoryStream memStream = new MemoryStream();
                //BinaryFormatter binForm = new BinaryFormatter();
                //memStream.Write(readBuffer, 0, readBuffer.Length);
                //memStream.Seek(0, SeekOrigin.Begin);
                //Object obj = (Object) binForm.Deserialize(memStream);

                //BodyState bs = (BodyState)obj;
                //                  //  this.MessageReceived(encoder.GetString(readBuffer, 0, bytesRead));
                //                }
            }

            this.stream.Close();
            this.handle.Close();
        }

        public void SendMessage(string message)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] messageBuffer = encoder.GetBytes(message);
            this.stream.Write(messageBuffer, 0, messageBuffer.Length);
            this.stream.Flush();
        }

        public void SendMessageBS(BodyState message)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, message);
            byte[] messageBuffer = ms.GetBuffer();
                
            this.stream.Write(messageBuffer, 0, messageBuffer.Length);
            this.stream.Flush();
        }
    }

}
