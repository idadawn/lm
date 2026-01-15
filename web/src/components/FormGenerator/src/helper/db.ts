const DRAWING_ITEMS = 'drawingItems';

export function getDrawingList() {
  const str = localStorage.getItem(DRAWING_ITEMS);
  if (str) return JSON.parse(str);
  return null;
}

export function saveDrawingList(list) {
  localStorage.setItem(DRAWING_ITEMS, JSON.stringify(list));
}
