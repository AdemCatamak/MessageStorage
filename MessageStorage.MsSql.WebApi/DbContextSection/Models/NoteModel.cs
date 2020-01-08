namespace MessageStorage.MsSql.WebApi.DbContextSection.Models
{
    public class NoteModel
    {
        public int Id { get; private set; }
        public string NoteTitle { get; private set; }
        public string NoteContent { get; private set; }

        public NoteModel(string noteTitle, string noteContent)
        {
            NoteTitle = noteTitle;
            NoteContent = noteContent;
        }
    }
}