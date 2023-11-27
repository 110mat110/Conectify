import { DashboardDeviceApi } from "./DashboardDevice";

export interface DashboardApi {
    id:string;
    name:string;
    background: string;
    dashboardDevices: DashboardDeviceApi[];
}