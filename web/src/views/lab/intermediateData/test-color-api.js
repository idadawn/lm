// 测试颜色API的JavaScript脚本

// 测试获取颜色配置
async function testGetColors() {
  try {
    const response = await fetch('/api/lab/intermediate-data-color/get-colors', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        productSpecId: 'test-spec-id'
      })
    });

    if (response.ok) {
      const data = await response.json();
    } else {
      console.error('❌ GetColors API 失败:', response.status, response.statusText);
    }
  } catch (error) {
    console.error('❌ GetColors API 错误:', error);
  }
}

// 测试保存单个单元格颜色
async function testSaveCellColor() {
  try {
    const response = await fetch('/api/lab/intermediate-data-color/save-cell-color?intermediateDataId=test-data-1&fieldName=testField&colorValue=%23FF0000&productSpecId=test-spec-id', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      }
    });

    if (response.ok) {
      const data = await response.json();
    } else {
      console.error('❌ SaveCellColor API 失败:', response.status, response.statusText);
    }
  } catch (error) {
    console.error('❌ SaveCellColor API 错误:', error);
  }
}

// 运行测试
testGetColors();
testSaveCellColor();