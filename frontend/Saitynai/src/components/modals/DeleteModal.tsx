import { useState } from 'react';
import './style/DeleteModal.css';

interface DeleteModalProps<T> {
  item: T | null;
  onConfirm: () => Promise<void> | void;
  onClose: () => void;
  itemName?: string;
  getDisplayText?: (item: T) => string;
}

export function DeleteModal<T>({ 
  item, 
  onConfirm, 
  onClose,
  itemName = 'item',
  getDisplayText
}: DeleteModalProps<T>) {
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  if (!item) return null;

  const displayText = getDisplayText 
    ? getDisplayText(item) 
    : `this ${itemName}`;

  const handleConfirm = async () => {
    setError(null);
    setIsLoading(true);
    try {
      await onConfirm();
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.response?.data || err.message || 'Failed to delete item';
      setError(errorMessage);
      setIsLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content delete-modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>Confirm Deletion</h3>
          <button className="modal-close" onClick={onClose}>
            ×
          </button>
        </div>
        <div className="modal-body">
          {error && (
            <div className="error-message">
              <span className="error-icon">⚠️</span>
              <span>{error}</span>
            </div>
          )}
          <div className="warning-icon">⚠️</div>
          <p className="warning-text">
            Are you sure you want to delete <strong>{displayText}</strong>?
          </p>
          <p className="warning-subtext">
            This action cannot be undone.
          </p>
        </div>
        <div className="modal-footer">
          <button className="modal-button cancel-button" onClick={onClose} disabled={isLoading}>
            Cancel
          </button>
          <button className="modal-button delete-confirm-button" onClick={handleConfirm} disabled={isLoading}>
            {isLoading ? 'Deleting...' : 'Delete'}
          </button>
        </div>
      </div>
    </div>
  );
}
