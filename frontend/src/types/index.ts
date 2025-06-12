export interface Room {
    id: number;
    name: string;
    description?: string;
    floor?: string;
    maxOccupancy?: number;
    co2Readings: Co2Reading[];
}

export interface Co2Reading {
    id: number;
    roomId: number;
    ppm: number;
    timestamp: string;
    room?: Room;
}

export interface Co2ReadingStats {
    averagePpm: number;
    minPpm: number;
    maxPpm: number;
    totalReadings: number;
    startDate: string;
    endDate: string;
}

export interface RoomStats {
    roomId: number;
    roomName: string;
    averagePpm: number;
    minPpm: number;
    maxPpm: number;
    totalReadings: number;
    startDate: string;
    endDate: string;
}

export interface ApiResponse<T> {
    data: T;
    success: boolean;
    message?: string;
} 