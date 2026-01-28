// 调试自定义排序模态框的脚本
(function() {
    'use strict';


    // 等待Vue组件加载完成
    setTimeout(function() {

        // 1. 检查按钮是否存在
        const sortButton = document.querySelector('.custom-sort-control .ant-btn');
        if (sortButton) {

            // 2. 检查点击事件
            sortButton.addEventListener('click', function(e) {
            });
        } else {
            console.error('❌ 未找到自定义排序按钮');
        }

        // 3. 检查是否有模态框元素
        const modalElements = document.querySelectorAll('.ant-modal-root, .ant-modal-mask, .ant-modal-wrap');
        modalElements.forEach((el, index) => {
        });

        // 4. 检查是否有隐藏的模态框
        const hiddenModals = document.querySelectorAll('.ant-modal[style*="display: none"], .ant-modal[style*="visibility: hidden"]');

        // 5. 检查z-index冲突
        const highZIndexElements = Array.from(document.querySelectorAll('*')).filter(el => {
            const zIndex = window.getComputedStyle(el).zIndex;
            return zIndex && parseInt(zIndex) > 1000;
        });
        highZIndexElements.forEach((el, index) => {
        });

        // 6. 检查控制台错误
        const originalError = console.error;
        console.error = function() {
            originalError.apply(console, arguments);
        };

        // 7. 尝试手动触发（需要访问Vue实例）
        // 注意：这需要Vue Devtools或特定的调试环境

        // 8. 检查是否有任何覆盖层
        const overlays = document.querySelectorAll('.ant-modal-mask, .ant-dropdown-hidden, .ant-select-dropdown-hidden');
        overlays.forEach((overlay, index) => {
        });


    }, 2000); // 等待2秒让页面完全加载

})();