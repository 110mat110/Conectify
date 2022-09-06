import { Metadata } from "./metadata";
import { Thing } from "./thing";

export interface Sensor{
    id: string,
    sensorName: string,
    metadata: Metadata[],
    sourceThingId: string,
    sourceThing: Thing,
}