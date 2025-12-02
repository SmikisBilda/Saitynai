import { apiClient } from './api';
import type { AccessPoint, CreateAccessPointDto, UpdateAccessPointDto } from '../types/api';
import type { AxiosResponse } from 'axios';

export const getAccessPoints = (): Promise<AxiosResponse<AccessPoint[]>> => apiClient.get('/accesspoints');
export const getAccessPoint = (id: number): Promise<AxiosResponse<AccessPoint>> => apiClient.get(`/accesspoints/${id}`);
export const createAccessPoint = (data: CreateAccessPointDto): Promise<AxiosResponse<AccessPoint>> => apiClient.post('/accesspoints', data);
export const updateAccessPoint = (id: number, data: UpdateAccessPointDto): Promise<AxiosResponse<AccessPoint>> => apiClient.put(`/accesspoints/${id}`, data);
export const deleteAccessPoint = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/accesspoints/${id}`);
