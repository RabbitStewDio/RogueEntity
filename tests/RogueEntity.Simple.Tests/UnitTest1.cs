using NUnit.Framework;

namespace RogueEntity.Simple.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
            
            
        }

        int value;
        public ref int Value()
        {
            return ref value;
        }

        
        public ref int TryRefOut(ref int defaultValue, out bool result)
        {
            //System.HashCode;
            result = false;
            return ref defaultValue;
        }
    }
}