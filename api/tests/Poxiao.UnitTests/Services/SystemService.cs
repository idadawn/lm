using Poxiao.DependencyInjection;

namespace Poxiao.UnitTests;

public class SystemService : ISystemService, ITransient
{
    public string GetName() => "Poxiao";
}