// ReSharper disable once CheckNamespace
namespace FluentAssertions.Equivalency;

public static class SelfReferenceEquivalencyAssertionOptionsExtensions
{
	public static TSelf WithPrecision<TSelf>(
		this SelfReferenceEquivalencyAssertionOptions<TSelf> options, int precision)
		where TSelf : SelfReferenceEquivalencyAssertionOptions<TSelf>
	{
		if (precision is < 0 or > 9)
		{
			throw new ArgumentException($"Precision must be greater or equal 0 and less then 10, but was {precision}");
		}

		return options
			.Using<double>(ctx => ctx.Subject
				.Should()
				.BeApproximately(ctx.Expectation, 1 / Math.Pow(10, precision)))
			.WhenTypeIs<double>();
	}
}