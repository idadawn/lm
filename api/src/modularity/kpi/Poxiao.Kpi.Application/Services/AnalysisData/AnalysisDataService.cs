using MathNet.Numerics;
using MathNet.Numerics.Statistics;

namespace Poxiao.Kpi.Application;

/// <summary>
/// 数据分析服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-12-21.
/// </summary>
public class AnalysisDataService : IAnalysisDataService, ITransient
{
    /// <summary>
    /// 控制图系数.
    /// </summary>
    private readonly Dictionary<string, double[]> _factorChart = new Dictionary<string, double[]>()
    {
        { "A2", new double[9] { 1.880, 1.023, 0.729, 0.577, 0.483, 0.419, 0.373, 0.337, 0.308 }},
        { "D3", new double[9] { 0, 0, 0, 0, 0, 0.076, 0.136, 0.184, 0.223 }},
        { "D4", new double[9] { 3.267, 2.574, 2.282, 2.114, 2.004, 1.924, 1.864, 1.816, 1.777 }},
    };

    public AnalysisDataService()
    {
    }

    public AnalysisDataNormalListOutput GetNHChart()
    {
        // 正态分布
        var save = 4; // 保留几位小数
        var axis = new AnalysisDataNormalListOutput();
        var dataNum = 100; // 数据总数
        var nmData = Generate.Normal(dataNum, 0, 1); // 生成符合正态分布的数据
        var ds = new DescriptiveStatistics(nmData, true);

        for (var i = -3; i < 4; i++)
        {
            var x = ds.Mean + (ds.StandardDeviation * i);
            var y = NormalFormula(x, ds.Mean, ds.StandardDeviation);
            axis.XAxis.Add(Math.Round(x, save, MidpointRounding.ToNegativeInfinity));
            axis.YAxis.Add(Math.Round(y, save, MidpointRounding.ToNegativeInfinity));
        }

        // 直方图
        // K=1+3.3lgN 斯特杰斯-经验公式
        var colSum = (int)Math.Round(1 + (3.3 * Math.Log10(dataNum))); // 组数

        var minSpeed = ds.Minimum - Math.Round(Math.Pow(0.1, 4), save, MidpointRounding.ToNegativeInfinity); // x轴下限
        var maxSpeed = ds.Maximum + Math.Round(Math.Pow(0.1, 4), save, MidpointRounding.ToNegativeInfinity); // x轴上限
        var colSpeed = (maxSpeed - minSpeed) / colSum; // 间距
        var hg = new Histogram(nmData, colSum, minSpeed, maxSpeed);

        for (var i = 0; i < hg.BucketCount; i++)
        {
            axis.XAxisHistogram.Add(Math.Round((hg[i].LowerBound + hg[i].UpperBound) / 2, save));
            axis.YAxisHistogram.Add(Math.Round(hg[i].Count / dataNum / colSpeed, save));
        }

        return axis;
    }

    /// <summary>
    /// 正态分布公式.
    /// </summary>
    /// <param name="x">x坐标.</param>
    /// <param name="mean">平均值.</param>
    /// <param name="standardDeviation">标准差.</param>
    /// <returns></returns>
    private double NormalFormula(double x, double mean, double standardDeviation)
    {
        return (1 / (Math.Sqrt(2 * Math.PI) * standardDeviation)) * Math.Pow(Math.E, -(Math.Pow(x - mean, 2) / (2 * Math.Pow(standardDeviation, 2))));
    }

    // 测试数据，用于均值-极差图
    private List<List<double>> _xRProtoData = new List<List<double>>(){
            new List<double>(){10.948,10.903,10.947,10.962,10.984},
            new List<double>(){10.913,10.969,10.949,10.980,10.939},
            new List<double>(){10.973,10.909,10.940,10.954,10.929},
            new List<double>(){10.923,10.940,10.949,10.949,10.934},
            new List<double>(){11.020,10.959,10.920,10.983,10.996},
            new List<double>(){10.920,10.942,10.933,10.981,10.946},
            new List<double>(){10.983,10.912,10.959,10.901,10.929}, 
            new List<double>(){10.959,10.931,10.938,10.932,10.959},
            new List<double>(){10.939,10.926,10.970,10.964,10.953},
            new List<double>(){10.910,10.949,10.928,10.956,10.924},
            new List<double>(){10.937,10.942,10.985,10.944,10.966},
            new List<double>(){10.968,10.951,10.925,10.920,10.981},
            new List<double>(){10.984,10.954,10.947,10.948,10.956},
            new List<double>(){10.925,10.970,10.940,10.919,10.928},
            new List<double>(){11.026,10.978,10.970,10.958,10.925},
            new List<double>(){10.950,10.951,10.932,10.936,10.928},
            new List<double>(){10.961,10.953,10.969,10.987,10.955},
            new List<double>(){10.974,10.971,10.928,10.955,11.009},
            new List<double>(){10.999,10.930,10.951,10.959,10.961},
            new List<double>(){10.949,10.920,10.917,10.981,10.931},
            new List<double>(){10.952,10.942,10.954,10.958,10.973},
            new List<double>(){10.923,10.966,10.997,10.942,10.941},
            new List<double>(){10.947,10.944,10.932,10.958,10.949},
            new List<double>(){11.000,10.985,10.904,10.937,10.977},
            new List<double>(){10.939,10.916,10.956,10.926,10.957},
    };

    public AnalysisXbarRbarOutput GetXRChart()
    {
        var xrOut = new AnalysisXbarRbarOutput();
        if (_factorChart["A2"].Length < _xRProtoData[0].Count - 1) return xrOut;

        // 设置均值-极差图数据
        foreach (var item in _xRProtoData)
        {
            xrOut.Average.Axis.Add(Math.Round(item.Mean(), 4));
            xrOut.Range.Axis.Add(Math.Round(item.Maximum() - item.Minimum(), 4));
        }

        // 设置均值图管制限
        var acl = xrOut.Average.Axis.Mean();
        var a2R = _factorChart["A2"][_xRProtoData[0].Count - 2] * xrOut.Range.Axis.Mean();
        xrOut.Average.UCL = Math.Round(acl + a2R, 5);
        xrOut.Average.LCL = Math.Round(acl - a2R, 5);
        xrOut.Average.CL = Math.Round(acl, 5);

        // 设置极差图管制限
        var rcl = xrOut.Range.Axis.Mean();
        xrOut.Range.UCL = Math.Round(rcl * _factorChart["D4"][_xRProtoData[0].Count - 2], 4);
        xrOut.Range.LCL = Math.Round(rcl * _factorChart["D3"][_xRProtoData[0].Count - 2], 4);
        xrOut.Range.CL = Math.Round(rcl, 4);

        return xrOut;
    }
}