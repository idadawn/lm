// 调试自定义排序模态框的脚本
(function() {
    'use strict';

    console.log('=== 自定义排序模态框调试开始 ===');

    // 等待Vue组件加载完成
    setTimeout(function() {
        console.log('开始检查组件状态...');

        // 1. 检查按钮是否存在
        const sortButton = document.querySelector('.custom-sort-control .ant-btn');
        if (sortButton) {
            console.log('✅ 找到自定义排序按钮:', sortButton);
            console.log('按钮文本:', sortButton.textContent.trim());

            // 2. 检查点击事件
            sortButton.addEventListener('click', function(e) {
                console.log('🖱️ 按钮被点击:', e);
                console.log('点击时间:', new Date().toLocaleString());
            });
        } else {
            console.error('❌ 未找到自定义排序按钮');
        }

        // 3. 检查是否有模态框元素
        const modalElements = document.querySelectorAll('.ant-modal-root, .ant-modal-mask, .ant-modal-wrap');
        console.log('📋 找到的模态框相关元素数量:', modalElements.length);
        modalElements.forEach((el, index) => {
            console.log(`模态框元素 ${index + 1}:`, el);
            console.log(`  - 类名:`, el.className);
            console.log(`  - 样式:`, el.style.cssText);
            console.log(`  - 显示状态:`, window.getComputedStyle(el).display);
            console.log(`  - 可见性:`, window.getComputedStyle(el).visibility);
            console.log(`  - Z-index:`, window.getComputedStyle(el).zIndex);
        });

        // 4. 检查是否有隐藏的模态框
        const hiddenModals = document.querySelectorAll('.ant-modal[style*="display: none"], .ant-modal[style*="visibility: hidden"]');
        console.log('🔍 隐藏的模态框数量:', hiddenModals.length);

        // 5. 检查z-index冲突
        const highZIndexElements = Array.from(document.querySelectorAll('*')).filter(el => {
            const zIndex = window.getComputedStyle(el).zIndex;
            return zIndex && parseInt(zIndex) > 1000;
        });
        console.log('📊 Z-index大于1000的元素数量:', highZIndexElements.length);
        highZIndexElements.forEach((el, index) => {
            console.log(`高Z-index元素 ${index + 1}:`, el);
            console.log(`  - Z-index:`, window.getComputedStyle(el).zIndex);
            console.log(`  - 类名:`, el.className);
        });

        // 6. 检查控制台错误
        const originalError = console.error;
        console.error = function() {
            originalError.apply(console, arguments);
            console.log('❌ 捕获到错误:', arguments);
        };

        // 7. 尝试手动触发（需要访问Vue实例）
        console.log('🔧 尝试访问Vue组件...');
        // 注意：这需要Vue Devtools或特定的调试环境

        // 8. 检查是否有任何覆盖层
        const overlays = document.querySelectorAll('.ant-modal-mask, .ant-dropdown-hidden, .ant-select-dropdown-hidden');
        console.log('🎭 找到的覆盖层数量:', overlays.length);
        overlays.forEach((overlay, index) => {
            console.log(`覆盖层 ${index + 1}:`, overlay);
            console.log(`  - 显示状态:`, window.getComputedStyle(overlay).display);
        });

        console.log('=== 调试信息收集完成 ===');
        console.log('');
        console.log('💡 下一步建议：');
        console.log('1. 点击自定义排序按钮');
        console.log('2. 观察控制台输出');
        console.log('3. 如果有错误，请提供截图或复制错误信息');
        console.log('4. 检查网络面板是否有失败的请求');

    }, 2000); // 等待2秒让页面完全加载

})();