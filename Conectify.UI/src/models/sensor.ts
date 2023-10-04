import { Metadata } from "./metadata";

export interface Sensor{
    id: string,
    name: string,
    metadata: Metadata[],
    sourceDeviceId: string
}