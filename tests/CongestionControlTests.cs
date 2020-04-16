using NUnit.Framework;
using UdtSharp;

namespace UdtSharpTests
{
    [TestFixture]
    public class CongestionControlTests
    {
        [Test]
        public void BasicTest()
        {
            CCVirtualFactory ccFactory = new CCFactory<UDTCC>();
            CC cc = ccFactory.create();

            cc.init();
            cc.onACK(2);
            cc.onLoss(new int[] { 1, 3, 5, 7 }, 4);
            cc.onTimeout();

            cc.onACK(6);
            cc.onLoss(new int[] { 4 }, 1);
            cc.onTimeout();

            cc.close();
        }
    }
}
