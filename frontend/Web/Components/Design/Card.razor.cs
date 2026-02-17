using LibraryApp.Web.Design;
using Microsoft.AspNetCore.Components;

namespace LibraryApp.Web.Components.Design;

public partial class Card
{
    /// <summary>
    /// Conteúdo principal do card
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Título do card
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Conteúdo customizado do título
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// Conteúdo do header (abaixo do título)
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// Conteúdo do footer
    /// </summary>
    [Parameter]
    public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Nível de elevação (sombra)
    /// </summary>
    [Parameter]
    public CardElevation Elevation { get; set; } = CardElevation.Medium;

    /// <summary>
    /// Se mostra borda
    /// </summary>
    [Parameter]
    public bool ShowBorder { get; set; } = false;

    /// <summary>
    /// Classes CSS adicionais
    /// </summary>
    [Parameter]
    public string? Class { get; set; }
}
