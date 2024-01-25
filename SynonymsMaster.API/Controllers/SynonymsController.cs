using Microsoft.AspNetCore.Mvc;
using SynonymsMaster.API.Actions;
using SynonymsMaster.API.Models.API;

namespace SynonymsMaster.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SynonymsController(IStoreSynonym storeSynonym, IGetSynonyms getSynonyms) : ControllerBase
    {
        [HttpPost]
        public IActionResult AddSynonym(Synonym synonym)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            storeSynonym.Store(synonym.Word, synonym.To);

            return Ok();
        }

        [HttpGet]
        public IActionResult GetSynonyms(string word, bool transitiveSearch)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return BadRequest("You must supply a valid word.");
            }

            var synonyms = getSynonyms.Get(word, transitiveSearch);

            if (synonyms == null || synonyms.Count() == 0)
            {
                return NotFound("Could not find a valid synonym.");
            }

            return Ok(synonyms);
        }
    }
}
