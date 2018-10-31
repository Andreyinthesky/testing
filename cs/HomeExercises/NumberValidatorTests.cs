using System;
using System.Collections;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
		[TestCase(3, 2, false, "", TestName = "value is empty string")]
		[TestCase(3, 2, false, null, TestName = "value is null")]
		public void IsValidNumber_WhenValueIsNullOrEmpty_ReturnFalse(int precision, int scale, bool onlyPositive,
			string value)
		{
			new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value)
				.Should()
				.BeFalse();
		}

		[TestCase(17, 2, "0.000", TestName = "frac part length greater than scale")]
		[TestCase(17, 2, "+0.000", TestName = "frac part length greater than scale with plus")]
        [TestCase(17, 2, "-0.000", TestName = "frac part length greater than scale with minus")]
		[TestCase(3, 2, "00.00", TestName = "value is decimal and value greater than precision")]
		[TestCase(3, 2, "+0.00", TestName = "value is decimal and value greater than precision with plus")]
		[TestCase(3, 2, "-0.00", TestName = "value is decimal and value greater than precision with minus")]
		[TestCase(3, 2, "1234", TestName = "value is integer and value greater than precision")]
		[TestCase(3, 2, "+1234", TestName = "value is integer and value greater than precision with plus")]
		[TestCase(3, 2, "-1234", TestName = "value is integer and value greater than precision with minus")]
		public void IsValidNumber_WhenValueHaveTooLongIntegerOrFracPart_ReturnFalse(int precision, int scale,
			string value)
		{
			new NumberValidator(precision, scale).IsValidNumber(value)
				.Should()
				.BeFalse();
		}

		[TestCase("  5.5", TestName = "value have whitespaces at start")]
		[TestCase("5.5  ", TestName = "value have whitespaces at end")]
		[TestCase("++5.5", TestName = "value have more than one plus")]
		[TestCase("--5.5", TestName = "value have more than one minus")]
		[TestCase("5.5+", TestName = "value is decimal have plus in the end")]
		[TestCase("5.5-", TestName = "value is decimal have minus in the end")]
		[TestCase("5+", TestName = "value is integer have plus in the end")]
		[TestCase("5-", TestName = "value is integer have minus in the end")]
		[TestCase("5.-5", TestName = "value have minus after decimal point")]
		[TestCase("5.+5", TestName = "value have plus after decimal point")]
		[TestCase("5.5.5", TestName = "value have more than one decimal point")]
		[TestCase("5..5", TestName = "value have double decimal point")]
		[TestCase("5;5", TestName = "value have invalid separator")]
		[TestCase("5.", TestName = "value have nothing after decimal point")]
		[TestCase(".02", TestName = "value have nothing before decimal point")]
		[TestCase("5.sd", TestName = "value have letters after decimal point")]
		[TestCase("a.02", TestName = "value have letters before decimal point")]
		[TestCase("a.sd", TestName = "value have letters before and after decimal point")]
		[TestCase("something", TestName = "value have letters only")]
		[TestCase("+", TestName = "value is plus")]
		[TestCase("-", TestName = "value is minus")]
		[TestCase(",", TestName = "value is comma")]
		[TestCase(".", TestName = "value is decimal point")]
		public void IsValidNumber_WhenValueNotMatchRegex_ReturnFalse(string value)
		{
			new NumberValidator(17, 2).IsValidNumber(value)
				.Should()
				.BeFalse();
		}

		[TestCase(2, 0, "-0", TestName = "value is zero with minus")]
        [TestCase(2, 0, "-1", TestName = "value is negative integer")]
		[TestCase(3, 1, "-1.1", TestName = "value is negative decimal")]
		public void IsValidNumber_WhenValueIsNegativeAsOnlyPositive_ReturnFalse(int precision, int scale, string value)
		{
			new NumberValidator(precision, scale, true).IsValidNumber(value)
				.Should()
				.BeFalse();
		}

		[TestCase(1, 0, "0", TestName = "value is zero")]
		[TestCase(2, 0, "+0", TestName = "value is zero with plus")]
        [TestCase(2, 0, "+1", TestName = "value is positive integer with plus")]
		[TestCase(3, 1, "+1.1", TestName = "value is positive decimal with plus")]
		[TestCase(1, 0, "1", TestName = "value is positive integer")]
		[TestCase(2, 1, "1.1", TestName = "value is positive decimal")]
        public void IsValidNumber_WhenValueIsPositiveAsOnlyPositive_ReturnTrue(int precision, int scale, string value)
		{
			new NumberValidator(precision, scale, true).IsValidNumber(value)
				.Should()
				.BeTrue();
		}

        [TestCase(1, 0, "5", TestName = "value is integer")]
		[TestCase(2, 0, "+5", TestName = "value is integer and have plus")]
		[TestCase(2, 0, "-5", TestName = "value is integer and have minus")]
		[TestCase(2, 1, "5.0", TestName = "value is decimal")]
		[TestCase(3, 1, "+5.0", TestName = "value is decimal and have plus")]
		[TestCase(3, 1, "-5.0", TestName = "value is decimal and have minus")]
		[TestCase(2, 1, "5,0", TestName = "value is decimal with comma")]
		[TestCase(3, 1, "+5,0", TestName = "value is decimal and have plus with comma")]
		[TestCase(3, 1, "-5,0", TestName = "value is decimal and have minus with comma")]
		[TestCase(2, 0, "00", TestName = "value is double zero")]
		[TestCase(2, 0, "01", TestName = "value have zero at start")]
		public void IsValidNumber_WhenValueIsCorrect_ReturnTrue(int precision, int scale, string value)
		{
			new NumberValidator(precision, scale).IsValidNumber(value)
				.Should()
				.BeTrue();
		}

		[TestCase(-1, 2, "precision must be a positive number", TestName = "when precision is not positive number")]
		[TestCase(0, 2, "precision must be a positive number", TestName = "when precision is zero")]
		[TestCase(2, 3, "precision must be a non-negative number less than precision",
			TestName = "when scale greater than precision")]
		[TestCase(2, 2, "precision must be a non-negative number less than precision",
			TestName = "when scale equal precision")]
		[TestCase(2, -1, "precision must be a non-negative number less than precision",
			TestName = "when scale is negative number")]
		public void Constructor_ThrowArgumentException(int precision, int scale, string exceptionMessage)
		{
			Action act = () => new NumberValidator(precision, scale);
			act.ShouldThrow<ArgumentException>()
				.WithMessage(exceptionMessage);
		}

		[TestCase(1, 0, true, TestName = "when scale is zero")]
		[TestCase(3, 2, true, TestName = "when scale is less than precision")]
		public void Constructor_DoesNotThrowArgumentException(int precision, int scale, bool onlyPositive)
		{
			Action act = () => new NumberValidator(precision, scale, onlyPositive);
			act.ShouldNotThrow<ArgumentException>();
		}

		[Test]
		public void Constructor_WhenDefaultConstructor_DoesNotThrowsException()
		{
			Action action = () => new NumberValidator(1);
			action.ShouldNotThrow<ArgumentException>();
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
				throw new ArgumentException("precision must be a non-negative number less than precision");
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