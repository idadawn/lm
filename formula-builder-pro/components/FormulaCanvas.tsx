import React from 'react';
import { useDroppable } from '@dnd-kit/core';
import { SortableContext as SortableContextOriginal, horizontalListSortingStrategy } from '@dnd-kit/sortable';
import { SortableToken } from './SortableToken';
import { FormulaToken } from '../types';
import { Calculator } from 'lucide-react';

// Workaround for dnd-kit type incompatibility with React 18 types
const SortableContext = SortableContextOriginal as any;

interface Props {
  tokens: FormulaToken[];
  onDeleteToken: (id: string) => void;
  onClear: () => void;
}

export const FormulaCanvas: React.FC<Props> = ({ tokens, onDeleteToken, onClear }) => {
  const { setNodeRef, isOver } = useDroppable({
    id: 'formula-canvas',
  });

  return (
    <div className="flex flex-col h-full bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-slate-100 bg-slate-50">
        <div className="flex items-center gap-2 text-slate-700">
          <Calculator className="w-5 h-5" />
          <h2 className="font-semibold">公式编辑器</h2>
        </div>
        <button 
            onClick={onClear}
            className="text-xs text-red-600 hover:text-red-700 hover:underline px-2 py-1"
        >
            清空全部
        </button>
      </div>

      {/* Canvas Area */}
      <div
        ref={setNodeRef}
        className={`
          flex-1 p-6 overflow-y-auto flex flex-wrap content-start gap-2 transition-colors min-h-[200px]
          ${isOver ? 'bg-blue-50/50 ring-2 ring-inset ring-blue-300' : 'bg-white'}
        `}
      >
        <SortableContext items={tokens.map(t => t.id)} strategy={horizontalListSortingStrategy}>
          {tokens.map((token) => (
            <SortableToken
              key={token.id}
              token={token}
              onDelete={onDeleteToken}
            />
          ))}
        </SortableContext>
        
        {tokens.length === 0 && (
          <div className="w-full h-40 flex flex-col items-center justify-center text-slate-400 border-2 border-dashed border-slate-200 rounded-lg">
            <p className="text-sm">请将字段拖拽至此处，或点击右侧运算符构建公式</p>
          </div>
        )}
      </div>

      {/* Preview Footer */}
      <div className="px-4 py-4 bg-slate-50 border-t border-slate-200">
        <div className="text-xs font-semibold text-slate-500 uppercase mb-1">原始公式预览</div>
        <div className="font-mono text-sm text-slate-800 bg-white p-3 border rounded break-all">
          {tokens.length > 0 
            ? tokens.map(t => t.value).join(' ') 
            : <span className="text-slate-400 italic">= (等待输入...)</span>}
        </div>
      </div>
    </div>
  );
};