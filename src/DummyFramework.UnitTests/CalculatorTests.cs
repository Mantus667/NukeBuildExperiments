using System;
using Xunit;

namespace DummyFramework.UnitTests
{
    public class CalculatorTests
    {
        [Fact]
        public void Test1()
        {
            var result = Calculator.Add(1, 1);

            Assert.Equal(2, result);
        }
    }
}
