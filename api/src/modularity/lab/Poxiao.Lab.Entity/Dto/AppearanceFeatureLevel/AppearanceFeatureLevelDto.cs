using System.Text.Json.Serialization;
using Poxiao.Infrastructure.Contracts;
using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity.Entity;

namespace Poxiao.Lab.Entity.Dto.AppearanceFeatureLevel;

public class AppearanceFeatureLevelCrInput : AppearanceFeatureLevelEntity
{
}

public class AppearanceFeatureLevelUpInput : AppearanceFeatureLevelEntity
{
}

public class AppearanceFeatureLevelListQuery : PageInputBase
{
    public string Keyword { get; set; }
    public bool? Enabled { get; set; }
}

public class AppearanceFeatureLevelListOutput : AppearanceFeatureLevelEntity
{
}

public class AppearanceFeatureLevelInfoOutput : AppearanceFeatureLevelEntity
{
}
