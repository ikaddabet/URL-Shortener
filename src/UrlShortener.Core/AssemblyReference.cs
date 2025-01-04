using System.Reflection;

namespace UrlShortener.Core;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}