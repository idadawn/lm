using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.Infrastructure.Security;
using Poxiao.Systems.Entitys.Dto.Monitor;
using Poxiao.Systems.Interfaces.System;
using System.Runtime.InteropServices;

namespace Poxiao.Systems;

/// <summary>
/// 系统监控
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "Monitor", Order = 215)]
[Route("api/system/[controller]")]
public class MonitorService : IDynamicApiController, ITransient
{
    #region Get

    /// <summary>
    /// 系统监控.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public dynamic GetInfo()
    {
        var flag_Linux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        var flag_Windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        MonitorOutput output = new MonitorOutput();
        if (flag_Linux)
        {
            output.system = MachineHelper.GetSystemInfoLinux();
            output.cpu = MachineHelper.GetCpuInfoLinux();
            output.memory = MachineHelper.GetMemoryInfoLinux();
            output.disk = MachineHelper.GetDiskInfoLinux();
        }
        else if (flag_Windows)
        {
            output.system = MachineHelper.GetSystemInfoWindows();
            output.cpu = MachineHelper.GetCpuInfoWindows();
            output.memory = MachineHelper.GetMemoryInfoWindows();
            output.disk = MachineHelper.GetDiskInfoWindows();
        }

        output.time = DateTime.Now;
        return output;
    }

    #endregion
}