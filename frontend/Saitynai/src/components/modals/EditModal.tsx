import { useState, useEffect } from 'react';
import './style/EditModal.css';

interface FieldConfig {
  key: string;
  label: string;
  type?: 'text' | 'number' | 'checkbox' | 'select' | 'file';
  editable?: boolean;
  options?: { value: string | number; label: string }[];
}

interface EditModalProps<T> {
  item: T | null;
  onSave: (item: T) => Promise<void> | void;
  onClose: () => void;
  title?: string;
  fields?: FieldConfig[];
}

export function EditModal<T extends Record<string, any>>({ 
  item, 
  onSave, 
  onClose,
  title = 'Edit Item',
  fields
}: EditModalProps<T>) {
  const [formData, setFormData] = useState<T | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (item) {
      setFormData({ ...item });
    }
  }, [item]);

  if (!item || !formData) return null;

  const handleChange = (key: keyof T, value: any) => {
    setFormData(prev => prev ? { ...prev, [key]: value } : null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (formData) {
      setError(null);
      setIsLoading(true);
      try {
        await onSave(formData);
      } catch (err: any) {
        const errorMessage = err.response?.data?.message || err.response?.data || err.message || 'Failed to save changes';
        setError(errorMessage);
      } finally {
        setIsLoading(false);
      }
    }
  };

  const renderInput = (key: keyof T, value: any, config?: FieldConfig) => {
    const stringKey = String(key);
    const isEditable = config?.editable !== false;
    
    // If config is provided, use it to determine type
    if (config) {
      if (!isEditable) {
        return (
          <input
            type="text"
            value={value ?? ''}
            disabled
            className="form-input disabled"
          />
        );
      }

      if (config.type === 'select' && config.options) {
        return (
          <select
            value={String(value)}
            onChange={(e) => handleChange(key, isNaN(Number(e.target.value)) ? e.target.value : Number(e.target.value))}
            className="form-input"
          >
            <option value="">Select...</option>
            {config.options.map(opt => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        );
      }

      if (config.type === 'checkbox') {
        return (
          <select
            value={String(value)}
            onChange={(e) => handleChange(key, e.target.value === 'true')}
            className="form-input"
          >
            <option value="true">Yes</option>
            <option value="false">No</option>
          </select>
        );
      }

      if (config.type === 'number') {
        return (
          <input
            type="number"
            value={value ?? ''}
            onChange={(e) => handleChange(key, parseFloat(e.target.value) || 0)}
            className="form-input"
          />
        );
      }
    }

    // Default behavior if no config or fallthrough
    
    // Skip non-editable fields (default logic)
    if (!config && (stringKey === 'id' || stringKey.toLowerCase().includes('id') && typeof value === 'number')) {
      return (
        <input
          type="text"
          value={value ?? ''}
          disabled
          className="form-input disabled"
        />
      );
    }

    // Handle different types
    if (typeof value === 'boolean') {
      return (
        <select
          value={String(value)}
          onChange={(e) => handleChange(key, e.target.value === 'true')}
          className="form-input"
        >
          <option value="true">Yes</option>
          <option value="false">No</option>
        </select>
      );
    }

    if (typeof value === 'number') {
      return (
        <input
          type="number"
          value={value ?? ''}
          onChange={(e) => handleChange(key, parseFloat(e.target.value) || 0)}
          className="form-input"
        />
      );
    }

    // Special case: file upload for known path fields
    if (typeof value === 'string' && (stringKey.toLowerCase().includes('planpath') || stringKey.toLowerCase().endsWith('path'))) {
      return (
        <div className="file-upload-group">
          {value ? (
            <div className="file-current">
              <span className="file-label">Current:</span>
              <span className="file-value">{value}</span>
            </div>
          ) : (
            <div className="file-current empty">No file</div>
          )}
          <input
            type="file"
            accept="image/*"
            onChange={(e) => {
              const file = e.target.files?.[0] || null;
              // Attach selected file on a conventional key for handling upstream
              setFormData(prev => prev ? { ...prev, [stringKey]: value, [(`${stringKey}File`) as keyof T]: file } : null);
            }}
            className="form-input"
          />
        </div>
      );
    }

    // Default to text input
    return (
      <input
        type="text"
        value={value ?? ''}
        onChange={(e) => handleChange(key, e.target.value)}
        className="form-input"
      />
    );
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content edit-modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>{title}</h3>
          <button className="modal-close" onClick={onClose}>
            ×
          </button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="modal-body">
            {error && (
              <div className="error-message">
                <span className="error-icon">⚠️</span>
                <span>{error}</span>
              </div>
            )}
            {fields ? (
              fields.map(field => (
                <div key={field.key} className="form-group">
                  <label className="form-label">{field.label}:</label>
                  {renderInput(field.key as keyof T, formData[field.key as keyof T], field)}
                </div>
              ))
            ) : (
              Object.entries(formData).map(([key, value]) => (
                <div key={key} className="form-group">
                  <label className="form-label">{key}:</label>
                  {renderInput(key as keyof T, value)}
                </div>
              ))
            )}
          </div>
          <div className="modal-footer">
            <button type="button" className="modal-button cancel-button" onClick={onClose} disabled={isLoading}>
              Cancel
            </button>
            <button type="submit" className="modal-button save-button" disabled={isLoading}>
              {isLoading ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
