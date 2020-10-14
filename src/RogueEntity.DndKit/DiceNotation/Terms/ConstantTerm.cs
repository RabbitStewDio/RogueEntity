using System;
using System.Runtime.Serialization;

namespace RogueEntity.DndKit.DiceNotation.Terms
{
	/// <summary>
	/// Base term -- represents a numerical constant.
	/// </summary>
	[Serializable]
	[DataContract]
    public class ConstantTerm : ITerm
	{
        [DataMember]
        readonly int value;

		/// <summary>
		/// Constructor. Takes the numerical constant it represents.
		/// </summary>
		/// <param name="value">The numerical value this term represents.</param>
		public ConstantTerm(int value)
		{
			this.value = value;
		}

		/// <summary>
		/// Returns the numerical constant it represents. RNG is unused.
		/// </summary>
		/// <param name="rng">(Unused) rng.</param>
		/// <returns>The numerical constant this term represents.</returns>
		public int GetResult(Func<double> rng) => value;

		/// <summary>
		/// Returns a string representation of this constant.
		/// </summary>
		/// <returns>The numerical constant being represented, as a string.</returns>
		public override string ToString()
		{
			return value.ToString();
		}
	}
}