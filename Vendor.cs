using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidatableBindableBaseDemo
{
    public class Vendor: ValidatableBindableBase
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

        [CustomValidation(typeof(Vendor), "CheckAcceptableAreaCodes")]
        [RegularExpression(@"^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$", ErrorMessage = "You must enter a 10 digit phone number in a US format")]
        public string Phone
        {
            get { return _Phone; }
            set { SetProperty(ref _Phone, value); }
        }

        public static ValidationResult CheckAcceptableAreaCodes(string phone, ValidationContext context)
        {
            string[] areaCodes = { "305", "786" };
            bool match = false;
            foreach (var ac in areaCodes)
            {
                if (phone != null && phone.Contains(ac)) { match = true; break; }
            }
            if (!match) return new ValidationResult("Only Miami Area Codes accepted");
            else return ValidationResult.Success;
        }
    }
}
