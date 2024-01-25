using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SynonymsMaster.API.Actions;
using SynonymsMaster.Controllers;
using SynonymsMaster.API.Models.API;

namespace SynonymsMaster.API.Tests
{
    [TestFixture]
    public class SynonymsControllerTests
    {
        private SynonymsController _underTest = null!;
        private ISynonymContext _context = null!;

        [SetUp]
        public void Setup()
        {
            _context = new SynonymContext();
            _underTest = new SynonymsController(new StoreSynonym(_context), new GetSynonyms(_context));

            // Setup mvc so validation works.
            var httpContext = new DefaultHttpContext();
            var serviceProvider = new ServiceCollection()
                .AddMvc()
                .Services
                .BuildServiceProvider();
            httpContext.RequestServices = serviceProvider;
            _underTest.ControllerContext = new ControllerContext(new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor()));
        }

        [TestCase("happy", "joyful", true)]
        [TestCase("happy", "", false)]
        [TestCase("happy", null, false)]
        [TestCase("", "joyful", false)]
        [TestCase(null, "joyful", false)]
        [TestCase("", "", false)]
        [TestCase("", null, false)]
        [TestCase(null, "", false)]
        [TestCase(null, null, false)]
        public void AddSynonym_CorrectReturnValue_Test(string word, string to, bool expectOkResult)
        {
            // Arrange
            var synonym = new Synonym(word, to);
            _underTest.TryValidateModel(synonym);

            // Act
            var result = _underTest.AddSynonym(synonym);

            // Assert
            var okResult = result as OkResult;
            Assert.That(okResult != null, Is.EqualTo(expectOkResult));
            if(!expectOkResult)
            {
                var badResult = result as BadRequestObjectResult;
                Assert.That(badResult, Is.Not.Null);
            }
        }

        [Test]
        public void AddSynonym_IsStoringSynonym_Test()
        {
            // Arrange
            var synonym = new Synonym("happy", "joyful");

            // Act
            var result = _underTest.AddSynonym(synonym) is OkResult;

            // Assert
            Assert.That(result, Is.True);
            var storedSynonym = _context.GetSynonyms(synonym.Word, false);
            Assert.That(storedSynonym.Count(), Is.EqualTo(1));
            Assert.That(synonym.To, Is.EqualTo(storedSynonym.First()));
        }


        [Test]
        public void GetSynonym_NoValidWord_ReturnsBadRequest_Test()
        {
            // Arrange
            var word = string.Empty;

            // Act
            var result = _underTest.GetSynonyms(word, false);

            // Assert
            var okResult = result is OkResult;
            var badResult = result is BadRequestObjectResult;
            Assert.That(okResult, Is.False);
            Assert.That(badResult, Is.True);
        }

        [Test]
        public void GetSynonym_NoSynonymFound_ReturnNotFound_Test()
        {
            // Arrange
            var word = "ValidWordButNotFound";

            // Act
            var result = _underTest.GetSynonyms(word, false);

            // Assert
            var okResult = result is OkResult;
            var notFoundResult = result is NotFoundObjectResult;
            Assert.That(okResult, Is.False);
            Assert.That(notFoundResult, Is.True);
        }

        [Test]
        public void GetSynonym_ValidWord_NoTransitiveSearch_ReturnOnlyDirectSynonym_Test()
        {
            // Arrange
            var word = "validWord";
            var synonym1 = "synonym1";
            var synonym2 = "synonym2";
            var another1 = "another";
            var another2 = "something else";
            _context.Add(word, synonym1);
            _context.Add(synonym1, synonym2);
            _context.Add(another1, another2);

            // Act
            var result = _underTest.GetSynonyms(word, false);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            
            var synonyms = okResult!.Value as IEnumerable<string>;
            Assert.That(synonyms, Is.Not.Null);
            Assert.That(synonyms!.Count(), Is.EqualTo(1));
            Assert.That(synonyms, Contains.Item(synonym1));
        }

        [Test]
        public void GetSynonym_ValidWord_NoTransitiveSearch_DoesNotReturnMySelf_Test()
        {
            // Arrange
            var word = "validWord";
            var synonym1 = "synonym1";
            var synonym2 = "synonym2";
            var another1 = "another";
            var another2 = "something else";
            _context.Add(word, synonym1);
            _context.Add(synonym1, synonym2);
            _context.Add(another1, another2);

            // Act
            var result = _underTest.GetSynonyms(word, false);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var synonyms = okResult!.Value as IEnumerable<string>;
            Assert.That(synonyms, Is.Not.Null);
            Assert.That(synonyms!.Contains(word), Is.False);
        }

        [Test]
        public void GetSynonym_ValidWord_TransitiveSearch_ReturnAllSynonyms_Test()
        {
            // Arrange
            var word = "validWord";
            var synonym1 = "synonym1";
            var synonym2 = "synonym2";
            var another1 = "another";
            var another2 = "something else";
            _context.Add(word, synonym1);
            _context.Add(synonym1, synonym2);
            _context.Add(another1, another2);

            // Act
            var result = _underTest.GetSynonyms(word, true);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var synonyms = okResult!.Value as IEnumerable<string>;
            Assert.That(synonyms, Is.Not.Null);
            Assert.That(synonyms!.Count(), Is.EqualTo(2));
            Assert.That(synonyms, Contains.Item(synonym1));
            Assert.That(synonyms, Contains.Item(synonym2));
        }

        [Test]
        public void GetSynonym_ValidWord_TransitiveSearch_DoesNotReturnMyself_Test()
        {
            // Arrange
            var word = "validWord";
            var synonym1 = "synonym1";
            var synonym2 = "synonym2";
            var another1 = "another";
            var another2 = "something else";
            _context.Add(word, synonym1);
            _context.Add(synonym1, synonym2);
            _context.Add(another1, another2);

            // Act
            var result = _underTest.GetSynonyms(word, true);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var synonyms = okResult!.Value as IEnumerable<string>;
            Assert.That(synonyms, Is.Not.Null);
            Assert.That(synonyms!.Contains(word), Is.False);
        }
    }
}
