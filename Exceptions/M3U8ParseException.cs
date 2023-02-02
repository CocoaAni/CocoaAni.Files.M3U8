using System.Runtime.Serialization;

namespace CocoaAni.Files.M3U8.Exceptions;

public class M3U8ParseException : Exception
{
    public M3U8ParseException()
    {
    }

    protected M3U8ParseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public M3U8ParseException(string? message) : base(message)
    {
    }

    public M3U8ParseException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}