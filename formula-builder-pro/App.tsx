import React, { useState } from 'react';
import { FormulaList } from './components/FormulaList';
import { FormulaEditor } from './components/FormulaEditor';
import { FormulaRow } from './types';

const App: React.FC = () => {
  const [currentView, setCurrentView] = useState<'list' | 'editor'>('list');
  const [editingFormula, setEditingFormula] = useState<FormulaRow | null>(null);

  const handleEditFormula = (row: FormulaRow) => {
    setEditingFormula(row);
    setCurrentView('editor');
  };

  const handleBackToList = () => {
    setEditingFormula(null);
    setCurrentView('list');
  };

  return (
    <>
      {currentView === 'list' && (
        <FormulaList onEditFormula={handleEditFormula} />
      )}
      {currentView === 'editor' && (
        <FormulaEditor 
            editingRow={editingFormula} 
            onBack={handleBackToList} 
        />
      )}
    </>
  );
};

export default App;
