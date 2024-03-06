using System.Reflection;

namespace JaaS.Demos.Utility;

public static class FileUtility
{
    public static Stream LoadStreamFromEmbeddedResource(string resource)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(resource);
        Assert.IsNotNull(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}
