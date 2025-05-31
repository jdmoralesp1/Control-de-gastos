using System.Globalization;

namespace PruebaTecnicaMVC.Utilidades;
public static class StringUtil
{
    public static string ConvertToMoneyFormat(decimal amount)
        => amount.ToString("C", new CultureInfo("es-CO"));

    public static string GetMonthName(int month)
    {
        string nombreMes = CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.GetMonthName(month);

        return char.ToUpper(nombreMes[0]) + nombreMes.Substring(1);
    }
}
