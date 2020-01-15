using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessageStorage.MsSql.WebApi.DbContextSection.Models
{
    [Table("NoteModel")]
    public class NoteModel
    {
        [Key]
        public long Id { get; private set; }

        public string NoteTitle { get; private set; }
        public string NoteContent { get; private set; }

        public NoteModel(string noteTitle, string noteContent)
        {
            NoteTitle = noteTitle;
            NoteContent = noteContent;
        }
    }
}