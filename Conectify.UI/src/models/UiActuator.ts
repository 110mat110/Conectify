import { Metadata } from './metadata';
import { BaseInputType } from './extendedValue';

export interface UiActuator {
  id: string;
  name: string;
  sourceDeviceId: string;
  sensorId: string;
  deviceName: string;
  metadata: Metadata[];
  latestValue: BaseInputType | null;
}
