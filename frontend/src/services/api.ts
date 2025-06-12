import axios from 'axios';
import type { Co2Reading, Room } from '../types';

// Use relative URL in production (Docker) and absolute URL in development
const API_BASE_URL = process.env.NODE_ENV === 'production' ? '/api' : 'http://localhost:5000/api';

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

export interface PaginatedResponse<T> {
    data: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export interface Co2ReadingFilter {
    startDate?: string;
    endDate?: string;
    minPpm?: number;
    maxPpm?: number;
    roomId?: number;
    sortBy?: string;
    sortDescending?: boolean;
    page?: number;
    pageSize?: number;
}

export interface RoomFilter {
    floor?: string;
    minOccupancy?: number;
    maxOccupancy?: number;
    sortBy?: string;
    sortDescending?: boolean;
    page?: number;
    pageSize?: number;
}

export const co2Api = {
    getReadings: async (filter: Co2ReadingFilter = {}): Promise<PaginatedResponse<Co2Reading>> => {
        const response = await api.get('/co2readings', { params: filter });
        return response.data;
    },

    getReading: async (id: number): Promise<Co2Reading> => {
        const response = await api.get(`/co2readings/${id}`);
        return response.data;
    },

    addReading: async (reading: Omit<Co2Reading, 'id'>): Promise<Co2Reading> => {
        const response = await api.post('/co2readings', reading);
        return response.data;
    },

    updateReading: async (id: number, reading: Partial<Co2Reading>): Promise<void> => {
        await api.put(`/co2readings/${id}`, reading);
    },

    deleteReading: async (id: number): Promise<void> => {
        await api.delete(`/co2readings/${id}`);
    },

    getStats: async (startDate: string, endDate: string): Promise<any> => {
        const response = await api.get('/co2readings/stats', {
            params: { startDate, endDate }
        });
        return response.data;
    }
};

export const roomApi = {
    getRooms: async (filter: RoomFilter = {}): Promise<PaginatedResponse<Room>> => {
        const response = await api.get('/rooms', { params: filter });
        return response.data;
    },

    getRoom: async (id: number): Promise<Room> => {
        const response = await api.get(`/rooms/${id}`);
        return response.data;
    },

    addRoom: async (room: Omit<Room, 'id' | 'co2Readings'>): Promise<Room> => {
        const response = await api.post('/rooms', room);
        return response.data;
    },

    updateRoom: async (id: number, room: Partial<Room>): Promise<void> => {
        await api.put(`/rooms/${id}`, room);
    },

    deleteRoom: async (id: number): Promise<void> => {
        await api.delete(`/rooms/${id}`);
    },

    getRoomStats: async (id: number, startDate: string, endDate: string): Promise<any> => {
        const response = await api.get(`/rooms/${id}/stats`, {
            params: { startDate, endDate }
        });
        return response.data;
    }
}; 