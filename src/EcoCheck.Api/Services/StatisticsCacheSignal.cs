using Microsoft.Extensions.Primitives;

namespace EcoCheck.Api.Services;

/// <summary>
/// Sinal compartilhado (singleton) que expira de uma vez todas as entradas de cache
/// das estatísticas, de qualquer filtro, quando uma nova participação é registrada.
/// </summary>
public sealed class StatisticsCacheSignal : IDisposable
{
    private readonly Lock _lock = new();
    private CancellationTokenSource _source = new();

    public IChangeToken CreateToken()
    {
        lock (_lock)
        {
            return new CancellationChangeToken(_source.Token);
        }
    }

    public void Reset()
    {
        CancellationTokenSource previous;
        lock (_lock)
        {
            previous = _source;
            _source = new CancellationTokenSource();
        }

        previous.Cancel();
        previous.Dispose();
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _source.Dispose();
        }
    }
}
