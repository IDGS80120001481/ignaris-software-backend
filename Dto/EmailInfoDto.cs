namespace LignarisBack.Dto
{
    public class EmailInfoDto
    {
        public string Subject { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Folder { get; set; }
        public string Body { get; set; }
        public DateTime Date { get; set; }
    }
}
