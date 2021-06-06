using ServiceStack.DataAnnotations;

namespace BBB.botdata
{
    public class WordComponent
    {
        [AutoIncrement] public long ComponentId { get; set; }

        public string Word { get; set; }
        public int Position { get; set; }
        public int Occurrences { get; set; }
    }
}