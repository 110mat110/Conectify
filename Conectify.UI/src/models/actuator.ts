export interface Actuator {
    id: string;
    name: string;
    sourceDeviceId: string;
    sensorId: string;
}

export interface CreateActuatorApi{
    actuatorName: string;
}