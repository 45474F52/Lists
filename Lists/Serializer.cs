using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lists
{
    internal sealed class Serializer
    {
        public readonly string path;

        public Serializer() => path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "lists.txt");

        public async Task Serialize(string text)
        {
            using FileStream fs = new(path, FileMode.Create, FileAccess.Write, FileShare.None);
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            await fs.WriteAsync(buffer);
        }

        public async Task<string> Deserialize()
        {
            using FileStream fs = File.OpenRead(path);
            byte[] buffer = new byte[fs.Length];
            await fs.ReadAsync(buffer);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}