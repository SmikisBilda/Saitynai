import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import type { User, UserPermissionDto, PermissionScopeDto, RoleDto } from '../types/api';
import { login as loginService, register as registerService, logout as logoutService, getCurrentUser } from '../services/authService';
import { getMyPermissions, getMyPermissionScopes } from '../services/permissionService';

interface AuthContextType {
  user: User | null;
  roles: RoleDto[];
  permissions: UserPermissionDto[];
  permissionScopes: PermissionScopeDto[];
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (username: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string) => Promise<void>;
  logout: () => void;
  hasPermission: (permissionName: string, resourceType?: string, resourceId?: number) => boolean;
  hasPermissionForId: (permissionName: string, resourceType: string, resourceId: number) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [permissions, setPermissions] = useState<UserPermissionDto[]>([]);
  const [permissionScopes, setPermissionScopes] = useState<PermissionScopeDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Check if user is logged in on mount
  useEffect(() => {
    const initAuth = async () => {
      const token = localStorage.getItem('accessToken');
      if (token) {
        try {
          // Fetch user info and permissions
          const [userResponse, permsResponse, scopesResponse] = await Promise.all([
            getCurrentUser(),
            getMyPermissions(),
            getMyPermissionScopes()
          ]);
          setUser(userResponse.data);
          setRoles(userResponse.data.roles);
          setPermissions(permsResponse.data.permissions);
          setPermissionScopes(scopesResponse.data);
        } catch (error) {
          console.error('Failed to initialize auth:', error);
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
        }
      }
      setIsLoading(false);
    };
    initAuth();
  }, []);

  const login = async (username: string, password: string) => {
    const response = await loginService({ username, password });
    localStorage.setItem('accessToken', response.data.accessToken);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    
    const [userResponse, permsResponse, scopesResponse] = await Promise.all([
      getCurrentUser(),
      getMyPermissions(),
      getMyPermissionScopes()
    ]);
    setUser(userResponse.data);
    setRoles(userResponse.data.roles);
    setPermissions(permsResponse.data.permissions);
    setPermissionScopes(scopesResponse.data);
  };

  const register = async (username: string, email: string, password: string) => {
    const response = await registerService({ username, email, password });
    localStorage.setItem('accessToken', response.data.accessToken);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    
    const [userResponse, permsResponse, scopesResponse] = await Promise.all([
      getCurrentUser(),
      getMyPermissions(),
      getMyPermissionScopes()
    ]);
    setUser(userResponse.data);
    setRoles(userResponse.data.roles);
    setPermissions(permsResponse.data.permissions);
    setPermissionScopes(scopesResponse.data);
  };

  const logout = async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      try {
        await logoutService({ refreshToken });
      } catch (error) {
        console.error('Logout API call failed:', error);
      }
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    setUser(null);
    setRoles([]);
    setPermissions([]);
  };

  const hasPermission = (permissionName: string, resourceType?: string, resourceId?: number) => {
    // If checking a specific resource, see if user has ANY permission for that resource type
    // Backend will enforce the actual cascading/hierarchy rules
    if (resourceType && resourceId !== undefined) {
      return permissions.some(p => 
        p.permissionName === permissionName &&
        p.resourceType === resourceType &&
        p.allow
      );
    }
    
    // For general checks (like "can create"), check for any allowed permission
    return permissions.some(p => {
      if (p.permissionName !== permissionName) return false;
      if (!p.allow) return false;
      if (!resourceType) return true;
      if (p.resourceType !== resourceType) return false;
      return true;
    });
  };

  // Precise check for a specific resource ID using scopes (direct allows only)
  const hasPermissionForId = (permissionName: string, resourceType: string, resourceId: number) => {
    const scope = permissionScopes.find(
      s => s.permissionName === permissionName && s.resourceType === resourceType
    );
    if (!scope) return false;
    return scope.allowIds.includes(resourceId);
  };

  return (
    <AuthContext.Provider value={{ 
      user, 
      roles,
      permissions,
      permissionScopes,
      isAuthenticated: !!user, 
      isLoading, 
      login, 
      register, 
      logout,
      hasPermission,
      hasPermissionForId
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
