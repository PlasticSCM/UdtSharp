using NUnit.Framework;

using UdtSharp;

namespace UdtSharpTests
{
    [TestFixture]
    public class BufferTests
    {
        [Test]
        public void SndBuffer()
        {
            int shortTTL = 0;
            int longTTL = 1000000;
            int bufferSize = 2;
            int maxPacketSize = 5;

            SndBuffer sndBuffer = new SndBuffer(bufferSize, maxPacketSize);
            Assert.AreEqual(0, sndBuffer.getCurrBufSize());

            byte[] readData1 = null;
            byte[] readData2 = null;
            byte[] readData3 = null;
            byte[] readData4 = null;
            byte[] readData5 = null;
            byte[] readData6 = null;
            int[] readBytes = new int[6];
            uint msgNo = 0;
            readBytes[0] = sndBuffer.readData(ref readData1, ref msgNo);
            Assert.AreEqual(0, readBytes[0]);
            Assert.IsNull(readData1);

            // add some data into the buffer
            byte[] data1 = new byte[] { 1, 2, 3, 4 };
            byte[] data2 = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
            byte[] data3 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

            sndBuffer.addBuffer(data1, 0, data1.Length, longTTL);
            Assert.AreEqual(1, sndBuffer.getCurrBufSize());
            readBytes[0] = sndBuffer.readData(ref readData1, ref msgNo);

            Assert.AreEqual(data1.Length, readBytes[0]);
            for (int i = 0; i < data1.Length; ++i)
            {
                Assert.AreEqual(data1[i], readData1[i]);
            }

            sndBuffer.addBuffer(data2, 0, data2.Length, shortTTL);
            Assert.AreEqual(3, sndBuffer.getCurrBufSize());
            readBytes[1] = sndBuffer.readData(ref readData2, ref msgNo);
            Assert.AreEqual(5, readBytes[1]);
            for (int i = 0; i < 5; ++i)
            {
                Assert.AreEqual(data2[i], readData2[i]);
            }

            sndBuffer.addBuffer(data3, 0, data3.Length, longTTL);
            Assert.AreEqual(6, sndBuffer.getCurrBufSize());

            readBytes[2] = sndBuffer.readData(ref readData3, ref msgNo);
            Assert.AreEqual(2, readBytes[2]);
            for (int i = 0; i < 2; ++i)
            {
                Assert.AreEqual(data2[i + 5], readData3[i]);
            }

            readBytes[3] = sndBuffer.readData(ref readData4, ref msgNo);
            Assert.AreEqual(5, readBytes[3]);
            for (int i = 0; i < 5; ++i)
            {
                Assert.AreEqual(data3[i], readData4[i]);
            }

            readBytes[4] = sndBuffer.readData(ref readData5, ref msgNo);
            Assert.AreEqual(5, readBytes[4]);
            for (int i = 0; i < 5; ++i)
            {
                Assert.AreEqual(data3[i + 5], readData5[i]);
            }

            readBytes[5] = sndBuffer.readData(ref readData6, ref msgNo);
            Assert.AreEqual(1, readBytes[5]);
            for (int i = 0; i < 1; ++i)
            {
                Assert.AreEqual(data3[i + 10], readData6[i]);
            }

            byte[] readData7 = null;
            int read = sndBuffer.readData(ref readData7, ref msgNo);
            Assert.AreEqual(0, read);

            int buffers = sndBuffer.getCurrBufSize();
            Assert.AreEqual(6, buffers);

            byte[][] allData = { readData1, readData2, readData3, readData4, readData5, readData6 };

            byte[] data = null;
            int msgLen;
            for (int i = 0; i < buffers; ++i)
            {
                read = sndBuffer.readData(ref data, i, ref msgNo, out msgLen);
                if (read == -1)
                {
                    // buffer 1 & 2 timed-out
                    Assert.IsTrue(i == 1 || i == 2);
                    continue;
                }
                Assert.AreEqual(readBytes[i], read);
                for (int b = 0; b < read; ++b)
                {
                    Assert.AreEqual(allData[i][b], data[b]);
                }
            }

            sndBuffer.ackData(6);
            byte[] newData1 = new byte[] { 5, 4, 3, 2, 1 };
            sndBuffer.addBuffer(newData1, 0, newData1.Length, longTTL);
            sndBuffer.addBuffer(newData1, 0, newData1.Length, shortTTL);
            sndBuffer.addBuffer(newData1, 0, newData1.Length, shortTTL);

            // sleep to allow timeout
            System.Threading.Thread.Sleep(1);

            read = sndBuffer.readData(ref data, 0, ref msgNo, out msgLen);
            Assert.AreEqual(5, read);
            read = sndBuffer.readData(ref data, 1, ref msgNo, out msgLen);
            Assert.AreEqual(-1, read);
            read = sndBuffer.readData(ref data, 2, ref msgNo, out msgLen);
            Assert.AreEqual(-1, read);
        }

        [Test]
        public void RcvBufferReadBuffer()
        {
            Unit unit1 = new Unit();
            unit1.m_iFlag = 0;
            unit1.m_Packet.SetDataFromBytes(new byte[] { 1, 2 });

            Unit unit2 = new Unit();
            unit2.m_iFlag = 0;
            unit2.m_Packet.SetDataFromBytes(new byte[] { 3, 4 });

            Unit unit3 = new Unit();
            unit3.m_iFlag = 0;
            unit3.m_Packet.SetDataFromBytes(new byte[] { 5, 6 });

            int bufferSize = 3;
            RcvBuffer rcvBuffer = new RcvBuffer(bufferSize);
            Assert.AreEqual(2, rcvBuffer.getAvailBufSize());

            Assert.AreEqual(0, rcvBuffer.addData(unit1, 0));
            Assert.AreEqual(1, unit1.m_iFlag);
            Assert.AreEqual(2, rcvBuffer.getAvailBufSize());

            Assert.AreEqual(0, rcvBuffer.addData(unit2, 1));
            Assert.AreEqual(1, unit2.m_iFlag);
            Assert.AreEqual(2, rcvBuffer.getAvailBufSize());

            Assert.AreEqual(0, rcvBuffer.addData(unit3, 2));
            Assert.AreEqual(1, unit3.m_iFlag);

            byte[] readData = new byte[4];
            int read = rcvBuffer.readBuffer(readData, 0, readData.Length);
            Assert.AreEqual(0, read);

            rcvBuffer.ackData(1);
            read = rcvBuffer.readBuffer(readData, 0, readData.Length);
            Assert.AreEqual(2, read);
            Assert.AreEqual(1, readData[0]);
            Assert.AreEqual(2, readData[1]);

            rcvBuffer.ackData(2);
            read = rcvBuffer.readBuffer(readData, 0, readData.Length);
            Assert.AreEqual(4, read);
            Assert.AreEqual(3, readData[0]);
            Assert.AreEqual(4, readData[1]);
            Assert.AreEqual(5, readData[2]);
            Assert.AreEqual(6, readData[3]);

            read = rcvBuffer.readBuffer(readData, 0, readData.Length);
            Assert.AreEqual(0, read);

            Assert.AreEqual(0, unit1.m_iFlag);
            Assert.AreEqual(0, unit2.m_iFlag);
            Assert.AreEqual(0, unit3.m_iFlag);


            Assert.AreEqual(0, rcvBuffer.addData(unit1, 0));
            Assert.AreEqual(0, rcvBuffer.addData(unit2, 1));
            Assert.AreEqual(0, rcvBuffer.addData(unit3, 2));

            rcvBuffer.ackData(2);

            byte[] smallBuffer = new byte[1];
            read = rcvBuffer.readBuffer(smallBuffer, 0, 1);

            Assert.AreEqual(1, read);
            Assert.AreEqual(1, smallBuffer[0]);

            read = rcvBuffer.readBuffer(smallBuffer, 0, 1);

            Assert.AreEqual(1, read);
            Assert.AreEqual(2, smallBuffer[0]);

            read = rcvBuffer.readBuffer(readData, 1, 2);

            Assert.AreEqual(2, read);
            Assert.AreEqual(3, readData[1]);
            Assert.AreEqual(4, readData[2]);
        }

        [Test]
        public void RcvBufferReadMsg()
        {
            uint messageStart = 2U << 30;
            uint messageEnd = 1U << 30;
            Unit unit1 = new Unit();
            unit1.m_iFlag = 0;
            unit1.m_Packet.SetDataFromBytes(new byte[] { 1, 2 });
            unit1.m_Packet.SetMessageNumber(messageStart);

            Unit unit2 = new Unit();
            unit2.m_iFlag = 0;
            unit2.m_Packet.SetDataFromBytes(new byte[] { 3, 4 });
            unit2.m_Packet.SetMessageNumber(0);

            Unit unit3 = new Unit();
            unit3.m_iFlag = 0;
            unit3.m_Packet.SetDataFromBytes(new byte[] { 5, 6 });
            unit3.m_Packet.SetMessageNumber(messageEnd);

            int bufferSize = 4;
            RcvBuffer rcvBuffer = new RcvBuffer(bufferSize);

            rcvBuffer.addData(unit1, 0);
            rcvBuffer.addData(unit2, 1);
            rcvBuffer.addData(unit3, 2);

            byte[] msg = new byte[6];
            int read = rcvBuffer.readMsg(msg, msg.Length);
            Assert.AreEqual(6, read);
            Assert.AreEqual(1, msg[0]);
            Assert.AreEqual(2, msg[1]);
            Assert.AreEqual(3, msg[2]);
            Assert.AreEqual(4, msg[3]);
            Assert.AreEqual(5, msg[4]);
            Assert.AreEqual(6, msg[5]);
        }
    }
}
