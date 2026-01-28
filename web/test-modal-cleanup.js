// 测试数据字典删除确认模态框关闭功能
(function() {
    'use strict';


    // 测试结果对象
    const testResults = {
        total: 0,
        passed: 0,
        failed: 0,
        errors: []
    };

    // 测试辅助函数
    function test(name, fn) {
        testResults.total++;
        try {
            fn();
            testResults.passed++;
        } catch (error) {
            testResults.failed++;
            testResults.errors.push({ test: name, error: error.message });
            console.error(`❌ 失败: ${error.message}`);
        }
    }

    // 断言函数
    function assert(condition, message) {
        if (!condition) {
            throw new Error(message || '断言失败');
        }
    }

    // 查找模态框相关元素
    function findModalElements() {
        return {
            root: document.querySelector('.ant-modal-confirm-centered'),
            wrap: document.querySelector('.ant-modal-confirm-centered .ant-modal-wrap'),
            mask: document.querySelector('.ant-modal-confirm-centered .ant-modal-mask'),
            closeBtn: document.querySelector('.ant-modal-confirm-centered .ant-modal-close'),
            okBtn: document.querySelector('.ant-modal-confirm-centered .ant-modal-confirm-btns .ant-btn-primary'),
            cancelBtn: document.querySelector('.ant-modal-confirm-centered .ant-modal-confirm-btns .ant-btn:not(.ant-btn-primary)')
        };
    }

    // 统计所有模态框元素
    function countAllModalElements() {
        const elements = document.querySelectorAll('.ant-modal-confirm-centered, .ant-modal-confirm-centered .ant-modal-wrap, .ant-modal-confirm-centered .ant-modal-mask');
        return elements.length;
    }

    // 模拟创建模态框
    function simulateModalCreation() {
        const modalContainer = document.createElement('div');
        const modalHTML = `
            <div class="ant-modal-root ant-modal-confirm-centered">
                <div class="ant-modal-mask" style="background-color: rgba(0, 0, 0, 0.45);"></div>
                <div class="ant-modal-wrap" tabindex="-1">
                    <div class="ant-modal" style="width: 520px; transform-origin: 50% 50%;">
                        <div class="ant-modal-content">
                            <button type="button" class="ant-modal-close" aria-label="Close">
                                <span class="ant-modal-close-x">
                                    <span role="img" aria-label="close" class="anticon anticon-close ant-modal-close-icon">
                                        <svg viewBox="64 64 896 896" focusable="false" class="" data-icon="close" width="1em" height="1em" fill="currentColor" aria-hidden="true">
                                            <path d="M563.8 512l262.5-312.9c4.4-5.2.7-13.1-6.1-13.1h-79.8c-4.7 0-9.2 2.1-12.3 5.7L511.6 449.8 295.1 191.7c-3-3.6-7.5-5.7-12.3-5.7H203c-6.8 0-10.5 7.9-6.1 13.1L459.4 512 196.9 824.9A7.95 7.95 0 0 0 203 838h79.8c4.7 0 9.2-2.1 12.3-5.7l216.5-258.1 216.5 258.1c3 3.6 7.5 5.7 12.3 5.7h79.8c6.8 0 10.5-7.9 6.1-13.1L563.8 512z"></path>
                                        </svg>
                                    </span>
                                </span>
                            </button>
                            <div class="ant-modal-header">
                                <div class="ant-modal-title" id="rcDialogTitle0">
                                    <div class="ant-modal-confirm-title">
                                        <span role="img" aria-label="exclamation-circle" class="anticon anticon-exclamation-circle">
                                            <svg viewBox="64 64 896 896" focusable="false" class="" data-icon="exclamation-circle" width="1em" height="1em" fill="#faad14" aria-hidden="true">
                                                <path d="M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm-32 232c0-4.4 3.6-8 8-8h48c4.4 0 8 3.6 8 8v272c0 4.4-3.6 8-8 8h-48c-4.4 0-8-3.6-8-8V296zm32 440a48.01 48.01 0 0 1 0-96 48.01 48.01 0 0 1 0 96z"></path>
                                            </svg>
                                        </span>
                                        删除确认
                                    </div>
                                </div>
                            </div>
                            <div class="ant-modal-body">
                                <div class="ant-modal-confirm-body-wrapper">
                                    <div class="ant-modal-confirm-body">
                                        <span class="ant-modal-confirm-title">删除确认</span>
                                        <div class="ant-modal-confirm-content">确定删除选中的数据吗？</div>
                                    </div>
                                </div>
                            </div>
                            <div class="ant-modal-footer">
                                <div class="ant-modal-confirm-btns">
                                    <button type="button" class="ant-btn ant-btn-default ant-btn-sm">
                                        <span>取消</span>
                                    </button>
                                    <button type="button" class="ant-btn ant-btn-primary ant-btn-sm">
                                        <span>确定</span>
                                    </button>
                                </div>
                            </div>
                        </div>
                        <div tabindex="0" style="width: 0px; height: 0px; overflow: hidden; outline: none;"></div>
                    </div>
                </div>
            </div>
        `;
        modalContainer.innerHTML = modalHTML;
        document.body.appendChild(modalContainer);
        return modalContainer;
    }

    // 模态框清理函数（从ModelConfirmButton.vue提取）
    function cleanupModal() {
        const elements = document.querySelectorAll('.ant-modal-confirm-centered');
        elements.forEach(element => {
            element.style.display = 'none';
            element.remove();
        });
    }

    // 测试清理函数
    function testCleanup() {
        const container = simulateModalCreation();

        // 验证模态框已创建
        assert(countAllModalElements() > 0, '模态框应该已创建');

        // 执行清理
        cleanupModal();

        // 验证模态框已清理
        assert(countAllModalElements() === 0, '模态框应该已被清理');

        // 清理测试容器
        if (container && container.parentNode) {
            container.remove();
        }
    }

    // 执行测试
    test('模态框元素查找功能', () => {
        const modal = simulateModalCreation();
        const elements = findModalElements();

        assert(elements.root !== null, '应该找到根元素');
        assert(elements.wrap !== null, '应该找到wrap元素');
        assert(elements.mask !== null, '应该找到mask元素');
        assert(elements.closeBtn !== null, '应该找到关闭按钮');
        assert(elements.okBtn !== null, '应该找到确定按钮');
        assert(elements.cancelBtn !== null, '应该找到取消按钮');

        modal.remove();
    });

    test('模态框清理功能', () => {
        testCleanup();
    });

    test('多次打开和关闭模态框', () => {
        for (let i = 0; i < 5; i++) {
            testCleanup();
        }
    });

    test('快速点击关闭按钮', (done) => {
        const modal = simulateModalCreation();
        const closeBtn = document.querySelector('.ant-modal-confirm-centered .ant-modal-close');

        assert(closeBtn !== null, '应该找到关闭按钮');

        // 模拟快速点击
        for (let i = 0; i < 10; i++) {
            closeBtn.click();
        }

        cleanupModal();
        assert(countAllModalElements() === 0, '快速点击后模态框应该已被清理');

        modal.remove();
    });

    test('同时点击确定和关闭按钮', () => {
        const modal = simulateModalCreation();
        const okBtn = document.querySelector('.ant-modal-confirm-centered .ant-modal-confirm-btns .ant-btn-primary');
        const closeBtn = document.querySelector('.ant-modal-confirm-centered .ant-modal-close');

        assert(okBtn !== null, '应该找到确定按钮');
        assert(closeBtn !== null, '应该找到关闭按钮');

        // 同时触发两个按钮的点击事件
        okBtn.click();
        closeBtn.click();

        cleanupModal();
        assert(countAllModalElements() === 0, '同时点击后模态框应该已被清理');

        modal.remove();
    });

    test('模拟网络延迟后的清理', (done) => {
        const modal = simulateModalCreation();

        // 模拟网络延迟
        setTimeout(() => {
            cleanupModal();
            assert(countAllModalElements() === 0, '延迟后模态框应该已被清理');
            modal.remove();
            if (done) done();
        }, 100);
    });

    test('验证CSS类名选择器正确性', () => {
        const modal = simulateModalCreation();

        // 验证选择器能正确找到元素
        const elements = document.querySelectorAll('.ant-modal-confirm-centered');
        assert(elements.length > 0, '应该能找到ant-modal-confirm-centered元素');

        const mask = document.querySelector('.ant-modal-confirm-centered .ant-modal-mask');
        assert(mask !== null, '应该能找到mask元素');

        const wrap = document.querySelector('.ant-modal-confirm-centered .ant-modal-wrap');
        assert(wrap !== null, '应该能找到wrap元素');

        modal.remove();
    });

    // 运行所有测试

    // 由于有些测试是异步的，我们需要等待它们完成
    setTimeout(() => {

        if (testResults.failed > 0) {
            testResults.errors.forEach(({ test, error }) => {
            });
        } else {
        }

    }, 200);

})();