import './style/BackButton.css';

interface BackButtonProps {
  label: string;
  onClick: () => void;
}

export function BackButton({ label, onClick }: BackButtonProps) {
  return (
    <div className="back-button-container">
      <button className="back-button" onClick={onClick}>
        <span className="back-icon">←</span>
        <span className="back-label">{label}</span>
      </button>
    </div>
  );
}
