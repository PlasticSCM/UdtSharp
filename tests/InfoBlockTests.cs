using NUnit.Framework;

using UdtSharp;

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UdtSharpTests
{
    [TestFixture]
    public class InfoBlockTests
    {
        [Test]
        public void Cache()
        {
            IPAddress address1 = new IPAddress(new byte[] { 1, 2, 3, 4 });
            Assert.AreEqual(AddressFamily.InterNetwork, address1.AddressFamily);

            IPAddress address2 = new IPAddress(new byte[] { 2, 4, 6, 8 });
            Assert.AreEqual(AddressFamily.InterNetwork, address2.AddressFamily);

            IPAddress address3 = new IPAddress(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            Assert.AreEqual(AddressFamily.InterNetworkV6, address3.AddressFamily);

            InfoBlock infoBlock1 = new InfoBlock(address1);
            infoBlock1.m_iRTT = 1;

            InfoBlock infoBlock2 = new InfoBlock(address2);
            infoBlock2.m_iRTT = 2;

            InfoBlock infoBlock3 = new InfoBlock(address3);
            infoBlock3.m_iRTT = 3;

            HashSet<InfoBlock> cache = new HashSet<InfoBlock>();
            bool bAdded = cache.Add(infoBlock1);
            Assert.IsTrue(bAdded);

            bAdded = cache.Add(infoBlock2);
            Assert.IsTrue(bAdded);

            bAdded = cache.Add(infoBlock3);
            Assert.IsTrue(bAdded);

            Assert.AreEqual(3, cache.Count);

            InfoBlock findBlock1 = new InfoBlock(address1);

            bool bFound = cache.TryGetValue(findBlock1, out findBlock1);
            Assert.IsTrue(bFound);

            Assert.AreEqual(1, findBlock1.m_iRTT);

            InfoBlock findBlock2 = new InfoBlock(address2);

            bFound = cache.TryGetValue(findBlock2, out findBlock2);
            Assert.IsTrue(bFound);

            Assert.AreEqual(2, findBlock2.m_iRTT);

            InfoBlock findBlock3 = new InfoBlock(address3);

            bFound = cache.TryGetValue(findBlock3, out findBlock3);
            Assert.IsTrue(bFound);

            Assert.AreEqual(3, findBlock3.m_iRTT);

            infoBlock1.m_iBandwidth = 1000;
            bAdded = cache.Add(infoBlock1);
            Assert.IsFalse(bAdded);

            bFound = cache.TryGetValue(findBlock1, out findBlock1);
            Assert.IsTrue(bFound);

            Assert.AreEqual(1000, findBlock1.m_iBandwidth);
            Assert.AreEqual(1, findBlock1.m_iRTT);
        }

        [Test]
        public void Equals()
        {
            IPAddress address1 = new IPAddress(new byte[] { 1, 2, 3, 4 });
            Assert.AreEqual(AddressFamily.InterNetwork, address1.AddressFamily);

            IPAddress address2 = new IPAddress(new byte[] { 2, 4, 6, 8 });
            Assert.AreEqual(AddressFamily.InterNetwork, address2.AddressFamily);

            IPAddress address3 = new IPAddress(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            Assert.AreEqual(AddressFamily.InterNetworkV6, address3.AddressFamily);

            InfoBlock infoBlock11 = new InfoBlock(address1);
            infoBlock11.m_iRTT = 1;

            InfoBlock infoBlock21 = new InfoBlock(address2);
            infoBlock21.m_iRTT = 2;

            InfoBlock infoBlock31 = new InfoBlock(address3);
            infoBlock31.m_iRTT = 3;

            InfoBlock infoBlock12 = new InfoBlock(address1);
            infoBlock11.m_iRTT = 2;

            InfoBlock infoBlock22 = new InfoBlock(address2);
            infoBlock21.m_iRTT = 4;

            InfoBlock infoBlock32 = new InfoBlock(address3);
            infoBlock31.m_iRTT = 6;

            Assert.AreEqual(infoBlock11, infoBlock12);
            Assert.AreEqual(infoBlock21, infoBlock22);
            Assert.AreEqual(infoBlock31, infoBlock32);

            Assert.AreNotEqual(infoBlock11, infoBlock21);
            Assert.AreNotEqual(infoBlock11, infoBlock31);
            Assert.AreNotEqual(infoBlock21, infoBlock31);
            Assert.AreNotEqual(infoBlock12, infoBlock22);
            Assert.AreNotEqual(infoBlock12, infoBlock32);
            Assert.AreNotEqual(infoBlock22, infoBlock32);
        }


    }
}
