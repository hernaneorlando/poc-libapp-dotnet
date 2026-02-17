using LibraryApp.Web.Design;
using Microsoft.AspNetCore.Components;

namespace LibraryApp.Web.Components.Design;

public partial class Modal
{
    /// <summary>
    /// Se o modal está aberto
    /// </summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>
    /// Título do modal
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Conteúdo do modal
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Conteúdo customizado do footer
    /// </summary>
    [Parameter]
    public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Se mostra o header
    /// </summary>
    [Parameter]
    public bool ShowHeader { get; set; } = true;

    /// <summary>
    /// Se mostra o footer padrão (Cancelar/Confirmar)
    /// </summary>
    [Parameter]
    public bool ShowDefaultFooter { get; set; } = false;

    /// <summary>
    /// Se clicando fora fecha o modal
    /// </summary>
    [Parameter]
    public bool CloseOnBackdropClick { get; set; } = true;

    /// <summary>
    /// Tamanho do modal
    /// </summary>
    [Parameter]
    public ModalSize Size { get; set; } = ModalSize.Medium;

    /// <summary>
    /// Callback ao fechar
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// Callback ao confirmar
    /// </summary>
    [Parameter]
    public EventCallback OnConfirm { get; set; }

    /// <summary>
    /// Método para fechar o modal
    /// </summary>
    public async Task Close()
    {
        await OnClose.InvokeAsync();
    }

    private async Task OnBackdropClick()
    {
        if (CloseOnBackdropClick)
        {
            await Close();
        }
    }

    private string GetPositionClass() => Size switch
    {
        ModalSize.Small => "w-full max-w-sm top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2",
        ModalSize.Large => "w-full max-w-2xl top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2",
        ModalSize.ExtraLarge => "w-full max-w-4xl top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2",
        _ => "w-full max-w-md top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2" // Medium
    };
}
