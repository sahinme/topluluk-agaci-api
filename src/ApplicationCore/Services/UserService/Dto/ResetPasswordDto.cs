namespace Nnn.ApplicationCore.Services.UserService.Dto
{
    public class ResetPasswordDto
    {
        public string EmailAddress { get; set; }
        public string ResetCode { get; set; }
        public string NewPassword { get; set; }
    }
}