import { apiClient } from './api';
import type { Building, CreateBuildingDto, UpdateBuildingDto } from '../types/api';
import type { AxiosResponse } from 'axios';

export const getBuildings = (): Promise<AxiosResponse<Building[]>> => apiClient.get('/building');
export const getBuilding = (id: number): Promise<AxiosResponse<Building>> => apiClient.get(`/building/${id}`);
export const createBuilding = (data: CreateBuildingDto): Promise<AxiosResponse<Building>> => apiClient.post('/building', data);
export const updateBuilding = (id: number, data: UpdateBuildingDto): Promise<AxiosResponse<Building>> => apiClient.put(`/building/${id}`, data);
export const deleteBuilding = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/building/${id}`);