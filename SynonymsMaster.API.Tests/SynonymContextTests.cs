using NUnit.Framework;
using SynonymsMaster.API;

namespace SynonymsMaster.Api.Tests
{
    [TestFixture]
    public class SynonymContextTests
    {
        [Test]
        public void GetSynonym_WithoutTransitive_ReturnsOnlyDirectSynonyms_Test()
        {
            // Arrange
            var context = new SynonymContext();
            context.Add("happy", "cheerful");
            context.Add("cheerful", "joyful");
            context.Add("joyful", "festive");

            // Act
            var synonym = context.GetSynonyms("happy", false);

            // Assert
            Assert.That(synonym.Count, Is.EqualTo(1));
            Assert.That(synonym.First(), Is.EqualTo("cheerful"));
        }

        [Test]
        public void GetSynonym_WithTransitive_ReturnsAllSynonyms_Test()
        {
            // Arrange
            var context = new SynonymContext();
            context.Add("happy", "cheerful");
            context.Add("cheerful", "joyful");
            context.Add("joyful", "festive");
            context.Add("unrelated", "unconnected"); // Should not end up

            // Act
            var synonym = context.GetSynonyms("happy", true);

            // Assert
            var expectedResult = new List<string> { "cheerful", "joyful", "festive" };
            Assert.That(synonym.Count, Is.EqualTo(expectedResult.Count));
            Assert.That(synonym, Is.SupersetOf(expectedResult));
        }

        [Test]
        public void GetSynonym_IsCaseInsensitive_Test()
        {
            // Arrange
            var context = new SynonymContext();
            context.Add("HaPpY", "CheErful");
            context.Add("cheerful", "Joyful");
            context.Add("joyfuL", "festIve");
            context.Add("unrelated", "unconnected"); // Should not end up

            // Act
            var synonym = context.GetSynonyms("happy", true);

            // Assert
            var expectedResult = new List<string> { "cheerful", "joyful", "festive" };
            Assert.That(synonym.Count, Is.EqualTo(expectedResult.Count));
            Assert.That(synonym, Is.SupersetOf(expectedResult));
        }

        [Test]
        public void GetSynonym_WhenNoExisting_Test()
        {
            // Arrange
            var context = new SynonymContext();
            context.Add("happy", "cheerful");
            context.Add("cheerful", "joyful");
            context.Add("joyful", "festive");

            // Act
            var synonym = context.GetSynonyms("notExisting", true);

            // Assert
            Assert.That(synonym.Count, Is.EqualTo(0));
        }
    }
}
