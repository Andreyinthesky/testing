using System;
using System.Collections;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
		[Test]
		[TestCaseSource(nameof(NumberValidatorTestCases_StringValue_ShouldBeMatchRegex), Category =
			"StringValue_ShouldBeMatchRegex")]
		[TestCaseSource(nameof(NumberValidatorTestCases_StringValue_ShouldBeNotNullOrEmpty), Category =
			"StringValue_ShouldBeNotNullOrEmpty")]
		[TestCaseSource(nameof(NumberValidatorTestCases_IntAndFracPartCountSigns), Category =
			"IntAndFracPartCountSigns")]
		[TestCaseSource(nameof(NumberValidatorTestCases_FracPartCountSigns), Category = "FracPartCountSigns")]
		[TestCaseSource(nameof(NumberValidatorTestCases_NumberSign), Category = "NumberSign")]
		public bool Test(int precision, int scale, bool onlyPositive, string value)
		{
			return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value);
		}

		public static IEnumerable NumberValidatorTestCases_StringValue_ShouldBeMatchRegex
		{
			get
			{
				yield return new TestCaseData(17, 2, true, "something").Returns(false);
				yield return new TestCaseData(3, 2, true, "a.sd").Returns(false);
				yield return new TestCaseData(3, 2, true, "0.sd").Returns(false);
				yield return new TestCaseData(3, 2, true, "a.02").Returns(false);
				yield return new TestCaseData(4, 2, false, "-a.02").Returns(false);
			}
		}

		public static IEnumerable NumberValidatorTestCases_StringValue_ShouldBeNotNullOrEmpty
		{
			get
			{
				yield return new TestCaseData(17, 2, true, null).Returns(false);
				yield return new TestCaseData(17, 2, true, string.Empty).Returns(false);
			}
		}

		public static IEnumerable NumberValidatorTestCases_IntAndFracPartCountSigns
		{
			get
			{
				yield return new TestCaseData(17, 2, true, "0.0").Returns(true);
				yield return new TestCaseData(17, 2, true, "0").Returns(true);
				yield return new TestCaseData(4, 2, true, "+1.23").Returns(true);
				yield return new TestCaseData(4, 2, true, "11.23").Returns(true);

				yield return new TestCaseData(3, 2, true, "00.00").Returns(false);
				yield return new TestCaseData(3, 2, false, "-0.00").Returns(false);
			}
		}

		public static IEnumerable NumberValidatorTestCases_FracPartCountSigns
		{
			get { yield return new TestCaseData(17, 2, false, "0.000").Returns(false); }
		}

		public static IEnumerable NumberValidatorTestCases_NumberSign
		{
			get
			{
				yield return new TestCaseData(4, 2, true, "-0.00").Returns(false);
				yield return new TestCaseData(4, 2, false, "-2").Returns(true);
				yield return new TestCaseData(4, 2, false, "+2").Returns(true);
				yield return new TestCaseData(4, 2, false, "2").Returns(true);
			}
		}

		[Category("TestThrows")]
		[TestCase(-1, 2, true, true)]
		[TestCase(2, 2, true, true)]
		[TestCase(1, 0, true, false)]
		public void TestThrows(int precision, int scale, bool onlyPositive, bool doesThrow)
		{
			if (doesThrow)
			{
				Assert.That(() => new NumberValidator(precision, scale, onlyPositive),
					Throws.TypeOf<ArgumentException>());
			}
			else
			{
				Assert.That(() => new NumberValidator(precision, scale, onlyPositive), Throws.Nothing);
			}
		}
    }

	public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("precision must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}