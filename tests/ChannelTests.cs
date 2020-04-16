using NUnit.Framework;

using UdtSharp;

using System;
using System.Net;
using System.Net.Sockets;

namespace UdtSharpTests
{
    [TestFixture]
    public class ChannelTests
    {
        [Test]
        public void Open()
        {
            IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });
            Assert.AreEqual(AddressFamily.InterNetwork, address.AddressFamily);

            IPEndPoint endPoint = new IPEndPoint(address, 8080);

            Channel channel = new Channel();
            channel.open(endPoint);
            channel.close();

            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            channel.open(udpSocket);
            channel.close();
        }

        [Test]
        public void SendEmptyPacket()
        {
            IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });
            Assert.AreEqual(AddressFamily.InterNetwork, address.AddressFamily);

            IPEndPoint localEndPoint = new IPEndPoint(address, 9000);
            IPEndPoint remoteEndPoint = new IPEndPoint(address, 9001);

            Packet packetToSend = new Packet();

            Channel recvChannel = new Channel();
            recvChannel.open(remoteEndPoint);

            Channel sendChannel = new Channel();
            sendChannel.open(localEndPoint);
            int bytesSent = sendChannel.sendto(remoteEndPoint, packetToSend);
            Assert.AreEqual(Packet.m_iPktHdrSize + packetToSend.getLength(), bytesSent);

            Packet receivedPacket = new Packet();

            IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
            int bytesReceived = recvChannel.recvfrom(ref sourceEndPoint, receivedPacket);
            sendChannel.close();
            recvChannel.close();

            Assert.AreEqual(receivedPacket.getLength(), bytesReceived);

            Assert.AreEqual(localEndPoint.AddressFamily, sourceEndPoint.AddressFamily);
            Assert.AreEqual(localEndPoint.Address, sourceEndPoint.Address);
            Assert.AreEqual(localEndPoint.Port, sourceEndPoint.Port);

            AssertPacketsEqual(packetToSend, receivedPacket);
        }

        [Test]
        public void SendPacketHeader()
        {
            IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });
            Assert.AreEqual(AddressFamily.InterNetwork, address.AddressFamily);

            IPEndPoint localEndPoint = new IPEndPoint(address, 9000);
            IPEndPoint remoteEndPoint = new IPEndPoint(address, 9001);

            Packet packetToSend = new Packet();
            packetToSend.SetSequenceNumber(1);
            packetToSend.SetMessageNumber(2);
            packetToSend.SetTimestamp(3);
            packetToSend.SetId(4);

            Channel recvChannel = new Channel();
            recvChannel.open(remoteEndPoint);

            Channel sendChannel = new Channel();
            sendChannel.open(localEndPoint);
            int bytesSent = sendChannel.sendto(remoteEndPoint, packetToSend);
            Assert.AreEqual(Packet.m_iPktHdrSize + packetToSend.getLength(), bytesSent);

            Packet receivedPacket = new Packet();

            IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
            int bytesReceived = recvChannel.recvfrom(ref sourceEndPoint, receivedPacket);
            sendChannel.close();
            recvChannel.close();

            Assert.AreEqual(receivedPacket.getLength(), bytesReceived);

            Assert.AreEqual(localEndPoint.AddressFamily, sourceEndPoint.AddressFamily);
            Assert.AreEqual(localEndPoint.Address, sourceEndPoint.Address);
            Assert.AreEqual(localEndPoint.Port, sourceEndPoint.Port);

            AssertPacketsEqual(packetToSend, receivedPacket);
        }

        [Test]
        public void SendPacketWithData()
        {
            IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });
            Assert.AreEqual(AddressFamily.InterNetwork, address.AddressFamily);

            IPEndPoint localEndPoint = new IPEndPoint(address, 9000);
            IPEndPoint remoteEndPoint = new IPEndPoint(address, 9001);

            Packet packetToSend = new Packet();
            packetToSend.SetSequenceNumber(1);
            packetToSend.SetMessageNumber(2);
            packetToSend.SetTimestamp(3);
            packetToSend.SetId(4);

            byte[] dataToSend = { 0, 1, 2, 3, 4, 5, 6, 7 };
            packetToSend.SetDataFromBytes(dataToSend);

            Channel recvChannel = new Channel();
            recvChannel.open(remoteEndPoint);

            Channel sendChannel = new Channel();
            sendChannel.open(localEndPoint);
            int bytesSent = sendChannel.sendto(remoteEndPoint, packetToSend);
            Assert.AreEqual(Packet.m_iPktHdrSize + packetToSend.getLength(), bytesSent);

            Packet receivedPacket = new Packet();
            receivedPacket.setLength(packetToSend.getLength());

            IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
            int bytesReceived = recvChannel.recvfrom(ref sourceEndPoint, receivedPacket);
            sendChannel.close();
            recvChannel.close();

            Assert.AreEqual(receivedPacket.getLength(), bytesReceived);

            Assert.AreEqual(localEndPoint.AddressFamily, sourceEndPoint.AddressFamily);
            Assert.AreEqual(localEndPoint.Address, sourceEndPoint.Address);
            Assert.AreEqual(localEndPoint.Port, sourceEndPoint.Port);

            AssertPacketsEqual(packetToSend, receivedPacket);
        }

        [Test]
        public void SendACK()
        {
            IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });
            Assert.AreEqual(AddressFamily.InterNetwork, address.AddressFamily);

            IPEndPoint localEndPoint = new IPEndPoint(address, 9000);
            IPEndPoint remoteEndPoint = new IPEndPoint(address, 9001);

            // control ACK
            int packageTypeACK = 2;
            int ackSeqNo = 300;
            int[] ackParameters = { 0, 1, 2, 3 };

            Packet packetToSend = new Packet();
            packetToSend.pack(packageTypeACK, ackSeqNo, ackParameters);

            Channel recvChannel = new Channel();
            recvChannel.open(remoteEndPoint);

            Channel sendChannel = new Channel();
            sendChannel.open(localEndPoint);
            int bytesSent = sendChannel.sendto(remoteEndPoint, packetToSend);
            Assert.AreEqual(Packet.m_iPktHdrSize + packetToSend.getLength(), bytesSent);

            Packet receivedPacket = new Packet();
            receivedPacket.setLength(packetToSend.getLength());

            IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
            int bytesReceived = recvChannel.recvfrom(ref sourceEndPoint, receivedPacket);
            sendChannel.close();
            recvChannel.close();

            Assert.AreEqual(receivedPacket.getLength(), bytesReceived);

            Assert.AreEqual(localEndPoint.AddressFamily, sourceEndPoint.AddressFamily);
            Assert.AreEqual(localEndPoint.Address, sourceEndPoint.Address);
            Assert.AreEqual(localEndPoint.Port, sourceEndPoint.Port);

            Assert.AreEqual(ackSeqNo, receivedPacket.getAckSeqNo());
            byte[] data = receivedPacket.GetDataBytes();
            int[] receivedAckParameters = new int[data.Length / 4];
            Buffer.BlockCopy(data, 0, receivedAckParameters, 0, data.Length);

            CollectionAssert.AreEqual(ackParameters, receivedAckParameters);
        }

        [Test]
        public void SendHandshake()
        {
            IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });
            Assert.AreEqual(AddressFamily.InterNetwork, address.AddressFamily);

            IPEndPoint localEndPoint = new IPEndPoint(address, 9000);
            IPEndPoint remoteEndPoint = new IPEndPoint(address, 9001);

            // handshake
            Handshake handshake = new Handshake();
            handshake.m_iVersion = 4;
            handshake.m_iType = SocketType.Dgram;
            handshake.m_iISN = 1;
            handshake.m_iMSS = 2;
            handshake.m_iFlightFlagSize = 20;
            handshake.m_iReqType = 1;
            handshake.m_iID = 12;
            handshake.m_iCookie = 10;
            handshake.m_piPeerIP = new uint[] { 127, 0, 0, 1 };

            byte[] handshakeBytes = new byte[Handshake.m_iContentSize];
            handshake.serialize(handshakeBytes);

            int packageTypeHandshake = 0;

            Packet packetToSend = new Packet();
            packetToSend.pack(packageTypeHandshake, handshakeBytes);

            Channel recvChannel = new Channel();
            recvChannel.open(remoteEndPoint);

            Channel sendChannel = new Channel();
            sendChannel.open(localEndPoint);
            int bytesSent = sendChannel.sendto(remoteEndPoint, packetToSend);
            Assert.AreEqual(Packet.m_iPktHdrSize + packetToSend.getLength(), bytesSent);

            Packet receivedPacket = new Packet();
            receivedPacket.setLength(packetToSend.getLength());

            IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
            int bytesReceived = recvChannel.recvfrom(ref sourceEndPoint, receivedPacket);
            sendChannel.close();
            recvChannel.close();

            Assert.AreEqual(receivedPacket.getLength(), bytesReceived);

            Assert.AreEqual(localEndPoint.AddressFamily, sourceEndPoint.AddressFamily);
            Assert.AreEqual(localEndPoint.Address, sourceEndPoint.Address);
            Assert.AreEqual(localEndPoint.Port, sourceEndPoint.Port);

            byte[] data = receivedPacket.GetDataBytes();
            Assert.AreEqual(Handshake.m_iContentSize, data.Length);

            Handshake receivedHandshake = new Handshake();
            receivedHandshake.deserialize(data, data.Length);

            AssertHandshakesEqual(handshake, receivedHandshake);
        }

        void AssertPacketsEqual(Packet expected, Packet actual)
        {
            Assert.AreEqual(expected.GetSequenceNumber(), actual.GetSequenceNumber());
            Assert.AreEqual(expected.GetMessageNumber(), actual.GetMessageNumber());
            Assert.AreEqual(expected.GetTimestamp(), actual.GetTimestamp());
            Assert.AreEqual(expected.GetId(), actual.GetId());
            CollectionAssert.AreEqual(expected.GetHeaderBytes(), actual.GetHeaderBytes());
            CollectionAssert.AreEqual(expected.GetDataBytes(), actual.GetDataBytes());
            CollectionAssert.AreEqual(expected.GetBytes(), actual.GetBytes());
        }

        void AssertHandshakesEqual(Handshake expected, Handshake actual)
        {
            Assert.AreEqual(expected.m_iVersion, actual.m_iVersion);
            Assert.AreEqual(expected.m_iType, actual.m_iType);
            Assert.AreEqual(expected.m_iISN, actual.m_iISN);
            Assert.AreEqual(expected.m_iMSS, actual.m_iMSS);
            Assert.AreEqual(expected.m_iFlightFlagSize, actual.m_iFlightFlagSize);
            Assert.AreEqual(expected.m_iReqType, actual.m_iReqType);
            Assert.AreEqual(expected.m_iID, actual.m_iID);
            Assert.AreEqual(expected.m_iCookie, actual.m_iCookie);
            CollectionAssert.AreEqual(expected.m_piPeerIP, actual.m_piPeerIP);
        }

    }
}
