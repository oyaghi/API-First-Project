using Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Core.Models
{
    public class User : ITenantEntity
    { 
        public int Id { get; set; }
       
        public string Email { get; set; } = string.Empty;
      
        public string FirstName { get; set; } = string.Empty;
        
        public string Lastname { get; set; } = string.Empty;
       
        public Gender Gender { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;

        public Settings Setting { get; set; } = null!;

        public class Settings
        {
            public string Language { get; set; } = null!; 
            public string Color { get; set; } = null!;
            public string Theme { get; set; } = null!;
        }
    }
}
