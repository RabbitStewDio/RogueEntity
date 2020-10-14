using NUnit.Framework;
using RogueEntity.DndKit.DiceNotation;

namespace GoLightweightRogue.Tests
{
	public class DiceNotationTests
	{
		[Test]
		public void AdvancedDice()
		{
			var expr = Dice.Parse("1d(1d12+4)+3");
			assertMinMaxValues(expr, 4, 19);
			assertReturnedInRange(expr, 4, 19);
		}

        [Test]
		public void KeepDiceAdd()
		{
			var expr = Dice.Parse("5d6k2+3");
			assertMinMaxValues(expr, 5, 15);
			assertReturnedInRange(expr, 5, 15);
		}

        [Test]
		public void MultipleDice()
		{
			var expr = Dice.Parse("2d6");
			assertMinMaxValues(expr, 2, 12);
			assertReturnedInRange(expr, 2, 12);
		}

        [Test]
		public void MultipleDiceAdd()
		{
			var expr = Dice.Parse("2d6+3");
			assertMinMaxValues(expr, 5, 15);
			assertReturnedInRange(expr, 5, 15);
		}

        [Test]
		public void MultipleDiceAddMultiply()
		{
			var expr = Dice.Parse("(2d6+2)*3");
			assertMinMaxValues(expr, 12, 42);
			assertReturnedInRange(expr, 12, 42);
		}

        [Test]
		public void MultipleDiceMultiply()
		{
			var expr = Dice.Parse("3*2d6");
			assertMinMaxValues(expr, 6, 36);
			assertReturnedInRange(expr, 6, 36);
		}

        [Test]
		public void SingleDice()
		{
			var expr = Dice.Parse("1d6");
			assertMinMaxValues(expr, 1, 6);
			assertReturnedInRange(expr, 1, 6);
		}

        [Test]
		public void SingleDiceAdd()
		{
			var expr = Dice.Parse("1d6+3");
			assertMinMaxValues(expr, 4, 9);
			assertReturnedInRange(expr, 4, 9);
		}

        [Test]
		public void SingleDiceAddMultiply()
		{
			var expr = Dice.Parse("3*(1d6+2)");
			assertMinMaxValues(expr, 9, 24);
			assertReturnedInRange(expr, 9, 24);
		}

        [Test]
		public void SingleDiceMultiply()
		{
			var expr = Dice.Parse("1d6*3");
			assertMinMaxValues(expr, 3, 18);
			assertReturnedInRange(expr, 3, 18);
		}

		private void assertMinMaxValues(IDiceExpression expr, int min, int max)
		{
			Assert.AreEqual(min, expr.MinRoll());
			Assert.AreEqual(max, expr.MaxRoll());
		}

		private void assertReturnedInRange(IDiceExpression expr, int min, int max)
		{
			for (int i = 0; i < 100; i++)
			{
				int result = expr.Roll(GeneratorExtensions.FromSeed(100));

				bool inRange = result >= min && result <= max;
				Assert.AreEqual(true, inRange);
			}
		}
	}
}