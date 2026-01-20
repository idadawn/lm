import React from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { FormulaToken } from '../types';
import { X, GripVertical } from 'lucide-react';

interface Props {
  token: FormulaToken;
  onDelete: (id: string) => void;
}

export const SortableToken: React.FC<Props> = ({ token, onDelete }) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: token.id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  const getColors = () => {
    switch (token.type) {
      case 'field':
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'operator':
        return 'bg-orange-100 text-orange-800 border-orange-200 font-bold';
      case 'function':
        return 'bg-purple-100 text-purple-800 border-purple-200 font-bold';
      case 'value':
        return 'bg-green-100 text-green-800 border-green-200 font-mono';
      case 'parenthesis':
      case 'separator':
        return 'bg-slate-100 text-slate-600 border-slate-200 font-bold';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={`
        relative group flex items-center h-10 px-2 rounded-md border shadow-sm select-none
        ${getColors()}
        ${isDragging ? 'opacity-30 z-50' : 'opacity-100'}
      `}
    >
      {/* Drag Handle */}
      <div {...attributes} {...listeners} className="cursor-grab active:cursor-grabbing p-1 -ml-1 hover:bg-black/5 rounded">
         <GripVertical className="w-3 h-3 opacity-50" />
      </div>

      {/* Content */}
      <span className="px-1 text-sm whitespace-nowrap">
        {token.label || token.value}
      </span>

      {/* Delete Button (visible on hover) */}
      <button
        onPointerDown={(e) => {
            // Prevent drag from starting when clicking delete
            e.stopPropagation(); 
        }}
        onClick={(e) => {
            e.stopPropagation();
            onDelete(token.id);
        }}
        className="ml-1 p-0.5 rounded-full hover:bg-black/10 text-current opacity-0 group-hover:opacity-100 transition-opacity"
      >
        <X className="w-3 h-3" />
      </button>
    </div>
  );
};
