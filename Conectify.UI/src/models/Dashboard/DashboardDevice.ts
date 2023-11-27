export interface DashboardDeviceApi {
    id: string;
    deviceId: string;
    posX: number;
    posY: number;
    sourceType: string;
}

export interface AddDashboardDeviceApi {
    deviceId: string;
    posX: number;
    posY: number;
    sourceType: string;
}