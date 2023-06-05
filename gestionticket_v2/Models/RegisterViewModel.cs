using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Le champ {0} est obligatoire")]
    public string Nom { get; set; }

    [Required(ErrorMessage = "Le champ {0} est obligatoire")]
    public string Prenom { get; set; }

    [Required(ErrorMessage = "Le champ {0} est obligatoire")]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    public string UserType { get; set; } // Can be "Client" or "MembreSupportTechnique"
}
