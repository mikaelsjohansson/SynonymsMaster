namespace SynonymsMaster.API.Actions
{
    public interface IStoreSynonym
    {
        void Store(string word, string to);
    }

    public class StoreSynonym(ISynonymContext context) : IStoreSynonym
    {

        public void Store(string word, string to)
        {
            context.Add(word, to);
        }
    }
}