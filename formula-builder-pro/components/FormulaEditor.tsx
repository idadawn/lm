import React, { useState } from 'react';
import { nanoid } from 'nanoid';
import { Play, Info, ArrowLeft, Plus, Calculator, GitMerge, Save } from 'lucide-react';
import { DndContext, DragEndEvent, useSensor, useSensors, PointerSensor } from '@dnd-kit/core';
import { arrayMove } from '@dnd-kit/sortable';

import { MOCK_FIELDS, OPERATORS, FUNCTIONS, SNIPPETS } from '../constants';
import { FormulaToken, FormulaSnippet, FormulaRow, TokenType } from '../types';
import { DraggableField } from './DraggableField';
import { FormulaCanvas } from './FormulaCanvas';
import { JudgmentBuilder } from './JudgmentBuilder';

interface Props {
  editingRow: FormulaRow | null;
  onBack: () => void;
}

export const FormulaEditor: React.FC<Props> = ({ editingRow, onBack }) => {
  const [tokens, setTokens] = useState<FormulaToken[]>([]);
  const [manualInputValue, setManualInputValue] = useState<string>('');
  const isJudgmentMode = editingRow?.type === '判定';

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    })
  );

  // --- Actions ---
  const addToken = (type: TokenType, value: string, label?: string) => {
    const newToken: FormulaToken = {
      id: nanoid(),
      type,
      value,
      label,
    };
    setTokens((prev) => [...prev, newToken]);
  };

  const addSnippet = (snippet: FormulaSnippet) => {
    const newTokens = snippet.tokens.map((t) => ({ ...t, id: nanoid() }));
    setTokens((prev) => [...prev, ...newTokens]);
  };

  const handleDeleteToken = (id: string) => {
    setTokens((prev) => prev.filter((t) => t.id !== id));
  };

  const handleClear = () => {
    if (window.confirm('确定要清空公式吗？')) {
      setTokens([]);
    }
  };

  // --- Drag & Drop Handlers ---
  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (!over) return;

    // Handle dropping a Field from sidebar to canvas
    if (active.data.current?.type === 'field-source' && over.id === 'formula-canvas') {
      const field = active.data.current.fieldData;
      addToken('field', field.code, field.name);
      return;
    }

    // Handle reordering tokens within canvas
    if (active.id !== over.id) {
        setTokens((items) => {
            const oldIndex = items.findIndex((item) => item.id === active.id);
            const newIndex = items.findIndex((item) => item.id === over.id);
            
            if (oldIndex !== -1 && newIndex !== -1) {
                return arrayMove(items, oldIndex, newIndex);
            }
            return items;
        });
    }
  };

  // --- Render Logic ---

  // Calculation View
  const renderCalculationEditor = () => (
    <DndContext sensors={sensors} onDragEnd={handleDragEnd}>
      <div className="h-full grid grid-cols-12 gap-6">
        {/* Left Sidebar: Fields */}
        <aside className="col-span-3 flex flex-col gap-6">
          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-4 h-full flex flex-col">
            <h3 className="font-semibold text-slate-700 mb-4 flex items-center gap-2">
              <span className="bg-blue-100 text-blue-700 w-6 h-6 flex items-center justify-center rounded-full text-xs">
                1
              </span>
              可用字段
            </h3>
            <div className="space-y-1 overflow-y-auto flex-1 pr-2 min-h-0">
              {MOCK_FIELDS.map((field) => (
                <DraggableField key={field.id} field={field} />
              ))}
            </div>

            <div className="mt-6 p-4 bg-slate-50 rounded-lg border border-slate-200">
              <h4 className="text-xs font-bold text-slate-500 uppercase mb-2">手动输入数值</h4>
              <div className="flex gap-2">
                <input
                  type="number"
                  placeholder="100"
                  className="w-full px-2 py-1 text-sm border rounded focus:ring-2 focus:ring-blue-500 outline-none"
                  value={manualInputValue}
                  onChange={(e) => setManualInputValue(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && manualInputValue) {
                      addToken('value', manualInputValue);
                      setManualInputValue('');
                    }
                  }}
                />
                <button
                  onClick={() => {
                    if (manualInputValue) {
                      addToken('value', manualInputValue);
                      setManualInputValue('');
                    }
                  }}
                  className="bg-slate-200 hover:bg-slate-300 px-3 rounded text-slate-600"
                >
                  <Plus className="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>
        </aside>

        {/* Center: Editor Canvas */}
        <section className="col-span-6 flex flex-col gap-4">
          <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 flex gap-3 text-sm text-amber-800">
            <Info className="w-5 h-5 shrink-0 mt-0.5" />
            <div>
              <span className="font-bold">除法提示:</span> 您可以使用标准的
              <code className="mx-1 bg-amber-100 px-1 rounded">IF(分母 &lt;&gt; 0, ...)</code> 模式。
            </div>
          </div>

          <div className="flex-1 min-h-[500px]">
            <FormulaCanvas
              tokens={tokens}
              onDeleteToken={handleDeleteToken}
              onClear={handleClear}
            />
          </div>
        </section>

        {/* Right Sidebar: Operators */}
        <aside className="col-span-3 flex flex-col gap-6">
          <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-4">
            <h3 className="font-semibold text-slate-700 mb-4 flex items-center gap-2">
              <span className="bg-orange-100 text-orange-700 w-6 h-6 flex items-center justify-center rounded-full text-xs">
                2
              </span>
              运算符
            </h3>

            <div className="grid grid-cols-4 gap-2 mb-6">
              {OPERATORS.map((op) => (
                <button
                  key={op.label}
                  onClick={() => addToken(op.type as TokenType, op.value, op.label)}
                  className="p-2 text-sm font-bold bg-slate-50 hover:bg-orange-50 hover:text-orange-600 border border-slate-200 hover:border-orange-200 rounded transition-colors"
                >
                  {op.label}
                </button>
              ))}
            </div>

            <h3 className="font-semibold text-slate-700 mb-4 flex items-center gap-2">
              <span className="bg-purple-100 text-purple-700 w-6 h-6 flex items-center justify-center rounded-full text-xs">
                3
              </span>
              函数
            </h3>
            <div className="flex flex-wrap gap-2 mb-6">
              {FUNCTIONS.map((fn) => (
                <button
                  key={fn.value}
                  onClick={() => {
                    addToken('function', fn.value);
                    addToken('parenthesis', '(');
                  }}
                  className="px-3 py-1.5 text-xs font-semibold bg-purple-50 text-purple-700 border border-purple-200 rounded-full hover:bg-purple-100 transition-colors"
                >
                  {fn.label}
                </button>
              ))}
            </div>

            <div className="border-t border-slate-100 pt-4">
              <h3 className="font-semibold text-slate-700 mb-3 text-sm">常用逻辑模板</h3>
              <div className="space-y-2">
                {SNIPPETS.map((snippet, idx) => (
                  <button
                    key={idx}
                    onClick={() => addSnippet(snippet)}
                    className="w-full text-left p-3 rounded-lg border border-slate-200 hover:border-indigo-300 hover:bg-indigo-50 transition group"
                  >
                    <div className="font-medium text-sm text-slate-700 group-hover:text-indigo-700">
                      {snippet.name}
                    </div>
                    <div className="text-xs text-slate-400 mt-1">{snippet.description}</div>
                  </button>
                ))}
              </div>
            </div>
          </div>
        </aside>
      </div>
    </DndContext>
  );

  return (
    <div className="min-h-screen flex flex-col bg-slate-50">
      {/* Navigation Bar */}
      <header className="bg-white border-b border-slate-200 px-6 py-4 flex items-center justify-between sticky top-0 z-10">
        <div className="flex items-center gap-4">
          <button
            onClick={() => onBack()}
            className="p-2 hover:bg-slate-100 rounded-full text-slate-500 transition-colors"
            title="返回列表"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div className="h-8 w-px bg-slate-200 mx-1"></div>
          <div
            className={`p-2 rounded-lg text-white ${
              isJudgmentMode ? 'bg-orange-500' : 'bg-indigo-600'
            }`}
          >
            {isJudgmentMode ? <GitMerge className="w-5 h-5" /> : <Calculator className="w-5 h-5" />}
          </div>
          <div>
            <h1 className="text-xl font-bold text-slate-800">
              {editingRow ? `编辑公式: ${editingRow.name}` : '公式构建器'}
            </h1>
            <p className="text-xs text-slate-500">
              {editingRow ? `目标字段: ${editingRow.column}` : '可视化设计复杂计算逻辑'}
            </p>
          </div>
        </div>
        <div className="flex gap-3">
          <button className="px-4 py-2 bg-white border border-slate-300 text-slate-700 rounded hover:bg-slate-50 flex items-center gap-2">
             <Save className="w-4 h-4" /> 保存草稿
          </button>
          <button className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 flex items-center gap-2">
             <Play className="w-4 h-4" /> 校验并运行
          </button>
        </div>
      </header>

      <main className="flex-1 max-w-[1600px] w-full mx-auto p-6 h-[calc(100vh-80px)] overflow-hidden">
        {isJudgmentMode ? (
          // Judgment Rule Editor
          <div className="h-full bg-slate-100 rounded-xl overflow-hidden">
            <JudgmentBuilder />
          </div>
        ) : (
          // Calculation Formula Editor
          renderCalculationEditor()
        )}
      </main>
    </div>
  );
};