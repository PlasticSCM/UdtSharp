using NUnit.Framework;
using System.Net;
using System.Net.Sockets;
using UdtSharp;

namespace UdtSharpTests
{
    [TestFixture]
    public class UtilitiesTests
    {
        [Test]
        public void ConvertIPAddressTest()
        {
            IPAddress address = new IPAddress(new byte[] { 1, 2, 3, 4 });
            Assert.AreEqual(AddressFamily.InterNetwork, address.AddressFamily);

            uint[] output = new uint[4];
            ConvertIPAddress.ToUintArray(address, ref output);

            CollectionAssert.AreEqual(new uint[] { 0x1 + 0x200 + 0x30000 + 0x4000000, 0, 0, 0 }, output);

            address = new IPAddress(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            Assert.AreEqual(AddressFamily.InterNetworkV6, address.AddressFamily);

            ConvertIPAddress.ToUintArray(address, ref output);

            CollectionAssert.AreEqual(
                new uint[]
                {
                    0x0 + 0x100 + 0x20000 + 0x3000000,
                    0x4 + 0x500 + 0x60000 + 0x7000000,
                    0x8 + 0x900 + 0xA0000 + 0xB000000,
                    0xC + 0xD00 + 0xE0000 + 0xF000000
                },
                output);

        }

        [Test]
        public unsafe void ConvertLingerOptionTest()
        {
            LingerOption lingerOption = new LingerOption(true, 10000);

            byte[] buffer = new byte[5];
            fixed (void* option = buffer)
            {
                ConvertLingerOption.ToVoidPointer(lingerOption, option);

                bool* pEnabled = (bool*)option;
                Assert.IsTrue(*pEnabled);
                *pEnabled = false;

                int* pTime = (int*)(++pEnabled);
                Assert.AreEqual(10000, *pTime);
                *pTime = 2000000;

                lingerOption = ConvertLingerOption.FromVoidPointer(option);
                Assert.IsFalse(lingerOption.Enabled);
                Assert.AreEqual(2000000, lingerOption.LingerTime);
            }
        }
    }
}
