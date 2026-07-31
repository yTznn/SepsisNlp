using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SepsisNlp.Application.Common.Helpers;

public static class TextNormalizerHelper
{
    public static string NormalizarTextoClinico(string textoBruto)
    {
        if (string.IsNullOrWhiteSpace(textoBruto))
            return string.Empty;

        // 1. Converte tudo para minúsculo
        var texto = textoBruto.ToLowerInvariant();

        // 2. Remove acentos e cedilha
        texto = RemoverAcentos(texto);

        // 3. Remove caracteres especiais (MANTENDO /, º, °, e pontuação clínica)
        texto = Regex.Replace(texto, @"[^a-z0-9\s.,;?!\/º°-]", "");

        // 4. Remove espaços duplos ou quebras de linha em excesso
        texto = Regex.Replace(texto, @"\s+", " ").Trim();

        return texto;
    }

    private static string RemoverAcentos(string texto)
    {
        var normalizedString = texto.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}