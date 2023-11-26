export interface DashboardApi {
    background: string;
    dashboardDevices: DashboardDeviceApi[];
}

export interface DashboardDeviceApi {
    deviceId: string;
    posX: number;
    posY: number;
    sourceType: string;
}