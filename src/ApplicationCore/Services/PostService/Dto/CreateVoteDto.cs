using System;

namespace Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto
{
    public class CreateVoteDto
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public sbyte Value { get; set; }
    }
}