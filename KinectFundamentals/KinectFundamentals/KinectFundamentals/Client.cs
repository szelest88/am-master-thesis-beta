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

            connected = true;

            readThread = new Thread(new ThreadStart(Read));
            readThread.Start();
        }

        public void Read()
        {
            stream = new FileStream(handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] readBuf = new byte[BUFFER_SIZE];
            ASCIIEncoding e = new ASCIIEncoding();
            while (true)
            {
                int bytesRead = 0;

                try
                {
                    bytesRead = stream.Read(readBuf, 0, BUFFER_SIZE);
                }
                catch
                {break;}

                if (bytesRead == 0)
                    break;

                if (MessageReceived != null)
                    MessageReceived(e.GetString(readBuf, 0, bytesRead));

            }

            stream.Close();
            handle.Close();
        }

        public void SendMessage(string message)
        {
            ASCIIEncoding e = new ASCIIEncoding();
            byte[] buf = e.GetBytes(message);
            stream.Write(buf, 0, buf.Length);
            stream.Flush();
        }

        public void SendMessageBS(BodyState message)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, message);
            byte[] messageBuf = ms.GetBuffer();
                
            this.stream.Write(messageBuf, 0, messageBuf.Length);
            this.stream.Flush();
        }
    }

}
