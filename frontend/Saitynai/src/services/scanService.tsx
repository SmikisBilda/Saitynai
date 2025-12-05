import { apiClient } from './api';
import type { Scan, CreateScanDto, UpdateScanDto } from '../types/api';
import type { AxiosResponse } from 'axios';

export const getScans = (): Promise<AxiosResponse<Scan[]>> => apiClient.get('/scan');
export const getScan = (id: number): Promise<AxiosResponse<Scan>> => apiClient.get(`/scan/${id}`);
export const createScan = (data: CreateScanDto): Promise<AxiosResponse<Scan>> => apiClient.post('/scan', data);
export const updateScan = (id: number, data: UpdateScanDto): Promise<AxiosResponse<Scan>> => apiClient.put(`/scan/${id}`, data);
export const deleteScan = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/scan/${id}`);