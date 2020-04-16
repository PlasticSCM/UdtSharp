using NUnit.Framework;

using UdtSharp;

namespace UdtSharpTests
{
    [TestFixture]
    public class LossListTests
    {
        [Test]
        public void SndLossListTest()
        {
            int listSize = 5;
            SndLossList sndLossList = new SndLossList(listSize);

            Assert.AreEqual(4, sndLossList.insert(1, 4));

            Assert.AreEqual(4, sndLossList.getLossLength());

            Assert.AreEqual(1, sndLossList.getLostSeq());

            Assert.AreEqual(3, sndLossList.getLossLength());

            Assert.AreEqual(2, sndLossList.getLostSeq());

            Assert.AreEqual(2, sndLossList.getLossLength());

            Assert.AreEqual(3, sndLossList.getLostSeq());

            Assert.AreEqual(1, sndLossList.getLossLength());

            Assert.AreEqual(4, sndLossList.getLostSeq());

            Assert.AreEqual(0, sndLossList.getLossLength());

            Assert.AreEqual(6, sndLossList.insert(1, 6));

            Assert.AreEqual(6, sndLossList.getLossLength());

            sndLossList.remove(2);

            Assert.AreEqual(4, sndLossList.getLossLength());

            Assert.AreEqual(3, sndLossList.getLostSeq());

            Assert.AreEqual(3, sndLossList.getLossLength());

            sndLossList.remove(6);

            Assert.AreEqual(0, sndLossList.getLossLength());

            Assert.AreEqual(-1, sndLossList.getLostSeq());

            Assert.AreEqual(6, sndLossList.insert(5, 10));

            Assert.AreEqual(4, sndLossList.insert(11, 14));

            Assert.AreEqual(10, sndLossList.insert(15, 24));

            Assert.AreEqual(20, sndLossList.getLossLength());



            sndLossList = new SndLossList(listSize);

            sndLossList.insert(4, 4);
            sndLossList.insert(5, 5);
            sndLossList.insert(6, 6);
            Assert.AreEqual(3, sndLossList.getLossLength());
            sndLossList.insert(7, 7);
            sndLossList.insert(8, 8);
            sndLossList.insert(9, 9);
            Assert.AreEqual(6, sndLossList.getLossLength());
        }

        [Test]
        public void RcvLossListTest()
        {
            int listSize = 10;
            RcvLossList rcvLossList = new RcvLossList(listSize);

            rcvLossList.insert(1, 1);
            rcvLossList.insert(2, 8);
            rcvLossList.insert(9, 9);

            Assert.AreEqual(9, rcvLossList.getLossLength());
            Assert.AreEqual(1, rcvLossList.getFirstLostSeq());

            bool removed = rcvLossList.remove(1);
            Assert.IsTrue(removed);
            Assert.AreEqual(8, rcvLossList.getLossLength());
            Assert.AreEqual(2, rcvLossList.getFirstLostSeq());

            removed = rcvLossList.remove(9);
            Assert.IsTrue(removed);
            Assert.AreEqual(7, rcvLossList.getLossLength());
            Assert.AreEqual(2, rcvLossList.getFirstLostSeq());

            removed = rcvLossList.remove(3);
            Assert.IsTrue(removed);
            Assert.AreEqual(6, rcvLossList.getLossLength());
            Assert.AreEqual(2, rcvLossList.getFirstLostSeq());

            removed = rcvLossList.remove(3);
            Assert.IsFalse(removed);
            Assert.AreEqual(6, rcvLossList.getLossLength());
            Assert.AreEqual(2, rcvLossList.getFirstLostSeq());

            removed = rcvLossList.remove(5, 7);
            Assert.IsTrue(removed);
            Assert.AreEqual(3, rcvLossList.getLossLength());
            Assert.AreEqual(2, rcvLossList.getFirstLostSeq());

            removed = rcvLossList.remove(2, 4);
            Assert.IsTrue(removed);
            Assert.AreEqual(1, rcvLossList.getLossLength());
            Assert.AreEqual(8, rcvLossList.getFirstLostSeq());

            removed = rcvLossList.remove(8);
            Assert.IsTrue(removed);
            Assert.AreEqual(0, rcvLossList.getLossLength());
            Assert.AreEqual(-1, rcvLossList.getFirstLostSeq());


            int maxLoss = 5;
            int[] lossArray = new int[maxLoss];
            int lossLength;

            rcvLossList.getLossArray(lossArray, out lossLength, maxLoss);
            Assert.AreEqual(0, lossLength);

            rcvLossList.insert(1, 1);
            rcvLossList.insert(3, 5);
            rcvLossList.insert(7, 7);

            rcvLossList.getLossArray(lossArray, out lossLength, maxLoss);
            Assert.AreEqual(4, lossLength);

            Assert.AreEqual(1, SequenceNumberFromLossValue(lossArray[0]));
            Assert.AreEqual(3, SequenceNumberFromLossValue(lossArray[1]));
            Assert.AreEqual(5, SequenceNumberFromLossValue(lossArray[2]));
            Assert.AreEqual(7, SequenceNumberFromLossValue(lossArray[3]));

            Assert.IsFalse(LossValueIsRangeStart(lossArray[0]));
            Assert.IsTrue(LossValueIsRangeStart(lossArray[1]));
            Assert.IsFalse(LossValueIsRangeStart(lossArray[2]));
            Assert.IsFalse(LossValueIsRangeStart(lossArray[3]));
        }

        bool LossValueIsRangeStart(int value)
        {
            return (uint)value >> 31 == 1;
        }

        int SequenceNumberFromLossValue(int value)
        {
            return (int)((uint)value & 0x7FFFFFFF);
        }
    }
}
