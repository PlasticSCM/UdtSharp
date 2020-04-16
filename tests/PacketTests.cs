using NUnit.Framework;
using System;
using UdtSharp;

namespace UdtSharpTests
{
    [TestFixture]
    public class PacketTests
    {
        [Test]
        public void HandshakeTest()
        {
            Handshake handShake = new Handshake();
            handShake.m_iVersion = 100;
            handShake.m_iID = 20;

            byte[] buffer = new byte[Handshake.m_iContentSize];

            handShake.serialize(buffer);

            Handshake handShake2 = new Handshake();
            handShake2.deserialize(buffer, buffer.Length);

            Assert.AreEqual(100, handShake2.m_iVersion);
            Assert.AreEqual(20, handShake2.m_iID);
        }

        [Test]
        public void PacketHeader()
        {
            int seqNo = 1;
            uint msgNo = 2;
            int timeStamp = 3;
            int id = 4;

            Packet packet = new Packet();
            packet.SetSequenceNumber(seqNo);
            packet.SetMessageNumber(msgNo);
            packet.SetTimestamp(timeStamp);
            packet.SetId(id);

            Assert.AreEqual(seqNo, packet.GetSequenceNumber());
            Assert.AreEqual(msgNo, packet.GetMessageNumber());
            Assert.AreEqual(timeStamp, packet.GetTimestamp());
            Assert.AreEqual(id, packet.GetId());

            byte[] header = packet.GetHeaderBytes();
            Assert.AreEqual(seqNo, BitConverter.ToInt32(header, 0));
            Assert.AreEqual(msgNo, BitConverter.ToUInt32(header, 4));
            Assert.AreEqual(timeStamp, BitConverter.ToInt32(header, 8));
            Assert.AreEqual(id, BitConverter.ToInt32(header, 12));

            int newTimeStamp = 100;
            packet.SetTimestamp(newTimeStamp);
            header = packet.GetHeaderBytes();
            Assert.AreEqual(newTimeStamp, BitConverter.ToInt32(header, 8));

            byte[] data = packet.GetDataBytes();
            Assert.IsNull(data);

            int dataLength = 4;
            byte[] dataToSend = { 123, 132, 12, 52 };
            byte[] packetBytes = new byte[Packet.m_iPktHdrSize + dataLength];
            Array.Copy(header, packetBytes, Packet.m_iPktHdrSize);
            Array.Copy(dataToSend, 0, packetBytes, Packet.m_iPktHdrSize, dataLength);

            packet.SetHeaderAndDataFromBytes(packetBytes, packetBytes.Length);
            header = packet.GetHeaderBytes();
            data = packet.GetDataBytes();

            Assert.AreEqual(seqNo, BitConverter.ToInt32(header, 0));
            Assert.AreEqual(msgNo, BitConverter.ToUInt32(header, 4));
            Assert.AreEqual(newTimeStamp, BitConverter.ToInt32(header, 8));
            Assert.AreEqual(id, BitConverter.ToInt32(header, 12));

            CollectionAssert.AreEqual(dataToSend, data);
        }

        [Test]
        public unsafe void PackAck()
        {
            int typeACK = 2;
            int msgNo = 100;
            int* pMsgNo = &msgNo;
            int controlPacket = 1;

            byte[] data = { 1, 2, 3, 4 };
            fixed (byte* pData = data)
            {
                Packet packet = new Packet();
                packet.pack(typeACK, (void*)pMsgNo, (void*)pData, data.Length);

                Assert.AreEqual(controlPacket, packet.getFlag());
                Assert.AreEqual(typeACK, packet.getType());
                Assert.AreEqual(msgNo, packet.getAckSeqNo());

                iovec[] packetVector = packet.getPacketVector();
                Assert.AreEqual(2, packetVector.Length);

                iovec packetData = packetVector[1];
                Assert.AreEqual(data.Length, packetData.iov_len);
                uint iov = packetData.iov_base[0];

                uint expectedResult = 1 + (2 << 8) + (3 << 16) + (4 << 24);
                Assert.AreEqual(expectedResult, iov);
            }
        }

        [Test]
        public unsafe void PackAckAck()
        {
            int typeACKACK = 6;
            int msgNo = 100;
            int* pMsgNo = &msgNo;
            int controlPacket = 1;

            byte[] data = { 1, 2, 3, 4 };
            fixed (byte* pData = data)
            {
                Packet packet = new Packet();
                packet.pack(typeACKACK, (void*)pMsgNo, (void*)null, data.Length);

                Assert.AreEqual(controlPacket, packet.getFlag());
                Assert.AreEqual(typeACKACK, packet.getType());
                Assert.AreEqual(msgNo, packet.getAckSeqNo());

                iovec[] packetVector = packet.getPacketVector();
                Assert.AreEqual(2, packetVector.Length);

                iovec packetData = packetVector[1];
                Assert.IsNull(packetData.iov_base);
            }
        }

        [Test]
        public unsafe void PackError()
        {
            int typeError = 8;
            int msgNo = 100;
            int* pMsgNo = &msgNo;
            int controlPacket = 1;

            byte[] data = { 1, 2, 3, 4 };
            fixed (byte* pData = data)
            {
                Packet packet = new Packet();
                packet.pack(typeError, (void*)pMsgNo, (void*)pData, data.Length);

                Assert.AreEqual(controlPacket, packet.getFlag());
                Assert.AreEqual(typeError, packet.getType());
                Assert.AreEqual(msgNo, packet.getMsgSeq());

                iovec[] packetVector = packet.getPacketVector();
                Assert.AreEqual(2, packetVector.Length);

                iovec packetData = packetVector[1];
                Assert.IsNull(packetData.iov_base);
            }
        }

        [Test]
        public unsafe void PackOverloads()
        {
            byte[] data = { 1, 2, 3, 4 };
            int[] intData = new int[12];
            int intlparam = 200;
            uint lparam = 100;

            Packet packet = new Packet();
            packet.pack(0, data);
            packet.pack(1);
            packet.pack(2, intlparam, intData);
            packet.pack(2, intlparam, intData, 2);
            packet.pack(3, intData, 2);
            packet.pack(4);
            packet.pack(5);
            packet.pack(6, &lparam);
            packet.pack(8, &lparam);
        }


    }
}
