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
		[TestCase(17, 2, true, "0.000", ExpectedResult = false, TestName = "when scale value more than scale")]
		[TestCase(3, 2, true, "00.00", ExpectedResult = false, TestName = "when length value more than precision")]
		[TestCase(3, 2, true, "-5.5", ExpectedResult = false, TestName = "when value have minus and onlyPositive true")]
		[TestCase(5, 3, false, "--5.5", ExpectedResult = false, TestName = "when value have more then one sign")]
		[TestCase(3, 2, false, "5.5-", ExpectedResult = false, TestName = "when value have sign in the end")]
		[TestCase(3, 2, false, "5.-5", ExpectedResult = false, TestName = "when value have sign after decimal point")]
		[TestCase(5, 3, false, "5.5.5", ExpectedResult = false,
			TestName = "when value have more then one decimal point")]
		[TestCase(3, 2, false, "+1.23", ExpectedResult = false,
			TestName = "when length value with plus more than precision")]
		[TestCase(3, 2, false, "-0.00", ExpectedResult = false,
			TestName = "when length value with minus more than precision")]
		[TestCase(3, 2, false, "", ExpectedResult = false, TestName = "when value is empty string")]
		[TestCase(3, 2, false, null, ExpectedResult = false, TestName = "when value is null")]
		[TestCase(3, 2, false, "5.s", ExpectedResult = false, TestName = "when value have letters after decimal point")]
		[TestCase(3, 2, false, "5.", ExpectedResult = false, TestName = "when value have nothing after decimal point")]
		[TestCase(3, 2, false, ".02", ExpectedResult = false,
			TestName = "when value have nothing before decimal point")]
		[TestCase(17, 2, true, "something", ExpectedResult = false, TestName = "when value have letters only")]
		[TestCase(3, 2, true, "a.02", ExpectedResult = false,
			TestName = "when value have letters before decimal point")]
		[TestCase(3, 2, true, "a.sd", ExpectedResult = false,
			TestName = "when value have letters before and after decimal point")]
		[TestCase(4, 2, false, "-a.02", ExpectedResult = false,
			TestName = "when value with minus and have letters after decimal point")]
		[TestCase(3, 2, false, "+", ExpectedResult = false, TestName = "when value is plus")]
		[TestCase(3, 2, false, "-", ExpectedResult = false, TestName = "when value is minus")]
		[TestCase(3, 2, false, ",", ExpectedResult = false, TestName = "when value is comma")]
		[TestCase(3, 2, false, ".", ExpectedResult = false, TestName = "when value is decimal point")]
		public bool IsValidNumber_NotValidateNumber(int precision, int scale, bool onlyPositive, string value)
		{
			return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value);
		}

		[Test]
		[TestCase(3, 2, false, "+0,0", ExpectedResult = true, TestName = "when value have plus and separated by comma")]
		[TestCase(3, 2, false, "-0,0", ExpectedResult = true,
			TestName = "when value have minus and separated by comma")]
		[TestCase(3, 2, false, "0,0", ExpectedResult = true, TestName = "when value separated by comma")]
		[TestCase(3, 2, false, "-0.0", ExpectedResult = true, TestName = "when separated by decimal point")]
		[TestCase(17, 2, true, "0", ExpectedResult = true, TestName = "when value without scale")]
		[TestCase(17, 2, true, "0.0", ExpectedResult = true, TestName = "when value length less than precision")]
		[TestCase(3, 2, false, "+5.5", ExpectedResult = true, TestName = "when value have plus")]
		[TestCase(3, 2, false, "-5.5", ExpectedResult = true, TestName = "when value have minus")]
		public bool IsValidNumber_ValidateNumber(int precision, int scale, bool onlyPositive, string value)
		{
			return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value);
		}

		[Test]
		[TestCase(-1, 2, true, TestName = "when precision is not positive number")]
		[TestCase(0, 2, true, TestName = "when precision is zero")]
		[TestCase(2, 3, true, TestName = "when scale greater than precision")]
		[TestCase(2, 2, true, TestName = "when scale equal precision")]
		[TestCase(2, -1, true, TestName = "when scale is negative number")]
		public void Constructor_ThrowArgumentException(int precision, int scale, bool onlyPositive)
		{
			Action act = () => new NumberValidator(precision, scale, onlyPositive);
			act.ShouldThrow<ArgumentException>();
		}

		[Test]
		[TestCase(1, 0, true, TestName = "when scale is zero")]
		[TestCase(3, 2, true, TestName = "when scale is less than precision")]
		public void Constructor_DoesNotThrowArgumentException(int precision, int scale, bool onlyPositive)
		{
			Action act = () => new NumberValidator(precision, scale, onlyPositive);
			act.ShouldNotThrow<ArgumentException>();
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