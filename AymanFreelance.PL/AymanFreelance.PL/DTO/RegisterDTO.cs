﻿using AymanFreelance.DAL.BaseEntity;
using AymanFreelance.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AymanFreelance.PL.DTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [MaxLength(50, ErrorMessage = "Password must be at max 50 character")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [MaxLength(10, ErrorMessage = "First Name must be at max 10 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [MaxLength(10, ErrorMessage = "Last Name must be at max 10 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(200, ErrorMessage = "Last Name must be at max 200 character")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [MaxLength(50, ErrorMessage = "Phone must be at max 50 character")]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public int? CountryTBLId { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public int? GenderTBLId { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [Display(Name = "Type")]
        public int? UserTypeTBLId { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [Display(Name = "Profession")]
        public int? ProfessionTBLId { get; set; }

        public IEnumerable<SelectListItem> CountryOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> GenderOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TypeOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ProfessionOptions { get; set; } = new List<SelectListItem>();

    }
}
