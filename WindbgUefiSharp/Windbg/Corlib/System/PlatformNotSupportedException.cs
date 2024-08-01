namespace System;

public class PlatformNotSupportedException: Exception
{
    public PlatformNotSupportedException()
    {
    }

    public PlatformNotSupportedException(string str) : base(str)
    {
        Console.WriteLine("PlatformNotSupportedException:=>"+str);
        while (true)
        {
            System.Threading.Thread.Sleep(1000);
        }

    }
}