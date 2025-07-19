namespace CompanyDirectory.API.Services;

public static class DateTimeExtensions
{
    public static DateTime ToUtc(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime;

        return new DateTime(dateTime.Ticks, DateTimeKind.Utc);
    }
}
