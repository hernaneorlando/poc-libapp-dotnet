using LibraryApp.Web.Design;
using Microsoft.AspNetCore.Components;

namespace LibraryApp.Web.Components.Design;

public partial class Avatar
{
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    [Parameter]
    public string? FullName { get; set; }

    /// <summary>
    /// URL da imagem de avatar
    /// </summary>
    [Parameter]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Iniciais do nome (fallback)
    /// </summary>
    [Parameter]
    public string? Initials { get; set; }

    /// <summary>
    /// Tamanho do avatar
    /// </summary>
    [Parameter]
    public AvatarSize Size { get; set; } = AvatarSize.Medium;

    /// <summary>
    /// Variante visual
    /// </summary>
    [Parameter]
    public AvatarVariant Variant { get; set; } = AvatarVariant.Primary;

    /// <summary>
    /// Classes CSS adicionais
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    protected override void OnInitialized()
    {
        // Auto-gerar iniciais se nome for fornecido e iniciais não forem
        if (!string.IsNullOrEmpty(FullName) && string.IsNullOrEmpty(Initials))
        {
            var parts = FullName.Split(' ');
            if (parts.Length >= 2)
            {
                Initials = $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            }
            else if (parts.Length == 1)
            {
                Initials = parts[0][0].ToString().ToUpper();
            }
        }
    }
}
