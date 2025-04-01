using System.ComponentModel.DataAnnotations;

namespace MVC.WebApp.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        public GenderType Gender { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }
        [Required]
        public string? BirthPlace { get; set; }
        public bool IsGraduated { get; set; }

        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }
    }
}