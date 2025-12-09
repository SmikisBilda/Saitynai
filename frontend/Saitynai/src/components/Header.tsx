import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useState } from 'react';
import './style/Header.css';

const Header = () => {
  const navigate = useNavigate();
  const { user, roles, isAuthenticated, logout } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);

  const isAdmin = roles.some(role => role.name === 'Admin');

  const handleLogout = async () => {
    await logout();
    navigate('/login');
    setMenuOpen(false);
  };

  const handleNavigation = (path: string) => {
    navigate(path);
    setMenuOpen(false);
  };

  return (
    <header className="header">
      <div className="header-content">
        <h1 className="header-title" onClick={() => handleNavigation('/')}>Saitynai</h1>
        
        <button 
          className="menu-toggle"
          onClick={() => setMenuOpen(!menuOpen)}
          aria-label="Toggle menu"
        >
          <span className="hamburger"></span>
        </button>

        <nav className={`header-nav ${menuOpen ? 'open' : ''}`}>
          <button 
            className="nav-item"
            onClick={() => handleNavigation('/buildings')}
          >
            Buildings
          </button>
          {isAuthenticated && isAdmin && (
            <>
              <button 
                className="nav-item"
                onClick={() => handleNavigation('/permissions')}
              >
                Permissions
              </button>
              <button 
                className="nav-item"
                onClick={() => handleNavigation('/register-user')}
              >
                Register User
              </button>
            </>
          )}

          <div className="header-actions">
            {isAuthenticated ? (
              <>
                <span className="header-user">Welcome, {user?.username}</span>
                <button className="header-button logout-button" onClick={handleLogout}>
                  Logout
                </button>
              </>
            ) : (
              <button className="header-button login-button" onClick={() => handleNavigation('/login')}>
                Login
              </button>
            )}
          </div>
        </nav>
      </div>
    </header>
  );
};

export default Header;
