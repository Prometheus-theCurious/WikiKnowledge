using System.ComponentModel.DataAnnotations;

namespace WikiKnowledge.Models
{
    public class Cronologia
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
