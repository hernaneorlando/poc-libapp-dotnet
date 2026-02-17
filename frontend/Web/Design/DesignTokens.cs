namespace LibraryApp.Web.Design;

/// <summary>
/// Design System Tokens - Paleta de cores, espaçamento, tipografia
/// Todos os valores definem o padrão visual da aplicação
/// </summary>
public static class DesignTokens
{
    /// <summary>
    /// Cores Primárias - Laranja queimado (tema biblioteca)
    /// </summary>
    public static class Colors
    {
        public static class Primary
        {
            public const string Base = "e68a3c";
            public const string Light = "f0ba84";
            public const string Dark = "cc7131";
            public const string Darker = "984220";
        }

        public static class Secondary
        {
            public const string Base = "c99a56";
            public const string Light = "dfbf8f";
            public const string Dark = "b88948";
        }

        public static class Accent
        {
            public const string Base = "86a450";
            public const string Light = "b2c690";
            public const string Dark = "759044";
        }

        public static class Neutral
        {
            public const string Lightest = "faf9f7";
            public const string Light = "f5f3f0";
            public const string Base = "cdbdb0";
            public const string Dark = "827569";
            public const string Darkest = "5c4c42";
        }

        public static class Status
        {
            public const string Success = "22c55e";
            public const string Warning = "f59e0b";
            public const string Error = "ef4444";
            public const string Info = "3b82f6";
        }
    }

    /// <summary>
    /// Espaçamento - Escala baseada em 4px
    /// </summary>
    public static class Spacing
    {
        public const string Xs = "0.25rem";      // 4px
        public const string Sm = "0.5rem";       // 8px
        public const string Md = "1rem";         // 16px
        public const string Lg = "1.5rem";       // 24px
        public const string Xl = "2rem";         // 32px
        public const string Xl2 = "2.5rem";      // 40px
        public const string Xl3 = "3rem";        // 48px
    }

    /// <summary>
    /// Tipografia - Tamanhos e pesos
    /// </summary>
    public static class Typography
    {
        public const string FontSans = "Segoe UI, Roboto, Helvetica, Arial, sans-serif";
        public const string FontSerif = "Georgia, Garamond, serif";
        public const string FontMono = "Fira Code, Courier New, monospace";

        public const string SizeXs = "0.75rem";
        public const string SizeSm = "0.875rem";
        public const string SizeBase = "1rem";
        public const string SizeLg = "1.125rem";
        public const string SizeXl = "1.25rem";
        public const string Size2Xl = "1.5rem";
        public const string Size3Xl = "1.875rem";
        public const string Size4Xl = "2.25rem";

        public const int WeightRegular = 400;
        public const int WeightMedium = 500;
        public const int WeightSemibold = 600;
        public const int WeightBold = 700;
    }

    /// <summary>
    /// Border Radius - Cantos arredondados
    /// </summary>
    public static class BorderRadius
    {
        public const string Xs = "0.25rem";
        public const string Sm = "0.375rem";
        public const string Base = "0.5rem";
        public const string Lg = "0.75rem";
        public const string Xl = "1rem";
        public const string Full = "9999px";
    }

    /// <summary>
    /// Sombras - Profundidade e elevação
    /// </summary>
    public static class Shadows
    {
        public const string None = "none";
        public const string Xs = "0 1px 2px 0 rgb(0 0 0 / 0.05)";
        public const string Sm = "0 1px 2px 0 rgb(0 0 0 / 0.05)";
        public const string Base = "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)";
        public const string Md = "0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)";
        public const string Lg = "0 25px 50px -12px rgb(0 0 0 / 0.25)";
        public const string Xl = "0 20px 25px -5px rgba(230, 138, 60, 0.15)";
    }

    /// <summary>
    /// Transições - Duração e easing
    /// </summary>
    public static class Transitions
    {
        public const string Fast = "150ms ease-in-out";
        public const string Base = "200ms ease-in-out";
        public const string Slow = "300ms ease-in-out";
    }

    /// <summary>
    /// Breakpoints - Responsive design
    /// </summary>
    public static class Breakpoints
    {
        public const int Sm = 640;
        public const int Md = 768;
        public const int Lg = 1024;
        public const int Xl = 1280;
        public const int Xl2 = 1536;
    }
}
