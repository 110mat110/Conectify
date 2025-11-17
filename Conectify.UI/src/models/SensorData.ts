import { BaseInputType } from "src/models/extendedValue";
import { Metadata } from "src/models/metadata";
import { Sensor } from "src/models/sensor";

export interface SensorData {
  id: string;
  sensor: Sensor;
  metadatas: Metadata[];
  latestVal?: BaseInputType;
}