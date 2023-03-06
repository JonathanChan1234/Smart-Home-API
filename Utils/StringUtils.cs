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