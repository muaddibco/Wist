using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Wist.Core.Cryptography;
using Xunit;
using Xunit.Abstractions;

namespace Wist.Core.Tests
{
    public class CryptoHelperTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public CryptoHelperTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Theory]
        [InlineData("account name 1", "565DE343AA4DE3AA1BBD93B0E2EBA04EE70351A393BC95C7F2F42EE154234AF6B5ACF7EFEA949F19C589053C0C80EDE97B623C2CCFC0F5D0BF0E56FBBECF524A")]
        public void Single512Test(string inputText, string expectedHash)
        {
            byte[] input = Encoding.ASCII.GetBytes(inputText);
            byte[] expectedHashBytes = Enumerable.Range(0, expectedHash.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(expectedHash.Substring(x, 2), 16)).ToArray();

            byte[] computedHash = CryptoHelper.ComputeHash(input);

            Assert.Equal<byte>(expectedHashBytes, computedHash);
        }

        [Theory]
        [InlineData("account name 1", "7F0AE8AFFCA41A26F78DA4964C4A7E7532458BB3EEA7E366DD9E0A10EFD617BBDD80A91B8662A196299E29B4AC3AAD86394A5F8583100D87C6E7C4F1461E35DB", 10)]
        public void Multiple512Test(string inputText, string expectedHash, uint nestLevel)
        {
            byte[] input = Encoding.ASCII.GetBytes(inputText);
            byte[] expectedHashBytes = Enumerable.Range(0, expectedHash.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(expectedHash.Substring(x, 2), 16)).ToArray();

            byte[] computedHash = CryptoHelper.ComputeHash(input, nestLevel);

            Assert.Equal<byte>(expectedHashBytes, computedHash);
        }

        [Theory]
        [InlineData("account name 1", 1000000, 4000)]
        public void LongLoop512Test(string inputText, uint nestLevel, uint maxMSec)
        {
            byte[] input = Encoding.ASCII.GetBytes(inputText);

            Stopwatch timer = new Stopwatch();
            timer.Start();
            byte[] computedHash = CryptoHelper.ComputeHash(input, nestLevel);
            timer.Stop();

            _outputHelper.WriteLine($"Total run time for {nestLevel} loops is {timer.ElapsedMilliseconds} msec");
            Assert.True(timer.ElapsedMilliseconds < maxMSec, $"Total run time for {nestLevel} loops was {timer.ElapsedMilliseconds} msec, that exceeds expected {maxMSec} msec");
        }

        [Fact]
        public void HashComparisonEqualTest()
        {
            string text = "Some text to take hash of";
            byte[] textBytes = Encoding.ASCII.GetBytes(text);
            byte[] computedHash1 = CryptoHelper.ComputeHash(textBytes, 1);
            byte[] computedHash2 = CryptoHelper.ComputeHash(textBytes, 1);

            bool equal = CryptoHelper.HashX16Equals(computedHash1, computedHash2);

            Assert.True(equal);
        }

        [Fact]
        public void HashComparisonNotEqualTest()
        {
            string text1 = "Some text 1 to take hash of";
            string text2 = "Some text 2 to take hash of";
            byte[] text1Bytes = Encoding.ASCII.GetBytes(text1);
            byte[] text2Bytes = Encoding.ASCII.GetBytes(text2);
            byte[] computedHash1 = CryptoHelper.ComputeHash(text1Bytes, 1);
            byte[] computedHash2 = CryptoHelper.ComputeHash(text2Bytes, 1);

            bool equal = CryptoHelper.HashX16Equals(computedHash1, computedHash2);

            Assert.False(equal);
        }

        [Fact]
        public void HashComparisonEqualInLoopTest()
        {
            string text = "Some text to take hash of";
            byte[] textBytes = Encoding.ASCII.GetBytes(text);
            byte[] computedHash1 = CryptoHelper.ComputeHash(textBytes, 1);
            byte[] computedHash2 = CryptoHelper.ComputeHash(textBytes, 1);
            bool equal = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                equal = CryptoHelper.HashX16Equals(computedHash1, computedHash2);
            }
            sw.Stop();

            Assert.True(equal);
            _outputHelper.WriteLine($"Total run 1000000 loops is {sw.ElapsedMilliseconds} msec");
            Assert.True(sw.ElapsedMilliseconds < 1000, $"Total run time for 1000000 loops was {sw.ElapsedMilliseconds} msec, that exceeds expected 1000 msec");
        }

        [Fact]
        public void HashComparisonEqual2InLoopTest()
        {
            string text = "Some text to take hash of";
            byte[] textBytes = Encoding.ASCII.GetBytes(text);
            byte[] computedHash1 = CryptoHelper.ComputeHash(textBytes, 1);
            byte[] computedHash2 = CryptoHelper.ComputeHash(textBytes, 1);
            bool equal = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                equal = CryptoHelper.Hash64Equals(computedHash1, computedHash2);
            }
            sw.Stop();

            Assert.True(equal);
            _outputHelper.WriteLine($"Total run 1000000 loops is {sw.ElapsedMilliseconds} msec");
            Assert.True(sw.ElapsedMilliseconds < 1000, $"Total run time for 1000000 loops was {sw.ElapsedMilliseconds} msec, that exceeds expected 1000 msec");
        }
    }
}
