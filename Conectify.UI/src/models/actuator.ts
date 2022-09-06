import { Metadata } from "./metadata";
import { Sensor } from "./sensor";
import { Thing } from "./thing";

export interface Actuator {
    id: string;
    name: string;
    metadata: Metadata[];
    sourceThingId: string;
    sourceThing: Thing;
    sensor: Sensor;
    sensorId: string;
}