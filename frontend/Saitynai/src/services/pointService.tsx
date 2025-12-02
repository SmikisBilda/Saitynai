import { apiClient } from './api';
import type { Point, CreatePointDto, UpdatePointDto } from '../types/api';
import type { AxiosResponse } from 'axios';

export const getPoints = (): Promise<AxiosResponse<Point[]>> => apiClient.get('/points');
export const getPoint = (id: number): Promise<AxiosResponse<Point>> => apiClient.get(`/points/${id}`);
export const createPoint = (data: CreatePointDto): Promise<AxiosResponse<Point>> => apiClient.post('/points', data);
export const updatePoint = (id: number, data: UpdatePointDto): Promise<AxiosResponse<Point>> => apiClient.put(`/points/${id}`, data);
export const deletePoint = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/points/${id}`);
