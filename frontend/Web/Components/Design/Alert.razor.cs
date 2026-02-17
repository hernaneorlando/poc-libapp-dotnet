using LibraryApp.Web.Design;
using Microsoft.AspNetCore.Components;

namespace LibraryApp.Web.Components.Design;

public partial class Alert
{
    /// <summary>
    /// Conteúdo do alerta
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Título do alerta
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Tipo/Variante do alerta
    /// </summary>
    [Parameter]
    public AlertVariant AlertType { get; set; } = AlertVariant.Info;

    /// <summary>
    /// Ícone customizado
    /// </summary>
    [Parameter]
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// Se o alerta pode ser fechado
    /// </summary>
    [Parameter]
    public bool Closeable { get; set; }

    /// <summary>
    /// Callback ao fechar
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// Classes CSS adicionais
    /// </summary>
    [Parameter]
    public string? Class { get; set; }
}
