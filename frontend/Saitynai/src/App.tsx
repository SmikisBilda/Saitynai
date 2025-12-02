import { useState } from 'react';
import './App.css';
import Footer from './components/Footer';
import ErrorPopup from './components/ErrorPopup';

function App() {
  const [error, setError] = useState<string | null>(null);

  const triggerError = () => {
    setError('Something went wrong. Please try again.');
  };

  return (
    <>
      <div className="app-container">
        <h1>My App</h1>
        <button onClick={triggerError}>Trigger Error</button>
      </div>

      {error && (
        <ErrorPopup
          message={error}
          onClose={() => setError(null)}
        />
      )}

      <Footer />
    </>
  );
}

export default App;
