import { ref, reactive, computed, onMounted, onUnmounted } from 'vue';
import { useWebSocket } from '/@/hooks/web/useWebSocket';
import { notification } from 'ant-design-vue';

/** 计算进度数据结构 */
export interface CalcProgressData {
  batchId: string;
  taskType: 'CALC' | 'JUDGE' | 'MAGNETIC_JUDGE';
  total: number;
  completed: number;
  successCount: number;
  failedCount: number;
  status: 'PROCESSING' | 'COMPLETED' | 'FAILED';
  message: string;
  timestamp: string;
}

/** 计算通知数据结构 */
export interface CalcNotificationData {
  title: string;
  content: string;
  type: 'success' | 'error' | 'info';
  batchId: string;
  taskType: string;
  timestamp: string;
}

/**
 * 计算进度 composable。
 * 监听 WebSocket 的 calcProgress / calcNotification 消息，维护进度状态。
 *
 * @param options.onCompleted - 计算完成时的回调（可用于刷新表格）
 * @param options.showNotification - 是否显示全局通知（默认 true）
 */
export function useCalcProgress(options?: {
  onCompleted?: (progress: CalcProgressData) => void;
  onFailed?: (progress: CalcProgressData) => void;
  showNotification?: boolean;
}) {
  const { onWebSocket, offWebSocket } = useWebSocket();

  /** 当前活跃的计算进度（按 batchId 分组） */
  const progressMap = reactive<Map<string, CalcProgressData>>(new Map());

  /** 是否有正在计算中的任务 */
  const hasActiveCalc = computed(() => {
    for (const p of progressMap.values()) {
      if (p.status === 'PROCESSING') return true;
    }
    return false;
  });

  /** 所有活跃进度列表（用于 UI 展示） */
  const activeProgressList = computed(() => {
    return Array.from(progressMap.values()).filter((p) => p.status === 'PROCESSING');
  });

  /** 最近完成的进度列表 */
  const recentCompletedList = computed(() => {
    return Array.from(progressMap.values()).filter(
      (p) => p.status === 'COMPLETED' || p.status === 'FAILED',
    );
  });

  /** 正在计算中的数据总条数 */
  const totalCalculating = computed(() => {
    let total = 0;
    for (const p of progressMap.values()) {
      if (p.status === 'PROCESSING') {
        total += p.total - p.completed;
      }
    }
    return total;
  });

  /** WebSocket 消息处理 */
  function handleWsMessage(data: any) {
    if (!data || !data.method) return;

    if (data.method === 'calcProgress') {
      handleCalcProgress(data.data as CalcProgressData);
    } else if (data.method === 'calcNotification') {
      handleCalcNotification(data.data as CalcNotificationData);
    }
  }

  function handleCalcProgress(progress: CalcProgressData) {
    if (!progress?.batchId) return;

    // 更新进度
    progressMap.set(progress.batchId, { ...progress });

    if (progress.status === 'COMPLETED') {
      options?.onCompleted?.(progress);
      // 3 秒后移除已完成的进度
      setTimeout(() => {
        progressMap.delete(progress.batchId);
      }, 3000);
    } else if (progress.status === 'FAILED') {
      options?.onFailed?.(progress);
      // 5 秒后移除失败的进度
      setTimeout(() => {
        progressMap.delete(progress.batchId);
      }, 5000);
    }
  }

  function handleCalcNotification(data: CalcNotificationData) {
    if (options?.showNotification === false) return;
    if (!data) return;

    const notificationType = data.type === 'success' ? 'success' : data.type === 'error' ? 'error' : 'info';
    notification[notificationType]({
      message: data.title,
      description: data.content,
      duration: 5,
    });
  }

  /** 清除指定批次的进度 */
  function clearProgress(batchId: string) {
    progressMap.delete(batchId);
  }

  /** 清除所有进度 */
  function clearAllProgress() {
    progressMap.clear();
  }

  onMounted(() => {
    onWebSocket(handleWsMessage);
  });

  onUnmounted(() => {
    offWebSocket(handleWsMessage);
  });

  return {
    progressMap,
    hasActiveCalc,
    activeProgressList,
    recentCompletedList,
    totalCalculating,
    clearProgress,
    clearAllProgress,
  };
}
