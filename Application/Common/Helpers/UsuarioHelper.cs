namespace Application.Common.Helpers
{
    public static class UsuarioHelper
    {
        public static string OfuscarNombre(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "Anónimo";

            var trimmed = nombre.Trim();
            if (trimmed.Length <= 2)
                return $"{trimmed[0]}***";

            return $"{trimmed[0]}***{trimmed[^1]}";
        }
    }
}
