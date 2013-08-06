using System.Threading.Tasks;

using Microsoft.Owin;

namespace Cassette.Owin
{
    public interface ICassetteRequestHandler
    {
        Task ProcessRequest(IOwinContext context, string path);
    }
}