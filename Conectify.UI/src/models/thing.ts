export interface Device {
    id: string;
    ipAdress: string;
    macAdress: string;
    name: string;
    state: number;
}

export interface DeviceSelector{
    id: string;
    name: string;
    type: number;
}