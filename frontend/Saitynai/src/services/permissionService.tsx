import { apiClient } from './api';
import type { Role, Permission, UserRole, RolePermission, User, CreateRoleDto, CreatePermissionDto, AssignRoleDto, AssignPermissionDto } from '../types/api';
import type { AxiosResponse } from 'axios';

// Role
export const getRoles = (): Promise<AxiosResponse<Role[]>> => apiClient.get('/permission/roles');
export const createRole = (data: CreateRoleDto): Promise<AxiosResponse<Role>> => apiClient.post('/permission/roles', data);
export const deleteRole = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/permission/roles/${id}`);

// Permission
export const getPermissions = (): Promise<AxiosResponse<Permission[]>> => apiClient.get('/permission/permissions');
export const createPermission = (data: CreatePermissionDto): Promise<AxiosResponse<Permission>> => apiClient.post('/permission/permissions', data);
export const deletePermission = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/permission/permissions/${id}`);

// UserRole
export const getUserRoles = (): Promise<AxiosResponse<UserRole[]>> => apiClient.get('/permission/user-roles');
export const assignRoleToUser = (data: AssignRoleDto): Promise<AxiosResponse<string>> => apiClient.post('/permission/user-roles', data);
export const deleteUserRole = (userId: number, roleId: number): Promise<AxiosResponse<void>> => apiClient.delete(`/permission/user-roles/${userId}/${roleId}`);

// RolePermission
export const getRolePermissions = (): Promise<AxiosResponse<RolePermission[]>> => apiClient.get('/permission/role-permissions');
export const assignPermissionToRole = (data: AssignPermissionDto): Promise<AxiosResponse<string>> => apiClient.post('/permission/role-permissions', data);
export const deleteRolePermission = (roleId: number, permissionId: number, resourceType: string, resourceId: number): Promise<AxiosResponse<void>> =>
  apiClient.delete(`/permission/role-permissions/${roleId}/${permissionId}/${resourceType}/${resourceId}`);

// User
export const getUsers = (): Promise<AxiosResponse<User[]>> => apiClient.get('/permission/users');
export const deleteUser = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/permission/users/${id}`);