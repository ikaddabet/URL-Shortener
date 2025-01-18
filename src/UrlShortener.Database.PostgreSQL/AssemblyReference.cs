using System.Reflection;

namespace UrlShortener.Database.PostgreSQL;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}