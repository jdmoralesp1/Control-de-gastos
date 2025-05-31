namespace PruebaTecnicaMVC.Utilidades;
public static class TimeUtil
{
    public static DateTime GetDateTimeColombia()
    {
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
    }

    public static string DateTimeToFormatDDMMYYYYHHmm(DateTime dateTime)
        => dateTime.ToString("dd-MM-yyyy HH:mm");

    public static string DateTimeToFormatDDMMYYYY(DateTime dateTime)
        => dateTime.ToString("dd-MM-yyyy");
}
