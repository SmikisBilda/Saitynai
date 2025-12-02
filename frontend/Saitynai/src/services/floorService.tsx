import { apiClient } from './api';
import type { Floor, CreateFloorDto, UpdateFloorDto } from '../types/api';
import type { AxiosResponse } from 'axios';

export const getFloors = (): Promise<AxiosResponse<Floor[]>> => apiClient.get('/floors');
export const getFloor = (id: number): Promise<AxiosResponse<Floor>> => apiClient.get(`/floors/${id}`);
export const createFloor = (data: CreateFloorDto): Promise<AxiosResponse<Floor>> => apiClient.post('/floors', data);
export const updateFloor = (id: number, data: UpdateFloorDto): Promise<AxiosResponse<Floor>> => apiClient.put(`/floors/${id}`, data);
export const deleteFloor = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/floors/${id}`);
