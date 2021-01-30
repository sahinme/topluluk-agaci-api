using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.Suggesstions
{
    public class Suggestion:BaseEntity,IAggregateRoot
    {
        public string Content { get; set; }
        public string Email { get; set; }
    }
}