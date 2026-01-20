using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniExcelLibs;
using Xunit;

namespace Poxiao.UnitTests.Lab;

public class HeaderReaderTest
{
    [Fact]
    public void ReadHeaders()
    {
        var filePath = @"e:\project\2025\lm\原始数据表.xlsx";
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return;
        }

        using var stream = File.OpenRead(filePath);
        var rows = stream.Query(useHeaderRow: true).Cast<IDictionary<string, object>>();
        var firstRow = rows.FirstOrDefault();

        if (firstRow != null)
        {
            Console.WriteLine("HEADERS_START");
            foreach (var key in firstRow.Keys)
            {
                Console.WriteLine(key);
            }
            Console.WriteLine("HEADERS_END");
        }
        else
        {
            Console.WriteLine("No data found or empty file.");
        }
    }
}
