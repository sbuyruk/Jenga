using System.ComponentModel.DataAnnotations;

namespace Jenga.Models.Attributes
{
    /**
     * SB Telefon numarası doğrulama için özel bir doğrulama sınıfı 
     * 
     * örnek kullanımı
     public class TasinmazBagisci : BaseModel
        {
            [ValidateNever]
            [DisplayName("Telefon 1")]
            [TelefonValidation]
            public string? Telefon1 { get; set; }

            [ValidateNever]
            [DisplayName("Telefon 2")]
            [TelefonValidation]
            public string? Telefon2 { get; set; }
        }
     *
     *Regex Açıklaması:
        ^ ve $: İfadenin başlangıcını ve sonunu temsil eder.
        \+90: Telefon numarasının +90 ile başlamasını sağlar.
        \d{10}: 10 basamaklı rakamlardan oluşmasını bekler.
     **/

    public class TelefonValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string telefon && !string.IsNullOrEmpty(telefon))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(telefon, @"^\+90\d{10}$"))
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult("Telefon numarası '+90XXXXXXXXXX' formatında olmalıdır.");
            }
            return ValidationResult.Success; // Boşsa geçerli kabul edilir.
        }
    }
}
