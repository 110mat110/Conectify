import { Metadata } from "./metadata";

export interface Device {
    id: string;
    ipAdress: string;
    macAdress: string;
    name: string;
    state: number;
    metadata: Metadata[] | undefined,
}

export interface DeviceSelector{
    id: string;
    name: string;
    type: number;
}