using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace RogueEntity.Performance.Tests
{
    public class GeneratorTest
    {
        const int MaxParameterCount = 7;
        
        [Test]
        public void TestForLoop()
        {
            
            foreach (var (inParam, outParam) in ProduceParameterConfig().Select(e => e))
            {
                Console.WriteLine($"{inParam + outParam}: {inParam}, {outParam}");
            }
        }
        
        public IEnumerable<(int, int)> ProduceParameterConfig()
        {
            for (int paramCount = 1; paramCount <= MaxParameterCount; paramCount += 1)
            {
                for (int inParam = 0; inParam < paramCount; inParam += 1)
                {
                    yield return (inParam, paramCount - inParam);
                }
            }
        }

    }
}
