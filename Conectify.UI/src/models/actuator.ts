import { Metadata } from "./metadata";

export interface Actuator {
    id: string;
    name: string;
    metadata: Metadata[];
    sourceDeviceId: string;
    sensorId: string;
}