import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requirePermission?: string;
  requireRole?: string;
  resourceType?: string;
  resourceId?: number;
  allowGuest?: boolean;
}

export function ProtectedRoute({ 
  children, 
  requirePermission,
  requireRole,
  resourceType,
  resourceId,
  allowGuest = false
}: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, hasPermission, roles } = useAuth();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!isAuthenticated) {
    if (allowGuest) {
      return <>{children}</>;
    }
    return <Navigate to="/login" replace />;
  }

  if (requireRole && !roles.some(r => r.name === requireRole)) {
    return <Navigate to="/unauthorized" replace />;
  }

  if (requirePermission && !hasPermission(requirePermission, resourceType, resourceId)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
}
