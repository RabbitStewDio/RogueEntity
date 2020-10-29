using FluentAssertions;
using NUnit.Framework;
using RogueEntity.DndKit.DiceNotation;

namespace RogueEntity.DndKit.Tests
{
	public class DiceNotationTests
	{
		[Test]
		public void AdvancedDice()
		{
			var expr = Dice.Parse("1d(1d12+4)+3");
			AssertMinMaxValues(expr, 4, 19);
			AssertReturnedInRange(expr, 4, 19);
		}

        [Test]
		public void KeepDiceAdd()
		{
			var expr = Dice.Parse("5d6k2+3");
			AssertMinMaxValues(expr, 5, 15);
			AssertReturnedInRange(expr, 5, 15);
		}

        [Test]
		public void MultipleDice()
		{
			var expr = Dice.Parse("2d6");
			AssertMinMaxValues(expr, 2, 12);
			AssertReturnedInRange(expr, 2, 12);
		}

        [Test]
		public void MultipleDiceAdd()
		{
			var expr = Dice.Parse("2d6+3");
			AssertMinMaxValues(expr, 5, 15);
			AssertReturnedInRange(expr, 5, 15);
		}

        [Test]
		public void MultipleDiceAddMultiply()
		{
			var expr = Dice.Parse("(2d6+2)*3");
			AssertMinMaxValues(expr, 12, 42);
			AssertReturnedInRange(expr, 12, 42);
		}

        [Test]
		public void MultipleDiceMultiply()
		{
			var expr = Dice.Parse("3*2d6");
			AssertMinMaxValues(expr, 6, 36);
			AssertReturnedInRange(expr, 6, 36);
		}

        [Test]
		public void SingleDice()
		{
			var expr = Dice.Parse("1d6");
			AssertMinMaxValues(expr, 1, 6);
			AssertReturnedInRange(expr, 1, 6);
		}

        [Test]
		public void SingleDiceAdd()
		{
			var expr = Dice.Parse("1d6+3");
			AssertMinMaxValues(expr, 4, 9);
			AssertReturnedInRange(expr, 4, 9);
		}

        [Test]
		public void SingleDiceAddMultiply()
		{
			var expr = Dice.Parse("3*(1d6+2)");
			AssertMinMaxValues(expr, 9, 24);
			AssertReturnedInRange(expr, 9, 24);
		}

        [Test]
		public void SingleDiceMultiply()
		{
			var expr = Dice.Parse("1d6*3");
			AssertMinMaxValues(expr, 3, 18);
			AssertReturnedInRange(expr, 3, 18);
		}

        static void AssertMinMaxValues(IDiceExpression expr, int min, int max)
        {
            expr.MinRoll().Should().Be(min);
            expr.MaxRoll().Should().Be(max);
		}

        static void AssertReturnedInRange(IDiceExpression expr, int min, int max)
		{
			for (int i = 0; i < 100; i++)
			{
				int result = expr.Roll(GeneratorExtensions.FromSeed(100));

				bool inRange = result >= min && result <= max;
                inRange.Should().BeTrue();
			}
		}
	}
}