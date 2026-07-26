import { UiIcon, UiValue } from './UiCube';

export interface UiActuator {
  id: string;
  deviceId: string;
  sensorId: string;
  name: string;
  deviceName: string;
  showName: string | null;
  visible: boolean;
  latest: UiValue | null;
  ioType: number;
  minValue: number;
  maxValue: number;
  metadataValue: number;
  icons: UiIcon[];
}
