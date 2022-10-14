using System.Threading.Tasks;

namespace Arrba.Parser.Mapper
{
    public interface IMapper<TTarget, in TSource>
    {
        Task<TTarget> MapAsync(TSource source);
    }
}
