using System.Reflection;

namespace UrlShortener.Database.MongoDB;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}