import { apiClient } from './api';
import type { Point, CreatePointDto, UpdatePointDto } from '../types/api';
import type { AxiosResponse } from 'axios';

export const getPoints = (): Promise<AxiosResponse<Point[]>> => apiClient.get('/point');
export const getPoint = (id: number): Promise<AxiosResponse<Point>> => apiClient.get(`/point/${id}`);
export const createPoint = (data: CreatePointDto): Promise<AxiosResponse<Point>> => apiClient.post('/point', data);
export const updatePoint = (id: number, data: UpdatePointDto): Promise<AxiosResponse<Point>> => apiClient.put(`/point/${id}`, data);
export const deletePoint = (id: number): Promise<AxiosResponse<void>> => apiClient.delete(`/point/${id}`);