using KSeF.Client.Validation;

namespace KSeF.Client.Tests.Core.UnitTests
{
    public class XmlUnicodeValidatorTests
    {
        [Theory]
        [InlineData("Zwykły tekst faktury")] // Bez znaków specjalnych
        [InlineData("Emoji \U0001F600 spoza BMP")] // Poprawny znak spoza BMP (para surogatów)
        [InlineData("")] // Pusty tekst
        public void FindDisallowedUnicodeCharacter_AllowedText_ReturnsNull(string value)
        {
            // Act
            string result = XmlUnicodeValidator.FindDisallowedUnicodeCharacter(value);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("abcdef", "U+007F")] // C0 control char (dolna granica)
        [InlineData("abcdef", "U+0084")] // C0 control char (górna granica)
        [InlineData("abcdef", "U+0086")] // C1 control char (dolna granica)
        [InlineData("abcdef", "U+009F")] // C1 control char (górna granica)
        [InlineData("abc﷐def", "U+FDD0")] // Noncharacter (dolna granica)
        [InlineData("abc﷯def", "U+FDEF")] // Noncharacter (górna granica)
        public void FindDisallowedUnicodeCharacter_DisallowedCharacterInBmp_ReturnsCodePoint(string value, string expected)
        {
            // Act
            string result = XmlUnicodeValidator.FindDisallowedUnicodeCharacter(value);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FindDisallowedUnicodeCharacter_NoncharacterOutsideBmp_ReturnsCodePoint()
        {
            // Arrange - U+1FFFE (para surogatów: 0xD83F 0xDFFE)
            string value = "abc" + char.ConvertFromUtf32(0x1FFFE) + "def";

            // Act
            string result = XmlUnicodeValidator.FindDisallowedUnicodeCharacter(value);

            // Assert
            Assert.Equal("U+1FFFE", result);
        }

        [Fact]
        public void FindDisallowedUnicodeCharacter_MultipleDisallowedCharacters_ReturnsFirstOne()
        {
            // Arrange - dwa niedozwolone znaki, oczekiwany jest pierwszy z nich
            string value = "abcdefghi";

            // Act
            string result = XmlUnicodeValidator.FindDisallowedUnicodeCharacter(value);

            // Assert
            Assert.Equal("U+007F", result);
        }
    }
}
