import type { ModalFunc, ModalFuncProps } from 'ant-design-vue/lib/modal/Modal';

import { Modal, message as Message, notification } from 'ant-design-vue';
import { InfoCircleFilled, CheckCircleFilled, CloseCircleFilled } from '@ant-design/icons-vue';

import { NotificationArgsProps, ConfigProps } from 'ant-design-vue/lib/notification';
import { useI18n } from './useI18n';
import { isString } from '/@/utils/is';

export interface NotifyApi {
  info(config: NotificationArgsProps): void;
  success(config: NotificationArgsProps): void;
  error(config: NotificationArgsProps): void;
  warn(config: NotificationArgsProps): void;
  warning(config: NotificationArgsProps): void;
  open(args: NotificationArgsProps): void;
  close(key: String): void;
  config(options: ConfigProps): void;
  destroy(): void;
}

export declare type NotificationPlacement = 'topLeft' | 'topRight' | 'bottomLeft' | 'bottomRight';
export declare type IconType = 'success' | 'info' | 'error' | 'warning';
export interface ModalOptionsEx extends Omit<ModalFuncProps, 'iconType'> {
  iconType: 'warning' | 'success' | 'error' | 'info';
}
export type ModalOptionsPartial = Partial<ModalOptionsEx> & Pick<ModalOptionsEx, 'content'>;

interface ConfirmOptions {
  info: ModalFunc;
  success: ModalFunc;
  error: ModalFunc;
  warn: ModalFunc;
  warning: ModalFunc;
}

function getIcon(iconType: string) {
  if (iconType === 'warning') {
    return <InfoCircleFilled class="modal-icon-warning" />;
  } else if (iconType === 'success') {
    return <CheckCircleFilled class="modal-icon-success" />;
  } else if (iconType === 'info') {
    return <InfoCircleFilled class="modal-icon-info" />;
  } else {
    return <CloseCircleFilled class="modal-icon-error" />;
  }
}

function renderContent({ content }: Pick<ModalOptionsEx, 'content'>) {
  if (isString(content)) {
    return <div innerHTML={`<div>${content as string}</div>`}></div>;
  } else {
    return content;
  }
}

/**
 * @description: Create confirmation box
 */
type ConfirmInstance = {
  destroy: () => void;
  update: (config: ModalFuncProps) => void;
};

/**
 * 强制清理所有确认弹窗的 DOM 元素
 * 解决 Ant Design Vue Modal.confirm 在某些情况下不能正确关闭的问题
 */
function forceCleanupConfirmModals() {
  setTimeout(() => {
    // 清理确认弹窗
    const confirmModals = document.querySelectorAll('.ant-modal-confirm');
    confirmModals.forEach((modal) => {
      const wrap = modal.closest('.ant-modal-wrap');
      if (wrap && wrap.parentNode) {
        wrap.parentNode.removeChild(wrap);
      }
    });

    // 清理遮罩层
    const masks = document.querySelectorAll('.ant-modal-mask');
    masks.forEach((mask) => {
      if (mask.parentNode) {
        mask.parentNode.removeChild(mask);
      }
    });

    // 恢复 body 样式
    document.body.style.overflow = '';
    document.body.style.paddingRight = '';
    document.body.classList.remove('ant-scrolling-effect');
  }, 100);
}

function createConfirm(options: ModalOptionsEx): ConfirmInstance {
  const iconType = options.iconType || 'warning';
  Reflect.deleteProperty(options, 'iconType');

  // 取出用户回调，避免后续被覆盖/重复调用
  const { onOk: userOnOk, onCancel: userOnCancel, ...rest } = options as ModalFuncProps;

  let instance: ConfirmInstance | null = null;
  let isDestroyed = false;

  // 安全销毁函数，确保只执行一次
  const safeDestroy = () => {
    if (isDestroyed) return;
    isDestroyed = true;

    try {
      instance?.destroy();
    } catch (e) {
      console.warn('Modal destroy failed:', e);
    }

    // 强制清理 DOM，确保弹窗关闭
    forceCleanupConfirmModals();
  };

  const opt: ModalFuncProps = {
    centered: true,
    icon: getIcon(iconType),
    ...rest,
    content: renderContent(options),
    // 关键：取消必须主动销毁，否则会残留遮罩/弹窗
    onCancel: async () => {
      try {
        await (userOnCancel as any)?.();
      } finally {
        safeDestroy();
      }
    },
    // 确认：成功后再关闭；若用户 onOk 抛错/拒绝，则不关闭，方便展示错误并重试
    onOk: async () => {
      if (userOnOk) {
        await (userOnOk as any)();
      }
      safeDestroy();
    },
  };

  instance = Modal.confirm(opt) as unknown as ConfirmInstance;
  return instance;
}

const getBaseOptions = () => {
  const { t } = useI18n();
  return {
    okText: t('common.okText'),
    centered: true,
  };
};

function createModalOptions(options: ModalOptionsPartial, icon: string): ModalOptionsPartial {
  return {
    ...getBaseOptions(),
    ...options,
    content: renderContent(options),
    icon: getIcon(icon),
  };
}

function createSuccessModal(options: ModalOptionsPartial) {
  return Modal.success(createModalOptions(options, 'success'));
}

function createErrorModal(options: ModalOptionsPartial) {
  return Modal.error(createModalOptions(options, 'error'));
}

function createInfoModal(options: ModalOptionsPartial) {
  return Modal.info(createModalOptions(options, 'info'));
}

function createWarningModal(options: ModalOptionsPartial) {
  return Modal.warning(createModalOptions(options, 'warning'));
}

Message.config({
  duration: 1.5,
  maxCount: 3,
});

notification.config({
  placement: 'topRight',
  duration: 3,
});

/**
 * @description: message
 */
export function useMessage() {
  return {
    createMessage: Message,
    notification: notification as NotifyApi,
    createConfirm: createConfirm,
    createSuccessModal,
    createErrorModal,
    createInfoModal,
    createWarningModal,
  };
}
