using System.Reflection;

namespace UrlShortener.Database.MSSQL;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}