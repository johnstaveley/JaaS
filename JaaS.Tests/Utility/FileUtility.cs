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
    public static byte[] LoadBytesFromEmbeddedResource(string resource)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(resource);
        Assert.IsNotNull(stream);
        stream.Seek(0, SeekOrigin.Begin);
        using (MemoryStream ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }

}
