using NUnit.Framework;

using UdtSharp;

namespace UdtSharpTests
{
    [TestFixture]
    public class WindowTests
    {
        [Test]
        public void ACKWindowTest()
        {
            int ackWindowSize = 5;

            ACKWindow ackWindow = new ACKWindow(ackWindowSize);

            ackWindow.store(1, 10);
            ackWindow.store(2, 12);
            ackWindow.store(3, 11);

            int ack = -1;

            System.Threading.Thread.Sleep(50);
            int rtt = ackWindow.acknowledge(1, ref ack);

            Assert.IsTrue(rtt >= 40000 && rtt <= 80000);
            Assert.AreEqual(10, ack);

            System.Threading.Thread.Sleep(50);
            rtt = ackWindow.acknowledge(3, ref ack);

            Assert.IsTrue(rtt >= 80000 && rtt <= 160000);
            Assert.AreEqual(11, ack);

            rtt = ackWindow.acknowledge(2, ref ack);
            Assert.AreEqual(-1, rtt);

            ackWindow.store(4, 13);
            ackWindow.store(5, 14);
            ackWindow.store(6, 15);
            ackWindow.store(7, 16);

            rtt = ackWindow.acknowledge(4, ref ack);
            Assert.AreEqual(13, ack);

            ackWindow.store(8, 17);
            rtt = ackWindow.acknowledge(4, ref ack);
            Assert.AreEqual(-1, rtt);

            rtt = ackWindow.acknowledge(5, ref ack);
            Assert.AreEqual(14, ack);
            rtt = ackWindow.acknowledge(6, ref ack);
            Assert.AreEqual(15, ack);
            rtt = ackWindow.acknowledge(7, ref ack);
            Assert.AreEqual(16, ack);

            ackWindow.store(8, 17);
            ackWindow.store(9, 18);
            ackWindow.store(10, 19);
            ackWindow.store(11, 20);
            ackWindow.store(12, 21);
            ackWindow.store(13, 22);

            Assert.AreEqual(-1, ackWindow.acknowledge(8, ref ack));
            Assert.AreEqual(-1, ackWindow.acknowledge(9, ref ack));
            Assert.AreNotEqual(-1, ackWindow.acknowledge(10, ref ack));
            Assert.AreNotEqual(-1, ackWindow.acknowledge(11, ref ack));
            Assert.AreNotEqual(-1, ackWindow.acknowledge(12, ref ack));
            Assert.AreNotEqual(-1, ackWindow.acknowledge(13, ref ack));
        }

        [Test]
        public void PacketWindowTest()
        {
            PktTimeWindow window = new PktTimeWindow(12, 12);

            window.onPktArrival();
            System.Threading.Thread.Sleep(100);
            window.onPktArrival();
            System.Threading.Thread.Sleep(50);
            window.onPktArrival();
            System.Threading.Thread.Sleep(100);
            window.onPktArrival();
            System.Threading.Thread.Sleep(50);
            window.onPktArrival();
            System.Threading.Thread.Sleep(200);
            window.onPktArrival();
            System.Threading.Thread.Sleep(100);
            window.onPktArrival();
            System.Threading.Thread.Sleep(50);
            window.onPktArrival();
            System.Threading.Thread.Sleep(150);
            window.onPktArrival();
            System.Threading.Thread.Sleep(100);
            window.onPktArrival();
            System.Threading.Thread.Sleep(200);
            window.onPktArrival();
            System.Threading.Thread.Sleep(100);
            window.onPktArrival();

            // 12 packets in 1200 miliseconds
            // approx 10 packets per second

            int pktReceivedPerSecond = window.getPktRcvSpeed();
            Assert.IsTrue(pktReceivedPerSecond >= 9 &&
                pktReceivedPerSecond <= 11);

            window.probe1Arrival();
            System.Threading.Thread.Sleep(100);
            window.probe2Arrival();
            window.probe1Arrival();
            System.Threading.Thread.Sleep(50);
            window.probe2Arrival();
            window.probe1Arrival();
            System.Threading.Thread.Sleep(100);
            window.probe2Arrival();
            window.probe1Arrival();
            System.Threading.Thread.Sleep(50);
            window.probe2Arrival();
            window.probe1Arrival();
            System.Threading.Thread.Sleep(200);
            window.probe2Arrival();
            window.probe1Arrival();
            System.Threading.Thread.Sleep(100);
            window.probe2Arrival();
            window.probe1Arrival();
            System.Threading.Thread.Sleep(50);
            window.probe2Arrival();
            window.probe1Arrival();
            System.Threading.Thread.Sleep(50);
            window.probe2Arrival();
            window.probe1Arrival();
            System.Threading.Thread.Sleep(100);
            window.probe2Arrival();
            window.probe1Arrival();
            System.Threading.Thread.Sleep(200);
            window.probe2Arrival();

            // 10 probes in 1 second
            int getBandwidthPktPerSecond = window.getBandwidth();
            Assert.IsTrue(getBandwidthPktPerSecond >= 9 &&
                getBandwidthPktPerSecond <= 11);


            window.onPktSent((int)Timer.getTime());
            System.Threading.Thread.Sleep(100);
            window.onPktSent((int)Timer.getTime());
            System.Threading.Thread.Sleep(200);
            window.onPktSent((int)Timer.getTime());
            System.Threading.Thread.Sleep(50);
            window.onPktSent((int)Timer.getTime());
            System.Threading.Thread.Sleep(100);
            window.onPktSent((int)Timer.getTime());

            // min gap between packet send = 50 miliseconds

            int minSendIntervalMicroseconds = window.getMinPktSndInt();
            Assert.IsTrue(minSendIntervalMicroseconds >= 45000 &&
                minSendIntervalMicroseconds <= 55000);
        }



    }
}
