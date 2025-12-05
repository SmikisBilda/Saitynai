export interface AccessPoint {
  id: number;
  scanId: number;
  ssid: string | null;
  bssid: string | null;
  capabilities: string | null;
  centerfreq0: number | null;
  centerfreq1: number | null;
  frequency: number | null;
  level: number;
}

export interface Building {
  id: number;
  address: string | null;
  name: string | null;
}

export interface Floor {
  id: number;
  buildingId: number;
  floorNumber: number;
  floorPlanPath: string | null;
}

export interface Point {
  id: number;
  floorId: number;
  latitude: number;
  longitude: number;
  apCount: number;
}

export interface Scan {
  id: number;
  pointId: number;
  scannedAt: string;
  filters: string | null;
  apCount: number;
}

export interface LoginDto {
  username: string;
  password: string;
}

export interface RegisterDto {
  username: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

export interface RefreshTokenDto {
  refreshToken: string;
}

export interface LogoutDto {
  refreshToken: string;
}

export interface User {
  id: number;
  username: string;
  email: string;
  passwordHash?: string;
  createdAt: string;
  roles: RoleDto[];
}

export interface Role {
  id: number;
  name: string;
}

export interface Permission {
  id: number;
  name: string;
}

export interface UserRole {
  userId: number;
  roleId: number;
  username?: string;
  roleName?: string;
}

export interface RolePermission {
  roleId: number;
  permissionId: number;
  resourceTypeId: number;
  resourceId: number;
  allow: boolean;
  cascade: boolean;
  roleName?: string;
  permissionName?: string;
  resourceTypeName?: string;
}

export interface CreateRoleDto {
  name: string;
}

export interface CreatePermissionDto {
  name: string;
}

export interface AssignRoleDto {
  userId: number;
  roleId: number;
}

export interface AssignPermissionDto {
  roleId: number;
  permissionId: number;
  resourceType: string;
  resourceId: number;
  allow: boolean;
  cascade: boolean;
}

export interface CreateAccessPointDto {
  scanId: number;
  ssid?: string;
  bssid?: string;
  capabilities?: string;
  centerfreq0?: number;
  centerfreq1?: number;
  frequency?: number;
  level: number;
}

export interface UpdateAccessPointDto {
  scanId?: number;
  ssid?: string;
  bssid?: string;
  capabilities?: string;
  centerfreq0?: number;
  centerfreq1?: number;
  frequency?: number;
  level?: number;
}

export interface CreateBuildingDto {
  address?: string;
  name?: string;
}

export interface UpdateBuildingDto {
  address?: string;
  name?: string;
}

export interface CreateFloorDto {
  buildingId: number;
  floorNumber: number;
  floorPlanPath?: string;
}

export interface UpdateFloorDto {
  buildingId?: number;
  floorNumber?: number;
  floorPlanPath?: string;
}

export interface CreatePointDto {
  floorId: number;
  latitude: number;
  longitude: number;
  apCount: number;
}

export interface UpdatePointDto {
  floorId?: number;
  latitude?: number;
  longitude?: number;
  apCount?: number;
}

export interface CreateScanDto {
  pointId: number;
  scannedAt: string;
  filters?: string;
  apCount: number;
}

export interface UpdateScanDto {
  pointId?: number;
  scannedAt?: string;
  filters?: string;
  apCount?: number;
}

export interface UserPermissionDto {
  permissionName: string;
  resourceType: string;
  resourceId: number;
  allow: boolean;
  cascade: boolean;
}

export interface RoleDto {
  id: number;
  name: string;
}

export interface UserPermissionsResponseDto {
  roles: RoleDto[];
  permissions: UserPermissionDto[];
}

export interface PermissionScopeDto {
  permissionName: string;
  resourceType: string;
  allowIds: number[];
  cascadeFrom: Record<string, number[]>;
}
