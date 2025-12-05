import { Routes, Route, Navigate } from 'react-router-dom';
import './App.css';
import Header from './components/Header';
import Footer from './components/Footer';
import LoginPage from './pages/LoginPage';
import BuildingsPage from './pages/BuildingsPage';
import FloorsPage from './pages/FloorsPage';
import PointsPage from './pages/PointsPage';
import ScansPage from './pages/ScansPage';
import AccessPointsPage from './pages/AccessPointsPage';
import RegisterUserPage from './pages/RegisterUserPage';
import PermissionsPage from './pages/PermissionsPage';
import UsersPage from './pages/permissions/UsersPage';
import RolePermissionsPage from './pages/permissions/RolePermissionsPage';
import UserRolesPage from './pages/permissions/UserRolesPage';
import { ProtectedRoute } from './components/ProtectedRoute';
import { useAuth } from './contexts/AuthContext';

function App() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <div className="app-container">
      {isAuthenticated && <Header />}
      <main className="main-content">
        <Routes>
          <Route 
            path="/login" 
            element={
              isAuthenticated ? <Navigate to="/buildings" replace /> : <LoginPage />
            } 
          />
          
          <Route 
            path="/" 
            element={
              isAuthenticated ? <Navigate to="/buildings" replace /> : <Navigate to="/login" replace />
            } 
          />

          <Route
            path="/buildings"
            element={
              <ProtectedRoute>
                <BuildingsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/buildings/:buildingId/floors"
            element={
              <ProtectedRoute>
                <FloorsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/floors/:floorId/points"
            element={
              <ProtectedRoute>
                <PointsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/points/:pointId/scans"
            element={
              <ProtectedRoute>
                <ScansPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/scans/:scanId/access-points"
            element={
              <ProtectedRoute>
                <AccessPointsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/register-user"
            element={
              <ProtectedRoute>
                <RegisterUserPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/permissions"
            element={
              <ProtectedRoute requireRole="Admin">
                <PermissionsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/permissions/users"
            element={
              <ProtectedRoute requireRole="Admin">
                <UsersPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/permissions/role-permissions"
            element={
              <ProtectedRoute requireRole="Admin">
                <RolePermissionsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/permissions/user-roles"
            element={
              <ProtectedRoute requireRole="Admin">
                <UserRolesPage />
              </ProtectedRoute>
            }
          />
        </Routes>
      </main>
      {isAuthenticated && <Footer />}
    </div>
  );
}

export default App;
