namespace smart_home_server.Utils;

public class StringUtils
{
    public static string RandomString(int length)
    {
        var random = new Random();
        var randomString = "";
        for (int i = 0; i < length; ++i)
        {
            // generate random character in between 33(!) to 126 (~)
            char randomChar = (char)(random.Next(126 - 33) + 33);
            randomString += randomChar;
        }
        return randomString;
    }
}
public static class StringUtilsExtension
{
    public static string FirstCharToLowerCase(this string str)
    {
        if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
            return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

        return str;
    }

    public static string FirstCharToUppercase(this string str)
    {
        if (!string.IsNullOrEmpty(str) && char.IsLower(str[0]))
            return str.Length == 1 ? char.ToUpper(str[0]).ToString() : char.ToUpper(str[0]) + str[1..];

        return str;
    }
}