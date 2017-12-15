// The link between a text and a term
namespace LWT.Models
{
    public class TextTerm
    {
        // Primary key is text id and term id
        public int TextID { get; set; }
        public int TermIndex { get; set; }
        public int TermID { get; set; }
        public Text Text { get; set; }
        public Term Term { get; set; }
    }
}