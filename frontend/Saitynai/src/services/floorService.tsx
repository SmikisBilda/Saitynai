import { apiClient } from './api';
import type { Floor, CreateFloorDto, UpdateFloorDto } from '../types/api';
import type { AxiosResponse } from 'axios';

export const getFloors = (): Promise<AxiosResponse<Floor[]>> => apiClient.get('/floor');
export const getFloor = (id: number): Promise<AxiosResponse<Floor>> => apiClient.get(`/floor/${id}`);
export const createFloor = (data: CreateFloorDto): Promise<AxiosResponse<Floor>> => apiClient.post('/floor', data);
export const updateFloor = (id: number, data: UpdateFloorDto): Promise<AxiosResponse<Floor>> => apiClient.put(`/floor/${id}`, data);
export const deleteFloor = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/floor/${id}`);
export const uploadFloorPlan = (id: number, file: File): Promise<AxiosResponse<Floor>> => {
  const formData = new FormData();
  formData.append('file', file);
  return apiClient.post(`/floor/${id}/upload-plan`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
};