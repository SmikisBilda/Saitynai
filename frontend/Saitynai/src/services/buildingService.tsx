import { apiClient } from './api';
import type { Building, CreateBuildingDto, UpdateBuildingDto } from '../types/api';
import type { AxiosResponse } from 'axios';

export const getBuildings = (): Promise<AxiosResponse<Building[]>> => apiClient.get('/buildings');
export const getBuilding = (id: number): Promise<AxiosResponse<Building>> => apiClient.get(`/buildings/${id}`);
export const createBuilding = (data: CreateBuildingDto): Promise<AxiosResponse<Building>> => apiClient.post('/buildings', data);
export const updateBuilding = (id: number, data: UpdateBuildingDto): Promise<AxiosResponse<Building>> => apiClient.put(`/buildings/${id}`, data);
export const deleteBuilding = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/buildings/${id}`);
