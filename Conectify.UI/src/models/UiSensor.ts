import { UiIcon, UiPoint, UiThreshold, UiValue } from './UiCube';

export interface UiSensor {
  id: string;
  deviceId: string;
  name: string;
  deviceName: string;
  showName: string | null;
  visible: boolean;
  latest: UiValue | null;
  values: UiPoint[];
  thresholds: UiThreshold[];
  icons: UiIcon[];
}
