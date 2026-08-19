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
    /** Function the created input rule should land in, empty for the top level board. */
    functionId?: string;
}