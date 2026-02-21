using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LibraryApp.Web.Components.Design;

public partial class Input
{
    /// <summary>
    /// Label do input
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Texto de placeholder
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Valor atual do input
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Callback quando o valor muda (para @bind-Value)
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Tipo HTML do input (text, email, password, number, etc)
    /// </summary>
    [Parameter]
    public string HtmlType { get; set; } = "text";

    /// <summary>
    /// Se o input é obrigatório
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Se o input está desabilitado
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Variante visual
    /// </summary>
    [Parameter]
    public InputVariant InputType { get; set; } = InputVariant.Default;

    /// <summary>
    /// Texto de ajuda abaixo do input
    /// </summary>
    [Parameter]
    public string? HelperText { get; set; }

    /// <summary>
    /// Mensagem de erro
    /// </summary>
    [Parameter]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Classes CSS adicionais
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    ///  TabIndex para controle de foco via teclado 
    /// </summary>
    [Parameter]
    public int TabIndex { get; set; } = 0;

    /// <summary>
    /// Callback ao mudar valor
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnChange { get; set; }

    /// <summary>
    /// Callback enquanto digita
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnInput { get; set; }

    /// <summary>
    /// Callback ao pressionar tecla
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyUp { get; set; }

    /// <summary>
    /// Referência ao elemento HTML do input
    /// </summary>
    private ElementReference InputElement { get; set; }

    /// <summary>
    /// Foca o input programaticamente
    /// </summary>
    public async Task FocusAsync()
    {
        await InputElement.FocusAsync();
    }
}
