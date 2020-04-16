using NUnit.Framework;

using System.Net;
using System.Net.Sockets;
using System.Threading;

using UdtSharp;

namespace UdtSharpTests
{
    [TestFixture]
    public class QueueTests
    {
        [Test]
        public void SndUListTest()
        {
            UDT udt = new UDT();
            udt.m_pSNode = new SNode();
            udt.m_pSNode.m_pUDT = udt;
            udt.m_pSNode.m_llTimeStamp = 1;
            udt.m_pSNode.m_iHeapLoc = -1;

            SndUList sndUList = new SndUList();
            sndUList.m_pTimer = new UdtSharp.Timer();
            sndUList.m_pWindowLock = new object();
            sndUList.m_pWindowCond = new EventWaitHandle(false, EventResetMode.AutoReset);

            IPEndPoint endPoint = null;
            Packet packet = null;

            Assert.AreEqual(-1, sndUList.pop(ref endPoint, ref packet));

            sndUList.insert(100, udt);

            sndUList.update(udt, false);
            sndUList.update(udt, true);

            Assert.AreEqual(-1, sndUList.pop(ref endPoint, ref packet));

            udt.m_pSNode.m_llTimeStamp = 1;
            udt.m_pSNode.m_iHeapLoc = -1;

            UDT udt2 = new UDT();
            udt2.m_pSNode = new SNode();
            udt2.m_pSNode.m_pUDT = udt2;
            udt2.m_pSNode.m_llTimeStamp = 1;
            udt2.m_pSNode.m_iHeapLoc = -1;

            sndUList.insert(10, udt);

            Assert.AreEqual(10UL, sndUList.getNextProcTime());
            Assert.AreEqual(0, udt.m_pSNode.m_iHeapLoc);

            sndUList.insert(5, udt2);

            Assert.AreEqual(5UL, sndUList.getNextProcTime());
            Assert.AreEqual(1, udt.m_pSNode.m_iHeapLoc);
            Assert.AreEqual(0, udt2.m_pSNode.m_iHeapLoc);

            sndUList.remove(udt2);

            Assert.AreEqual(10UL, sndUList.getNextProcTime());
            Assert.AreEqual(0, udt.m_pSNode.m_iHeapLoc);
            Assert.AreEqual(-1, udt2.m_pSNode.m_iHeapLoc);

            sndUList.remove(udt);

            Assert.AreEqual(0UL, sndUList.getNextProcTime());
            Assert.AreEqual(-1, udt.m_pSNode.m_iHeapLoc);
            Assert.AreEqual(-1, udt2.m_pSNode.m_iHeapLoc);

            sndUList.insert(10, udt);
            sndUList.insert(5, udt2);
            Assert.AreEqual(1, udt.m_pSNode.m_iHeapLoc);
            Assert.AreEqual(0, udt2.m_pSNode.m_iHeapLoc);

            sndUList.update(udt, true);
            Assert.AreEqual(0, udt.m_pSNode.m_iHeapLoc);
            Assert.AreEqual(1, udt2.m_pSNode.m_iHeapLoc);
        }

        [Test]
        public void RcvUListTest()
        {
            UDT udt = new UDT();
            udt.m_pRNode = new RNode();
            udt.m_pRNode.m_pUDT = udt;
            udt.m_pRNode.m_llTimeStamp = 1;
            udt.m_pRNode.m_bOnList = false;

            UDT udt2 = new UDT();
            udt2.m_pRNode = new RNode();
            udt2.m_pRNode.m_pUDT = udt2;
            udt2.m_pRNode.m_llTimeStamp = 1;
            udt2.m_pRNode.m_bOnList = false;

            RcvUList rcvUList = new RcvUList();

            udt.m_pRNode.m_bOnList = true;
            rcvUList.insert(udt);
            Assert.IsTrue(udt.m_pRNode.m_llTimeStamp > 1);

            udt2.m_pRNode.m_bOnList = true;
            rcvUList.insert(udt2);
            Assert.IsTrue(udt2.m_pRNode.m_llTimeStamp > 1);
            Assert.IsTrue(udt2.m_pRNode.m_llTimeStamp > udt.m_pRNode.m_llTimeStamp);

            rcvUList.update(udt);
            Assert.IsTrue(udt.m_pRNode.m_llTimeStamp > udt2.m_pRNode.m_llTimeStamp);

            rcvUList.remove(udt);
            rcvUList.remove(udt2);
        }

        [Test]
        public void RendezvouzQueueTest()
        {
            IPEndPoint endPointA = new IPEndPoint(IPAddress.Parse("1.1.1.1"), 8000);
            IPEndPoint endPointB = new IPEndPoint(IPAddress.Parse("1.1.1.1"), 5000);
            IPEndPoint endPointC = new IPEndPoint(IPAddress.Parse("2.2.2.2"), 8000);
            IPEndPoint endPointD = new IPEndPoint(IPAddress.Parse("3.3.3.3"), 8000);

            UDT udt = new UDT();
            udt.m_SocketID = 1;

            UDT udt2 = new UDT();
            udt2.m_SocketID = 2;

            RendezvousQueue rendezvousQueue = new RendezvousQueue();

            rendezvousQueue.insert(1, udt, AddressFamily.InterNetwork, endPointA, 100);
            rendezvousQueue.insert(2, udt2, AddressFamily.InterNetwork, endPointC, 100);

            int foundId = 0;
            UDT foundUdt = rendezvousQueue.retrieve(endPointB, ref foundId);
            Assert.IsNull(foundUdt);
            foundUdt = rendezvousQueue.retrieve(endPointD, ref foundId);
            Assert.IsNull(foundUdt);

            foundUdt = rendezvousQueue.retrieve(endPointC, ref foundId);
            Assert.AreEqual(2, foundId);
            Assert.AreEqual(2, foundUdt.m_SocketID);

            foundId = 0;
            foundUdt = rendezvousQueue.retrieve(endPointA, ref foundId);
            Assert.AreEqual(1, foundId);
            Assert.AreEqual(1, foundUdt.m_SocketID);

            rendezvousQueue.remove(2);
            rendezvousQueue.remove(1);

            foundId = 0;
            foundUdt = rendezvousQueue.retrieve(endPointA, ref foundId);
            Assert.IsNull(foundUdt);

            foundUdt = rendezvousQueue.retrieve(endPointC, ref foundId);
            Assert.IsNull(foundUdt);
        }

        [Test]
        public void RcvQueueTest()
        {
            RcvQueue rcvQueue = new RcvQueue();

            UDT udt = new UDT();
            UDT udt2 = new UDT();

            Assert.AreEqual(0, rcvQueue.setListener(udt));
            Assert.AreEqual(-1, rcvQueue.setListener(udt2));
            rcvQueue.removeListener(udt2);
            Assert.AreEqual(-1, rcvQueue.setListener(udt2));
            rcvQueue.removeListener(udt);
            Assert.AreEqual(0, rcvQueue.setListener(udt2));

            rcvQueue.registerConnector(1, udt, AddressFamily.InterNetwork, null, 100);
            rcvQueue.registerConnector(2, udt2, AddressFamily.InterNetwork, null, 100);

            rcvQueue.removeConnector(1);
            rcvQueue.removeConnector(2);
        }
    }
}
