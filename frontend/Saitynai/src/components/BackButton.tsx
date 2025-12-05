interface BackButtonProps {
  label: string;
  onClick: () => void;
}

export function BackButton({ label, onClick }: BackButtonProps) {
  return (
    <div style={{ marginBottom: '1rem' }}>
      <button 
        onClick={onClick} 
        style={{ 
          padding: '0.5rem 1rem', 
          backgroundColor: '#6c757d', 
          color: 'white', 
          border: 'none', 
          borderRadius: '4px', 
          cursor: 'pointer',
          fontSize: '1rem',
          transition: 'background-color 0.2s'
        }}
        onMouseOver={(e) => e.currentTarget.style.backgroundColor = '#5a6268'}
        onMouseOut={(e) => e.currentTarget.style.backgroundColor = '#6c757d'}
      >
        ← {label}
      </button>
    </div>
  );
}
