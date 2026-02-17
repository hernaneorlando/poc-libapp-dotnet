using LibraryApp.Web.Design;
using Microsoft.AspNetCore.Components;

namespace LibraryApp.Web.Components.Design;

public partial class Badge
{
    /// <summary>
    /// Conteúdo do badge
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Variante visual
    /// </summary>
    [Parameter]
    public BadgeVariant Variant { get; set; } = BadgeVariant.Primary;

    /// <summary>
    /// Tamanho do badge
    /// </summary>
    [Parameter]
    public BadgeSize Size { get; set; } = BadgeSize.Medium;

    /// <summary>
    /// Ícone antes do texto
    /// </summary>
    [Parameter]
    public RenderFragment? IconStart { get; set; }

    /// <summary>
    /// Ícone depois do texto
    /// </summary>
    [Parameter]
    public RenderFragment? IconEnd { get; set; }

    /// <summary>
    /// Classes CSS adicionais
    /// </summary>
    [Parameter]
    public string? Class { get; set; }
}
