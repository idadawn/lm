// 直接测试模态框关闭按钮功能
(function() {
    'use strict';


    // 这是E:\project\2025\lm\web\src\views\systemData\dictionary\index.vue中handleDelete函数里的清理代码
    function cleanupComprehensive() {
        // 查找所有确认对话框相关的元素
        const confirmModals = document.querySelectorAll('.ant-modal-confirm');
        confirmModals.forEach(modal => {
            const wrap = modal.closest('.ant-modal-wrap.ant-modal-confirm-centered');
            if (wrap) {
                wrap.style.display = 'none';
                if (wrap.parentNode) {
                    wrap.parentNode.removeChild(wrap);
                }
            }
        });

        // 清理所有modal相关的遮罩层
        const allModalMasks = document.querySelectorAll('.ant-modal-mask');
        allModalMasks.forEach(mask => {
            if (mask.parentNode) {
                mask.parentNode.removeChild(mask);
            }
        });

        // 清理body上的样式
        document.body.style.overflow = '';
        document.body.style.paddingRight = '';

        // 移除所有ant-modal相关的类
        const allModals = document.querySelectorAll('[class*="ant-modal"]');
        allModals.forEach(el => {
            if (el.className.includes('ant-modal') && el.parentNode) {
                el.parentNode.removeChild(el);
            }
        });
    }

    // ModelConfirmButton.vue中的清理函数
    function cleanupModal() {
        const elements = document.querySelectorAll('.ant-modal-confirm-centered');
        elements.forEach(element => {
            element.style.display = 'none';
            element.remove();
        });
    }

    // 测试函数
    function testCleanupFunction(cleanupFn, name) {

        // 创建测试模态框
        const modal1 = document.createElement('div');
        modal1.className = 'ant-modal-confirm ant-modal-confirm-centered';
        modal1.innerHTML = `
            <div class="ant-modal-mask"></div>
            <div class="ant-modal-wrap ant-modal-confirm-centered">
                <button class="ant-modal-close">
                    <span class="ant-modal-close-x">X</span>
                </button>
                <div class="ant-modal-confirm-btns">
                    <button class="ant-btn-primary">确定</button>
                </div>
            </div>
        `;
        document.body.appendChild(modal1);

        // 验证模态框存在
        const beforeCount = document.querySelectorAll('.ant-modal-confirm-centered, .ant-modal-mask').length;

        if (beforeCount === 0) {
            console.error('❌ 测试失败: 没有创建模态框元素');
            return false;
        }

        // 执行清理
        cleanupFn();

        // 验证模态框已清理
        const afterCount = document.querySelectorAll('.ant-modal-confirm-centered, .ant-modal-mask').length;

        if (afterCount > 0) {
            console.error('❌ 测试失败: 还有残留的模态框元素');
            // 显示残留的元素
            const remaining = document.querySelectorAll('.ant-modal-confirm-centered, .ant-modal-mask');
            remaining.forEach((el, i) => {
                console.error(`  残留元素 ${i+1}:`, el.className);
            });
            return false;
        }

        return true;
    }

    // 测试不同场景
    function runTests() {
        let allPassed = true;

        // 测试ModelConfirmButton.vue的清理函数
        allPassed &= testCleanupFunction(cleanupModal, 'cleanupModal');

        // 测试综合清理函数
        allPassed &= testCleanupFunction(cleanupComprehensive, 'cleanupComprehensive');

        // 测试edge case: 多个模态框
        for (let i = 0; i < 3; i++) {
            const modal = document.createElement('div');
            modal.className = 'ant-modal-confirm ant-modal-confirm-centered';
            modal.innerHTML = `<div class="ant-modal-mask"></div>`;
            document.body.appendChild(modal);
        }
        cleanupComprehensive();
        const remaining = document.querySelectorAll('.ant-modal-confirm-centered, .ant-modal-mask').length;
        if (remaining === 0) {
        } else {
            console.error(`❌ 测试失败: 还有${remaining}个残留元素`);
            allPassed = false;
        }

        // 最终结果
        if (allPassed) {
        } else {
        }

    }

    // 启动测试
    if (typeof window !== 'undefined') {
        // 在浏览器中运行
        document.addEventListener('DOMContentLoaded', runTests);
    } else {
        // 在Node.js中提示需要浏览器环境
    }

})();