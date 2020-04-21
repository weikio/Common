using System;

namespace Weikio.AspNetCore.Common
{
    public class ConfigureOptionsWithDependency<TOptions, TDependency1>
    {
        public Action<TOptions, TDependency1> Action { get; set; }
    }
}
