import React, { useState } from 'react';
import { nanoid } from 'nanoid';
import { Trash2, Plus, GitMerge, ArrowRight, CheckCircle2 } from 'lucide-react';
import { JudgmentRule, JudgmentCondition, ConditionGroup, ComparisonOperator } from '../types';
import { MOCK_FIELDS, RULE_OPERATORS } from '../constants';

export const JudgmentBuilder: React.FC = () => {
  // Mock initialization based on Excel formula
  const [rules, setRules] = useState<JudgmentRule[]>([
    {
      id: 'rule-1',
      resultValue: 'A',
      rootGroup: {
        id: 'g-1',
        logic: 'AND',
        conditions: [
          { id: 'c-1', fieldId: 'f_F3', operator: '>', value: '0' },
          { id: 'c-2', fieldId: 'f_F3', operator: '<=', value: '0.14' },
          { id: 'c-3', fieldId: 'f_I3', operator: 'IS_NULL', value: '' },
        ],
      },
    },
    {
      id: 'rule-2',
      resultValue: '性能不合',
      rootGroup: {
        id: 'g-2',
        logic: 'AND',
        conditions: [
          { id: 'c-21', fieldId: 'f_F3', operator: '>', value: '0.15' },
          { id: 'c-22', fieldId: 'f_AI3', operator: '<>', value: '严重' },
        ],
      },
    },
  ]);

  const [defaultValue, setDefaultValue] = useState<string>('B');

  // --- Actions ---
  const addRule = () => {
    setRules((prev) => [
      ...prev,
      {
        id: nanoid(),
        resultValue: '',
        rootGroup: {
          id: nanoid(),
          logic: 'AND',
          conditions: [],
        },
      },
    ]);
  };

  const removeRule = (index: number) => {
    setRules((prev) => prev.filter((_, i) => i !== index));
  };

  const updateRule = (index: number, updates: Partial<JudgmentRule>) => {
    setRules((prev) => prev.map((r, i) => (i === index ? { ...r, ...updates } : r)));
  };

  const updateGroup = (ruleIndex: number, groupUpdates: Partial<ConditionGroup>) => {
     setRules(prev => prev.map((r, i) => {
         if (i === ruleIndex) {
             return { ...r, rootGroup: { ...r.rootGroup, ...groupUpdates } };
         }
         return r;
     }));
  }

  const addCondition = (ruleIndex: number) => {
    setRules((prev) =>
      prev.map((r, i) => {
        if (i === ruleIndex) {
          return {
            ...r,
            rootGroup: {
              ...r.rootGroup,
              conditions: [
                ...r.rootGroup.conditions,
                {
                  id: nanoid(),
                  fieldId: MOCK_FIELDS[0].id,
                  operator: '=',
                  value: '',
                },
              ],
            },
          };
        }
        return r;
      })
    );
  };

  const removeCondition = (ruleIndex: number, conditionIndex: number) => {
    setRules((prev) =>
      prev.map((r, i) => {
        if (i === ruleIndex) {
          return {
            ...r,
            rootGroup: {
              ...r.rootGroup,
              conditions: r.rootGroup.conditions.filter((_, ci) => ci !== conditionIndex),
            },
          };
        }
        return r;
      })
    );
  };

  const updateCondition = (ruleIndex: number, conditionIndex: number, field: keyof JudgmentCondition, value: any) => {
      setRules(prev => prev.map((r, i) => {
          if (i === ruleIndex) {
              const newConditions = [...r.rootGroup.conditions];
              newConditions[conditionIndex] = { ...newConditions[conditionIndex], [field]: value };
              return { ...r, rootGroup: { ...r.rootGroup, conditions: newConditions }};
          }
          return r;
      }));
  }


  // --- Render Helpers ---

  const renderConditionRow = (
    condition: JudgmentCondition,
    ruleIndex: number,
    conditionIndex: number
  ) => (
    <div
      key={condition.id}
      className="flex items-center gap-2 mb-2 bg-white p-2 rounded border border-slate-200 shadow-sm hover:border-blue-300 transition-colors"
    >
      {/* Field Select */}
      <div className="w-1/3 min-w-[120px]">
        <select
          value={condition.fieldId}
          onChange={(e) => updateCondition(ruleIndex, conditionIndex, 'fieldId', e.target.value)}
          className="w-full text-sm border-slate-300 rounded-md shadow-sm focus:border-blue-500 focus:ring-blue-500 p-1 border"
        >
          {MOCK_FIELDS.map((f) => (
            <option key={f.id} value={f.id}>
              {f.name} ({f.code})
            </option>
          ))}
        </select>
      </div>

      {/* Operator Select */}
      <div className="w-[100px] shrink-0">
        <select
          value={condition.operator}
          onChange={(e) => updateCondition(ruleIndex, conditionIndex, 'operator', e.target.value)}
          className="w-full text-sm border-slate-300 rounded-md shadow-sm focus:border-blue-500 focus:ring-blue-500 p-1 border"
        >
          {RULE_OPERATORS.map((op) => (
            <option key={op.value} value={op.value}>
              {op.label}
            </option>
          ))}
        </select>
      </div>

      {/* Value Input */}
      <div className="flex-1">
        {!['IS_NULL', 'NOT_NULL'].includes(condition.operator) ? (
          <input
            type="text"
            value={condition.value}
            onChange={(e) => updateCondition(ruleIndex, conditionIndex, 'value', e.target.value)}
            className="w-full text-sm border-slate-300 rounded-md shadow-sm focus:border-blue-500 focus:ring-blue-500 p-1 border"
            placeholder="比较值"
          />
        ) : (
          <span className="text-xs text-slate-400 italic bg-slate-50 px-2 py-1 rounded block text-center">
            无需输入值
          </span>
        )}
      </div>

      <button
        onClick={() => removeCondition(ruleIndex, conditionIndex)}
        className="p-1.5 text-red-500 hover:bg-red-50 rounded transition-colors"
      >
        <Trash2 className="w-3.5 h-3.5" />
      </button>
    </div>
  );

  return (
    <div className="flex flex-col gap-6 p-2 max-w-4xl mx-auto h-full overflow-y-auto pb-20">
      {/* Intro */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-2 flex gap-3">
        <div className="bg-blue-100 p-2 rounded-full h-fit text-blue-600">
          <GitMerge className="w-5 h-5" />
        </div>
        <div>
          <h3 className="font-bold text-blue-900">判定逻辑配置</h3>
          <p className="text-sm text-blue-700 mt-1">
            系统将按照<span className="font-bold">从上到下</span>
            的顺序执行规则。一旦满足某条规则的条件，即返回对应的结果，并停止后续判断。
          </p>
        </div>
      </div>

      {/* Rule List */}
      {rules.map((rule, ruleIdx) => (
        <div key={rule.id} className="relative pl-8">
          {/* Visual Flow Line */}
          <div className="absolute left-3 top-0 bottom-0 w-0.5 bg-slate-200"></div>
          <div className="absolute left-0 top-6 w-6 h-6 bg-slate-100 border-2 border-slate-300 text-slate-500 rounded-full flex items-center justify-center text-xs font-bold z-10">
            {ruleIdx + 1}
          </div>

          <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
            {/* Rule Header / Result */}
            <div className="bg-slate-50 px-4 py-3 border-b border-slate-200 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <span className="text-sm font-medium text-slate-500">如果满足以下条件，则返回:</span>
                <div className="flex items-center gap-2">
                  <ArrowRight className="w-4 h-4 text-slate-400" />
                  <input
                    value={rule.resultValue}
                    onChange={(e) => updateRule(ruleIdx, { resultValue: e.target.value })}
                    className="w-40 font-bold text-blue-700 border-b border-blue-300 bg-transparent focus:outline-none focus:border-blue-600 px-1"
                    placeholder="例如: A"
                  />
                </div>
              </div>
              <button 
                onClick={() => removeRule(ruleIdx)}
                className="text-xs text-red-500 hover:text-red-700 hover:underline"
              >
                  删除规则
              </button>
            </div>

            {/* Conditions Area */}
            <div className="p-4 bg-slate-50/50">
              <div className="bg-white border border-slate-200 rounded-lg p-3">
                {/* Logic Toggle */}
                <div className="flex items-center gap-2 mb-3">
                  <span className="text-xs font-bold text-slate-500 uppercase">当</span>
                  <div className="inline-flex bg-slate-100 p-0.5 rounded-md border border-slate-200">
                    <button
                      onClick={() => updateGroup(ruleIdx, { logic: 'AND' })}
                      className={`px-3 py-0.5 text-xs rounded font-medium transition-all ${
                        rule.rootGroup.logic === 'AND'
                          ? 'bg-white shadow-sm text-blue-600'
                          : 'text-slate-500 hover:text-slate-700'
                      }`}
                    >
                      满足所有 (AND)
                    </button>
                    <button
                      onClick={() => updateGroup(ruleIdx, { logic: 'OR' })}
                      className={`px-3 py-0.5 text-xs rounded font-medium transition-all ${
                        rule.rootGroup.logic === 'OR'
                          ? 'bg-white shadow-sm text-orange-600'
                          : 'text-slate-500 hover:text-slate-700'
                      }`}
                    >
                      满足任一 (OR)
                    </button>
                  </div>
                  <span className="text-xs font-bold text-slate-500 uppercase">条件时:</span>
                </div>

                {/* Conditions List */}
                <div className="space-y-2 relative">
                  {rule.rootGroup.conditions.length > 0 && (
                    <div
                      className={`absolute top-2 bottom-2 left-[-10px] w-1.5 rounded-l ${
                        rule.rootGroup.logic === 'AND' ? 'bg-blue-200' : 'bg-orange-200'
                      }`}
                    ></div>
                  )}

                  {rule.rootGroup.conditions.map((cond, cIdx) =>
                    renderConditionRow(cond, ruleIdx, cIdx)
                  )}

                  {rule.rootGroup.conditions.length === 0 && (
                    <div className="text-center py-4 text-slate-400 text-sm border border-dashed rounded bg-slate-50">
                      暂无条件，请添加
                    </div>
                  )}
                </div>

                <div className="mt-3">
                  <button
                    onClick={() => addCondition(ruleIdx)}
                    className="w-full py-1 text-xs border border-dashed border-slate-300 text-slate-500 rounded hover:bg-slate-50 hover:border-blue-300 hover:text-blue-500 flex items-center justify-center gap-1 transition-colors"
                  >
                    <Plus className="w-3 h-3" /> 添加条件
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      ))}

      {/* Add Rule Button */}
      <div className="pl-8 relative">
        <div className="absolute left-3 top-0 bottom-0 w-0.5 bg-slate-200"></div>
        <button
          onClick={addRule}
          className="w-full h-12 border-2 border-dashed border-slate-300 text-slate-500 rounded-lg hover:border-blue-400 hover:text-blue-600 hover:bg-blue-50 transition-all flex items-center justify-center font-medium"
        >
          <Plus className="w-4 h-4 mr-2" /> 添加优先级规则
        </button>
      </div>

      {/* Default Fallback */}
      <div className="pl-8 relative mt-4">
        <div className="absolute left-3 top-0 h-8 w-0.5 bg-slate-200"></div>
        <div className="absolute left-0 top-2 w-6 h-6 bg-slate-800 text-white rounded-full flex items-center justify-center text-xs font-bold z-10">
          终
        </div>

        <div className="bg-slate-800 text-white rounded-xl shadow-lg p-5 flex items-center justify-between">
          <div>
            <h4 className="font-bold flex items-center gap-2">
              <CheckCircle2 className="w-5 h-5 text-green-400" />
              默认结果 (Else)
            </h4>
            <p className="text-slate-300 text-xs mt-1">
              如果以上所有规则都不满足，则返回此值。
            </p>
          </div>
          <div className="flex items-center gap-3">
            <span className="text-sm font-medium text-slate-300">返回:</span>
            <input
              value={defaultValue}
              onChange={(e) => setDefaultValue(e.target.value)}
              className="w-32 bg-slate-700 border border-slate-600 text-white font-bold px-2 py-1 rounded focus:outline-none focus:border-slate-500"
            />
          </div>
        </div>
      </div>
    </div>
  );
};