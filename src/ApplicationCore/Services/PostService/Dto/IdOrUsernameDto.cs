using System;

namespace Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto
{
    public class IdOrUsernameDto
    {
        public string Username { get; set; }
        public Guid Id { get; set; }
    }
}