import React from 'react';
import './style/ErrorPopup.css';

type ErrorPopupProps = {
  message: string;
  onClose: () => void;
};

const ErrorPopup: React.FC<ErrorPopupProps> = ({ message, onClose }) => {
  return (
    <div className="error-popup-backdrop" onClick={onClose}>
      <div className="error-popup" onClick={(e) => e.stopPropagation()}>
        <h2>Error</h2>
        <p>{message}</p>
        <button onClick={onClose}>Close</button>
      </div>
    </div>
  );
};

export default ErrorPopup;
