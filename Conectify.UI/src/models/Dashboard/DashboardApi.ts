import { DashboardDeviceApi } from "./DashboardDevice";

export interface DashboardApi {
    id:string;
    name:string;
    background: string;
    type: number; // 0 = Basic, 1 = Grid
    dashboardDevices: DashboardDeviceApi[];
}