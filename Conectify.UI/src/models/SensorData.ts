import { Sensor } from "src/models/sensor";
import { UiValue } from "src/models/UiCube";

export interface SensorData {
  id: string;
  sensor: Sensor;
  latestVal?: UiValue;
}
