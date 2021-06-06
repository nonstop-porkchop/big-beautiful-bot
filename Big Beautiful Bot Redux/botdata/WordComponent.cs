using ServiceStack.DataAnnotations;

namespace Big_Beautiful_Bot_Redux
{
    public class WordComponent
    {
        [AutoIncrement] public long ComponentId { get; set; }

        public string Word { get; set; }
        public int Position { get; set; }
        public int Occurrences { get; set; }
    }
}