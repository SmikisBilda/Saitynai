import { useState } from 'react';
import type { Floor } from '../../types/api';
import './style/EditModal.css';

interface FloorPlanUploadModalProps {
  floor: Floor | null;
  onUpload: (file: File) => Promise<void>;
  onClose: () => void;
}

export function FloorPlanUploadModal({ floor, onUpload, onClose }: FloorPlanUploadModalProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  if (!floor) return null;

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file type
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/svg+xml'];
    if (!validTypes.includes(file.type)) {
      setError('Please select a valid image file (JPG, PNG, GIF, or SVG)');
      return;
    }

    // Validate file size (max 10MB)
    if (file.size > 10 * 1024 * 1024) {
      setError('File size must be less than 10MB');
      return;
    }

    setError(null);
    setSelectedFile(file);

    // Create preview
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreview(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    setError(null);
    setIsUploading(true);
    try {
      await onUpload(selectedFile);
      onClose();
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.response?.data || err.message || 'Failed to upload file';
      setError(errorMessage);
    } finally {
      setIsUploading(false);
    }
  };

  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
  const currentImageUrl = floor.floorPlanPath ? `${apiBaseUrl}${floor.floorPlanPath}` : null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content edit-modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>Upload Floor Plan - Floor #{floor.id}</h3>
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

          {currentImageUrl && !preview && (
            <div style={{ marginBottom: '1rem' }}>
              <label className="form-label">Current Floor Plan:</label>
              <div style={{ border: '1px solid #ddd', padding: '0.5rem', borderRadius: '4px' }}>
                <img 
                  src={currentImageUrl} 
                  alt="Current floor plan" 
                  style={{ maxWidth: '100%', maxHeight: '300px', objectFit: 'contain' }}
                  onError={(e) => {
                    (e.target as HTMLImageElement).style.display = 'none';
                  }}
                />
              </div>
            </div>
          )}

          <div className="form-group">
            <label className="form-label">Select New Floor Plan:</label>
            <input
              type="file"
              accept="image/jpeg,image/jpg,image/png,image/gif,image/svg+xml"
              onChange={handleFileSelect}
              className="form-input"
              disabled={isUploading}
            />
            <small style={{ color: '#666', fontSize: '0.85rem', marginTop: '0.25rem', display: 'block' }}>
              Accepted formats: JPG, PNG, GIF, SVG (max 10MB)
            </small>
          </div>

          {preview && (
            <div className="form-group">
              <label className="form-label">Preview:</label>
              <div style={{ border: '1px solid #ddd', padding: '0.5rem', borderRadius: '4px' }}>
                <img 
                  src={preview} 
                  alt="Preview" 
                  style={{ maxWidth: '100%', maxHeight: '300px', objectFit: 'contain' }}
                />
              </div>
            </div>
          )}
        </div>
        <div className="modal-footer">
          <button 
            type="button" 
            className="modal-button cancel-button" 
            onClick={onClose}
            disabled={isUploading}
          >
            Cancel
          </button>
          <button 
            type="button" 
            className="modal-button save-button" 
            onClick={handleUpload}
            disabled={!selectedFile || isUploading}
          >
            {isUploading ? 'Uploading...' : 'Upload'}
          </button>
        </div>
      </div>
    </div>
  );
}
