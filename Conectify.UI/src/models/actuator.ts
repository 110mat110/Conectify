import { Metadata } from "./metadata";

export interface Actuator {
    id: string;
    name: string;
    sourceDeviceId: string;
    metadata: Metadata[],
    sensorId: string;
}

export interface CreateActuatorApi{
    actuatorName: string;
}