using NUnit.Framework;

namespace NeutronBlaster.Test
{
    public class JournalScannerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Multiple_Commanders_Ignore_Other()
        {
            var scanner = new JournalScanner("TestCases\\MultipleCommanders");

            scanner.ScanJournals();

            Assert.AreEqual("Smacker", scanner.Commander);
            Assert.AreEqual(11, scanner.JumpHistory.Count);
        }
    }
}