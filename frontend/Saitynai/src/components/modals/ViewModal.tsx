import './style/ViewModal.css';
import { API_ORIGIN } from '../../services/api';

interface ColumnDef<T> {
  key: keyof T | string;
  label: string;
}

interface ViewModalProps<T> {
  item: T | null;
  onClose: () => void;
  columns?: ColumnDef<T>[];
}

export function ViewModal<T>({ item, onClose, columns }: ViewModalProps<T>) {
  if (!item) return null;

  const labelForKey = (key: string) => {
    if (!columns || columns.length === 0) return key;
    const col = columns.find(c => String(c.key) === key);
    return col?.label ?? key;
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>Item Details</h3>
          <button className="modal-close" onClick={onClose}>
            ×
          </button>
        </div>
        <div className="modal-body">
          {Object.entries(item as object).map(([key, value]) => {
            const isImagePath = typeof value === 'string' && /\/uploads\//.test(value) && key.toLowerCase().endsWith('path');
            const src = isImagePath
              ? (String(value).startsWith('http') ? String(value) : `${API_ORIGIN}${String(value)}`)
              : '';
            return (
              <div key={key} className="detail-row">
                <span className="detail-label">{labelForKey(key)}:</span>
                <span className="detail-value">
                  {isImagePath ? (
                    <img
                      src={src}
                      alt={key}
                      style={{ maxWidth: '100%', maxHeight: '300px', borderRadius: '4px' }}
                    />
                  ) : (
                    value === null || value === undefined
                      ? '-'
                      : typeof value === 'boolean'
                      ? value ? 'Yes' : 'No'
                      : String(value)
                  )}
                </span>
              </div>
            );
          })}
        </div>
        <div className="modal-footer">
          <button className="modal-button" onClick={onClose}>
            Close
          </button>
        </div>
      </div>
    </div>
  );
}
