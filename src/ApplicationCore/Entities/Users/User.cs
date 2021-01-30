using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.Users
{
    public class User:BaseEntity,IAggregateRoot
    {
        public string ProfileImagePath { get; set; }
        public char Gender { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public long SPoint { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }

        [MaxLength(181)]
        public string Bio { get; set; }
        public string VerificationCode { get; set; }
        [DefaultValue(false)]
        public bool IsAdmin { get; set; }
        public string ResetPasswordCode { get; set; }
        [DefaultValue(false)]
        public bool EmailVerified { get; set; }
        private DateTime? _expirationDate = null;
        public DateTime ExpirationDate    
        {
            get
            {
                return _expirationDate.HasValue
                    ? _expirationDate.Value
                    : DateTime.Now.AddMonths(1);
            }

            set { _expirationDate = value; }
        }

        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<CommunityUser> Communities { get; set; }
    }
    
}