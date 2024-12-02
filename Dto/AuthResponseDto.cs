namespace LignarisBack.Dto
{
    public class AuthResponseDto
    {
        public string? Token { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Role { get; set; }
        public DateTime LastSession { get; set; }

    }
}
