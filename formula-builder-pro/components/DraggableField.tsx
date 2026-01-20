import React from 'react';
import { useDraggable } from '@dnd-kit/core';
import { FieldDefinition } from '../types';
import { GripVertical, Database } from 'lucide-react';

interface Props {
  field: FieldDefinition;
}

export const DraggableField: React.FC<Props> = ({ field }) => {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: `field-source-${field.id}`,
    data: {
      type: 'field-source',
      fieldData: field,
    },
  });

  const style = transform
    ? {
        transform: `translate3d(${transform.x}px, ${transform.y}px, 0)`,
      }
    : undefined;

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...listeners}
      {...attributes}
      className={`
        flex items-center gap-2 p-3 mb-2 bg-white border rounded-lg shadow-sm cursor-grab active:cursor-grabbing hover:border-blue-400 transition-colors
        ${isDragging ? 'opacity-50 ring-2 ring-blue-500' : 'border-slate-200'}
      `}
    >
      <GripVertical className="w-4 h-4 text-slate-400" />
      <div className="bg-blue-100 text-blue-700 p-1.5 rounded-md">
        <Database className="w-4 h-4" />
      </div>
      <div className="flex-1">
        <div className="text-sm font-medium text-slate-700">{field.name}</div>
        <div className="text-xs text-slate-400 font-mono">{field.code}</div>
      </div>
    </div>
  );
};
