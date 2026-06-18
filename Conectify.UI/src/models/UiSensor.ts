import { Metadata } from './metadata';
import { BaseInputType } from './extendedValue';

export interface UiSensor {
  id: string;
  name: string;
  sourceDeviceId: string;
  deviceName: string;
  metadata: Metadata[];
  latestValue: BaseInputType | null;
}
