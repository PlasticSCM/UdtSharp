using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace UdtPerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: udt|tcp client upload|download|integrity server_address port sizeInMB");
                Console.WriteLine("Usage: udt|tcp server port");
                return;
            }

            int ini = Environment.TickCount;


            try
            {
                switch (args[0])
                {
                    case "udt":
                        if (args[1] == "client")
                        {
                            UdtTest.Client.Run(ParseClientArgs(args));
                            return;
                        }

                        if (args[1] == "server")
                        {
                            UdtTest.Server.Run(Convert.ToInt32(args[2]));
                            return;
                        }

                        break;
                    case "tcp":
                        if (args[1] == "client")
                        {
                            TcpTest.Client.Run(ParseClientArgs(args));
                            return;
                        }

                        if (args[1] == "server")
                        {
                            TcpTest.Server.Run(Convert.ToInt32(args[2]));
                            return;
                        }
                        break;
                }
            }
            finally
            {
                Console.WriteLine("Total time: {0} ms", Environment.TickCount - ini);
            }
        }

        class ClientArgs
        {
            internal TestType TestType;
            internal IPAddress ServerAddress;
            internal int ServerPort;
            internal int SizeInMb;
        }

        static ClientArgs ParseClientArgs(string[] args)
        {
            ClientArgs result = new ClientArgs();

            string testType = args[2];
            if (testType == "upload")
                result.TestType = TestType.Upload;
            else if (testType == "download")
                result.TestType = TestType.Download;
            else
                result.TestType = TestType.Integrity;
            result.ServerAddress = IPAddress.Parse(args[3]);
            result.ServerPort = Convert.ToInt32(args[4]);
            result.SizeInMb = Convert.ToInt32(args[5]);

            return result;
        }

        internal interface ISocket
        {
            int Send(byte[] buffer, int offset, int size);
            int Receive(byte[] buffer, int offset, int size);
        }

        enum TestType
        {
            Upload = 0,
            Download = 1,
            Integrity = 2,
        }

        static class ClientTest
        {
            internal static void Upload(ISocket socket, int sizeInMb)
            {
                byte[] data = DataBlock.Create(sizeInMb * 1024 * 1024);

                for (int i = 0; i < 5; ++i)
                {
                    byte[] request = CreateRequest(TestType.Upload, sizeInMb);

                    socket.Send(request, 0, request.Length);

                    int ini = Environment.TickCount;

                    int sent = 0;
                    while (sent < data.Length)
                    {
                        int bytesSent = socket.Send(data, sent, data.Length - sent);
                        sent += bytesSent;
                    }

                    if (socket.Receive(request, 0, request.Length) != 5)
                    {
                        Console.WriteLine("Error in Upload ACK");
                    }

                    Console.WriteLine("{0}: {1} MB in {2, 5} ms. {3:0.00} mbps",
                        i, sizeInMb, Environment.TickCount - ini,
                        sizeInMb * 8.0f / ((float)(Environment.TickCount - ini) / 1000.0f));
                }
            }

            internal static void Download(ISocket socket, int sizeInMb)
            {
                byte[] data = new byte[sizeInMb * 1024 * 1024];

                for (int i = 0; i < 5; ++i)
                {
                    byte[] request = CreateRequest(TestType.Download, sizeInMb);

                    socket.Send(request, 0, request.Length);

                    int ini = Environment.TickCount;

                    int recv = 0;
                    while (recv < data.Length)
                    {
                        recv += socket.Receive(data, recv, data.Length - recv);
                    }

                    socket.Receive(request, 0, request.Length);

                    Console.WriteLine("{0}: {1} MB in {2, 5} ms. {3:0.00} mbps",
                        i, sizeInMb, Environment.TickCount - ini,
                        sizeInMb * 8.0f / ((float)(Environment.TickCount - ini) / 1000.0f));
                }
            }

            internal static void Integrity(ISocket socket, int sizeInMb)
            {
                byte[] data = DataBlock.Create(sizeInMb * 1024 * 1024, true);

                byte[] request = CreateRequest(TestType.Integrity, sizeInMb);

                socket.Send(request, 0, request.Length);

                int ini = Environment.TickCount;

                int sent = 0;
                while (sent < data.Length)
                {
                    int bytesSent = socket.Send(data, sent, data.Length - sent);
                    sent += bytesSent;
                }

                if (socket.Receive(request, 0, request.Length) != 5)
                {
                    Console.WriteLine("Error in Upload ACK");
                }

                Console.WriteLine("Upload: {0} MB in {1, 5} ms. {2:0.00} mbps",
                    sizeInMb, Environment.TickCount - ini,
                    sizeInMb * 8.0f / ((float)(Environment.TickCount - ini) / 1000.0f));

                ini = Environment.TickCount;

                byte[] recvData = new byte[data.Length];

                int recv = 0;
                while (recv < data.Length)
                {
                    recv += socket.Receive(recvData, recv, recvData.Length - recv);
                }

                socket.Receive(request, 0, request.Length);

                Console.WriteLine("Download: {0} MB in {1, 5} ms. {2:0.00} mbps",
                    sizeInMb, Environment.TickCount - ini,
                    sizeInMb * 8.0f / ((float)(Environment.TickCount - ini) / 1000.0f));

                bool dataOk = DataBlock.AreEqual(data, recvData);
                Console.WriteLine("Data " + (dataOk ? "OK!" : "ERROR"));
            }

            static byte[] CreateRequest(TestType testType, int size)
            {
                byte[] result = new byte[5];

                using (MemoryStream stream = new MemoryStream(result))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)testType);
                    writer.Write((int)size);
                    writer.Flush();
                }

                return result;
            }
        }

        static class ServerTest
        {
            internal static void Run(ISocket socket)
            {
                while (true)
                {
                    byte[] request = new byte[5];
                    try
                    {
                        int received = socket.Receive(request, 0, request.Length);
                        if (received == 0)
                            break;
                    }
                    catch (Exception ex)
                    {
                        // socket closed, probably
                        break;
                    }

                    TestType testType;
                    int sizeInMb;

                    ParseRequest(request, out testType, out sizeInMb);

                    if (testType == TestType.Upload) Console.Write("Upload Test");
                    if (testType == TestType.Download) Console.Write("Download Test");
                    if (testType == TestType.Integrity) Console.Write("Integrity Test");

                    Console.WriteLine(". {0} MB", sizeInMb);

                    switch (testType)
                    {
                        case TestType.Upload:
                            UploadTest(socket, sizeInMb);
                            break;
                        case TestType.Download:
                            DownloadTest(socket, sizeInMb);
                            break;
                        case TestType.Integrity:
                            IntegrityTest(socket, sizeInMb);
                            break;
                    }
                }

                Console.WriteLine("Test finished");
            }

            static void UploadTest(ISocket socket, int sizeInMb)
            {
                byte[] data = new byte[sizeInMb * 1024 * 1024];

                int read = 0;
                while (read < data.Length)
                {
                    read += socket.Receive(data, read, data.Length - read);
                }

                socket.Send(new byte[5], 0, 5);
            }

            static void DownloadTest(ISocket socket, int sizeInMb)
            {
                byte[] data = DataBlock.Create(sizeInMb * 1024 * 1024);

                int sent = 0;
                while (sent < data.Length)
                {
                    sent += socket.Send(data, sent, data.Length - sent);
                }

                socket.Send(new byte[5], 0, 5);
            }

            static void IntegrityTest(ISocket socket, int sizeInMb)
            {
                byte[] data = new byte[sizeInMb * 1024 * 1024];

                int read = 0;
                while (read < data.Length)
                {
                    read += socket.Receive(data, read, data.Length - read);
                }

                socket.Send(new byte[5], 0, 5);

                int sent = 0;
                while (sent < data.Length)
                {
                    sent += socket.Send(data, sent, data.Length - sent);
                }

                socket.Send(new byte[5], 0, 5);
            }


            static void ParseRequest(byte[] request, out TestType testType, out int sizeInMb)
            {
                using (MemoryStream stream = new MemoryStream(request))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    testType = (TestType)reader.ReadByte();
                    sizeInMb = reader.ReadInt32();
                }
            }
        }

        static class UdtTest
        {
            class UdtSocket : ISocket
            {
                internal UdtSocket(UdtSharp.UdtSocket s)
                {
                    mSocket = s;
                }

                int ISocket.Receive(byte[] buffer, int offset, int size)
                {
                    return mSocket.Receive(buffer, offset, size);
                }

                int ISocket.Send(byte[] buffer, int offset, int size)
                {
                    return mSocket.Send(buffer, offset, size);
                }

                UdtSharp.UdtSocket mSocket;
            }

            internal static class Client
            {
                internal static void Run(ClientArgs args)
                {
                    UdtSharp.UdtSocket client = new UdtSharp.UdtSocket(AddressFamily.InterNetwork, SocketType.Stream);

                    client.Connect(new IPEndPoint(args.ServerAddress, args.ServerPort));
                    try
                    {
                        switch (args.TestType)
                        {
                            case TestType.Upload:
                            ClientTest.Upload(new UdtSocket(client), args.SizeInMb);
                                break;
                            case TestType.Download:
                            ClientTest.Download(new UdtSocket(client), args.SizeInMb);
                                break;
                            case TestType.Integrity:
                                ClientTest.Integrity(new UdtSocket(client), args.SizeInMb);
                                break;
                        }
                    }
                    finally
                    {
                        client.Close();
                    }
                }
            }

            internal static class Server
            {
                internal static void Run(int port)
                {
                    UdtSharp.UdtSocket server = new UdtSharp.UdtSocket(AddressFamily.InterNetwork, SocketType.Stream);

                    server.Bind(new IPEndPoint(IPAddress.Any, port));

                    server.Listen(5);

                    while (true)
                    {
                        UdtSharp.UdtSocket serverSocket = server.Accept();
                        if (serverSocket == null)
                            break;

                        ServerTest.Run(new UdtSocket(serverSocket));

                        serverSocket.Close();
                    }

                    server.Close();
                }
            }
        }

        static class TcpTest
        {
            class TcpSocket : ISocket
            {
                internal TcpSocket(Socket s)
                {
                    mSocket = s;
                }

                int ISocket.Receive(byte[] buffer, int offset, int size)
                {
                    return mSocket.Receive(buffer, offset, size, SocketFlags.None);
                }

                int ISocket.Send(byte[] buffer, int offset, int size)
                {
                    return mSocket.Send(buffer, offset, size, SocketFlags.None);
                }

                Socket mSocket;
            }

            internal static class Client
            {
                internal static void Run(ClientArgs args)
                {
                    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    client.Connect(new IPEndPoint(args.ServerAddress, args.ServerPort));

                    try
                    {
                        switch (args.TestType)
                        {
                            case TestType.Upload:
                                ClientTest.Upload(new TcpSocket(client), args.SizeInMb);
                                break;
                            case TestType.Download:
                                ClientTest.Download(new TcpSocket(client), args.SizeInMb);
                                break;
                            case TestType.Integrity:
                                ClientTest.Integrity(new TcpSocket(client), args.SizeInMb);
                                break;
                        }
                    }
                    finally
                    {
                        client.Close();
                    }
                }
            }

            internal static class Server
            {
                internal static void Run(int serverPort)
                {
                    Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    server.Bind(new IPEndPoint(IPAddress.Any, serverPort));

                    server.Listen(5);

                    while (true)
                    {
                        Socket client = server.Accept();
                        if (client == null)
                            break;

                        ServerTest.Run(new TcpSocket(client));

                        client.Close();
                    }

                    server.Close();
                }
            }
        }
    }

    public static class DataBlock
    {
        static Random mRandom = new Random();

        public static byte[] Create(int length, bool bRandom = false)
        {
            byte[] bytes = new byte[length];
            if (bRandom)
                mRandom.NextBytes(bytes);
            return bytes;
        }

        public static bool AreEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
                return false;

            for (int i = 0; i < left.Length; ++i)
            {
                if (left[i] != right[i])
                    return false;
            }

            return true;
        }
    }
}
