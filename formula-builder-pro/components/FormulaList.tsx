import React from 'react';
import { FileText } from 'lucide-react';
import { MOCK_FORMULA_ROWS } from '../constants';
import { FormulaRow } from '../types';

interface Props {
  onEditFormula: (row: FormulaRow) => void;
}

export const FormulaList: React.FC<Props> = ({ onEditFormula }) => {
  return (
    <div className="min-h-screen bg-slate-50 flex flex-col">
      {/* Top Header */}
      <div className="bg-white px-8 py-5 border-b border-slate-200">
        <div className="flex items-center gap-2 mb-2">
            <div className="bg-blue-600 text-white p-1 rounded">
                <FileText className="w-5 h-5" />
            </div>
            <h1 className="text-xl font-bold text-slate-800">公式维护</h1>
        </div>
        <p className="text-sm text-slate-500">维护中间数据表中的计算公式，支持从多个数据源引用变量。</p>
        
        <div className="absolute top-6 right-8">
             <button className="bg-blue-500 hover:bg-blue-600 text-white text-sm px-4 py-2 rounded shadow-sm transition-colors">
                 初始化公式
             </button>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 p-8 max-w-[1600px] w-full mx-auto">
        <div className="bg-white rounded-lg shadow-sm border border-slate-200 overflow-hidden">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-slate-50 border-b border-slate-200 text-sm font-medium text-slate-700">
                <th className="px-6 py-4 w-20">排序</th>
                <th className="px-6 py-4">公式名称</th>
                <th className="px-6 py-4">列名</th>
                <th className="px-6 py-4 w-32">类型</th>
                <th className="px-6 py-4 text-slate-400 font-normal italic">公式</th>
                <th className="px-6 py-4 w-24">单位</th>
                <th className="px-6 py-4 w-24">精度</th>
                <th className="px-6 py-4 w-24">状态</th>
                <th className="px-6 py-4 w-48">操作</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {MOCK_FORMULA_ROWS.map((row) => (
                <tr key={row.id} className="hover:bg-slate-50 transition-colors">
                  <td className="px-6 py-4 text-slate-600">{row.sort}</td>
                  <td className="px-6 py-4 font-medium text-slate-800">{row.name}</td>
                  <td className="px-6 py-4 text-slate-600 font-mono text-sm">{row.column}</td>
                  <td className="px-6 py-4">
                    <span 
                        className={`inline-block px-2 py-0.5 text-xs border rounded ${
                            row.type === '判定' 
                            ? 'bg-orange-50 text-orange-600 border-orange-200' 
                            : 'bg-blue-50 text-blue-600 border-blue-200'
                        }`}
                    >
                        {row.type}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-slate-400 italic text-sm">{row.formulaDisplay}</td>
                  <td className="px-6 py-4 text-slate-600">{row.unit || '-'}</td>
                  <td className="px-6 py-4 text-slate-600">{row.precision !== undefined ? row.precision : '-'}</td>
                  <td className="px-6 py-4">
                     <span className="inline-block px-2 py-0.5 text-xs text-green-600 border border-green-200 bg-green-50 rounded">
                         {row.status}
                     </span>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex gap-4 text-sm">
                        <button 
                            className="text-blue-500 hover:text-blue-700"
                            onClick={() => onEditFormula(row)}
                        >
                            编辑
                        </button>
                        <button 
                            onClick={() => onEditFormula(row)}
                            className="font-medium text-blue-500 hover:text-blue-700 hover:underline"
                        >
                            公式
                        </button>
                        <button className="text-red-400 hover:text-red-600">删除</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};