import { apiClient } from './api';
import type { LoginDto, RegisterDto, RefreshTokenDto, LogoutDto, AuthResponse, User } from '../types/api';
import type { AxiosResponse } from 'axios';

export const login = (data: LoginDto): Promise<AxiosResponse<AuthResponse>> => apiClient.post('/auth/login', data);
export const register = (data: RegisterDto): Promise<AxiosResponse<AuthResponse>> => apiClient.post('/auth/register', data);
export const refreshToken = (data: RefreshTokenDto): Promise<AxiosResponse<AuthResponse>> => apiClient.post('/auth/refresh', data);
export const logout = (data: LogoutDto): Promise<AxiosResponse<string>> => apiClient.post('/auth/logout', data);
export const getCurrentUser = (): Promise<AxiosResponse<User>> => apiClient.get('/auth/me');
