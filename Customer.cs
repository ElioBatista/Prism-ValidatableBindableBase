using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ValidatableBindableBaseDemo
{
    public class Customer : ValidatableBindableBase
    {
        private string _FirstName;
        private string _LastName;
        private string _Phone;

        [Required]
        public string FirstName
        {
            get { return _FirstName; }
            set 
            { 
                SetProperty(ref _FirstName, value); 
                OnPropertyChanged(() => FullName); 
            }
        }

        [Required]
        public string LastName
        {
            get { return _LastName; }
            set 
            { 
                SetProperty(ref _LastName, value); 
                OnPropertyChanged(() => FullName); 
            }
        }

        public string FullName { get { return _FirstName + " " + _LastName; } }

        [CustomValidation(typeof(Customer), "CheckAcceptableAreaCodes")]
        [RegularExpression(@"^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$",ErrorMessage="You must enter a 10 digit phone number in a US format")]
        public string Phone
        {
            get { return _Phone; }
            set { SetProperty(ref _Phone, value); }
        }

        public static ValidationResult CheckAcceptableAreaCodes(string phone, ValidationContext context)
        {
            string[] areaCodes = { "760", "442" };
            bool match = false;
            foreach (var ac in areaCodes)
            {
                if (phone != null && phone.Contains(ac)) { match = true; break; }
            }
            if (!match) return new ValidationResult("Only San Diego Area Codes accepted");
            else return ValidationResult.Success;
        }
    }
}
