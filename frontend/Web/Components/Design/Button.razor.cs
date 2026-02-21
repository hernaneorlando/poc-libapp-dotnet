using LibraryApp.Web.Design;
using Microsoft.AspNetCore.Components;

namespace LibraryApp.Web.Components.Design;

public partial class Button
{
    /// <summary>
    /// Conteúdo do botão
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Tipo do botão: button, submit, reset
    /// </summary>
    [Parameter]
    public string Type { get; set; } = "button";

    /// <summary>
    /// Tamanho do botão
    /// </summary>
    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.Medium;

    /// <summary>
    /// Variante visual do botão
    /// </summary>
    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;

    /// <summary>
    /// Se o botão está desabilitado
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Se o botão ocupa a largura total do container
    /// </summary>
    [Parameter]
    public bool FullWidth { get; set; }

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

    /// <summary>
    /// Índice de tabulação do botão
    /// </summary>
    [Parameter]
    public int TabIndex { get; set; } = 0;

    /// <summary>
    /// Callback ao clicar
    /// </summary>
    [Parameter]
    public EventCallback OnClick { get; set; }

    /// <summary>
    /// Handler para invocar o callback OnClick
    /// </summary>
    private async Task HandleClick()
    {
        await OnClick.InvokeAsync();
    }
}
